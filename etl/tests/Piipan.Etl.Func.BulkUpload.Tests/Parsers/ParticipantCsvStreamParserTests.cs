using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using Piipan.Etl.Func.BulkUpload.Models;
using Piipan.Etl.Func.BulkUpload.Parsers;
using Piipan.Shared.API.Utilities;
using Xunit;

namespace Piipan.Etl.Func.BulkUpload.Tests.Parsers
{
    public class ParticipantCsvStreamParserTests
    {
        private const string LDS_HASH = "04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef";

        private Stream CsvFixture(string[] records, bool includeHeader = true, bool requiredOnly = false, bool isLegacy = false)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            if (isLegacy)
            {
                writer.WriteLine("lds_hash,case_id,participant_id,benefits_end_month,recent_benefit_issuance_dates,vulnerable_individual");
            }
            else
            {
                if (includeHeader)
                {
                    if (requiredOnly)
                    {
                        writer.WriteLine("lds_hash,case_id,participant_id");
                    }
                    else
                    {
                        writer.WriteLine("lds_hash,case_id,participant_id,participant_closing_date,recent_benefit_issuance_dates,vulnerable_individual");
                    }
                }
            }
            foreach (var record in records)
            {
                writer.WriteLine(record);
            }
            writer.Flush();
            stream.Position = 0;

            return stream;
        }

        [Fact]
        public void ReadAllFields()
        {
            // Arrange
            var stream = CsvFixture(new string[] {
                $"{LDS_HASH},CaseId,ParticipantId,2020-10-10,2021-05-01/2021-05-02 2021-06-01/2021-06-02,true"
            });

            var parser = new ParticipantCsvStreamParser();

            // Act
            var records = parser.Parse(stream).ToList();
            // Assert
            Assert.Single(records);
            Assert.Single(records, r => r.LdsHash == LDS_HASH);
            Assert.Single(records, r => r.CaseId == "CaseId");
            Assert.Single(records, r => r.ParticipantId == "ParticipantId");
            Assert.Single(records, r => r.ParticipantClosingDate == new DateTime(2020, 10, 10));
            DateRange dateRange = records.First().RecentBenefitIssuanceDates.First();
            Assert.Equal(dateRange, new DateRange(new DateTime(2021, 05, 01), new DateTime(2021, 05, 02)));
            Assert.Single(records, r => r.VulnerableIndividual == true);
        }

        [Fact]
        public void TestParticipantMapForCsvWriting()
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim
            };

            StringWriter stringWriter = new StringWriter();
            var csvwriter = new CsvWriter(stringWriter, config);
            csvwriter.Context.RegisterClassMap<ParticipantMap>();

            var state = "EA";

            var p = new Participant();

            var recId = 31;
            var padRecId = recId.ToString("00000000");
            p.LdsHash = "abc";

            p.CaseId = $"case-{state}-{padRecId}";
            p.ParticipantId = $"part-{state}-{padRecId}";
            p.ParticipantClosingDate = new DateTime(2022, 05, 09);

            var dr1 = new DateRange(new DateTime(2021, 04, 01), new DateTime(2021, 04, 15));
            var dr2 = new DateRange(new DateTime(2021, 03, 01), new DateTime(2021, 03, 30));
            var dr3 = new DateRange(new DateTime(2021, 02, 01), new DateTime(2021, 02, 28));
            p.RecentBenefitIssuanceDates = new List<DateRange>() { dr1, dr2, dr3 };
            p.VulnerableIndividual = true;

            csvwriter.WriteRecord(p);

            csvwriter.Flush();

            string expectedResult = "abc,case-EA-00000031,part-EA-00000031,2022-05-09,2021-04-01/2021-04-15 2021-03-01/2021-03-30 2021-02-01/2021-02-28,True";
            var result = stringWriter.ToString();
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ReadOptionalFields()
        {
            // Arrange
            var stream = CsvFixture(new string[] {
                $"{LDS_HASH},CaseId,ParticipantId,,,,"
            });
            var parser = new ParticipantCsvStreamParser();

            // Act
            var records = parser.Parse(stream).ToList();

            // Assert
            Assert.Single(records);
            Assert.Null(records.First().ParticipantClosingDate);
            Assert.Empty(records.First().RecentBenefitIssuanceDates);
            Assert.Null(records.First().VulnerableIndividual);
        }

        [Theory]
        [InlineData("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef,CaseId,,,")] // Missing ParticipantId
        [InlineData("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef,CaseId,ParticipantId,foobar,,")] // Malformed Participant closing date
        [InlineData("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef,CaseId,ParticipantId,,foo2bar,")] // Malformed Recent Benefit Months
        public void ExpectFieldValidationError(String inline)
        {
            // Arrange
            var stream = CsvFixture(new string[] { inline });
            var parser = new ParticipantCsvStreamParser();

            // Act
            var records = parser.Parse(stream);

            // Assert
            Assert.Throws<CsvHelper.FieldValidationException>(() =>
            {
                foreach (var record in records)
                {
                    ;
                }
            });
        }

        [Fact]
        public void BadCSVError()
        {
            // Arrange
            var stream = CsvFixture(new string[]
                {
                    $"{LDS_HASH},CaseId1,ParticipantId1,,,",
                    $"{LDS_HASH.Replace('0', '1')},CaseId2\",ParticipantId2,,,", // This line has a stray ", causing it to have bad data
                });
            var parser = new ParticipantCsvStreamParser();

            // Act
            var records = parser.Parse(stream);

            // Assert
            var ex = Assert.Throws<ReaderException>(() =>
            {
                foreach (var record in records)
                {
                    ;
                }
            });

            // Error on row 3 (header row + 2nd data row)
            Assert.Equal("Error parsing the CSV. Bad data found on row 3", ex.InnerException.Message);

        }

        [Theory]
        [InlineData("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef,CaseId,ParticipantId,,,foobar,")] // Malformed Protect Location
        public void ExpectTypeConverterError(String inline)
        {
            // Arrange
            var stream = CsvFixture(new string[] { inline });
            var parser = new ParticipantCsvStreamParser();

            // Act
            var records = parser.Parse(stream);

            // Assert
            Assert.Throws<CsvHelper.TypeConversion.TypeConverterException>(() =>
            {
                foreach (var record in records)
                {
                    ;
                }
            });
        }

        [Theory]
        [InlineData("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef,CaseId,ParticipantId,,")] // Missing last column
        public void ExpectMissingFieldError(String inline)
        {
            // Arrange
            var stream = CsvFixture(new string[] { inline });
            var parser = new ParticipantCsvStreamParser();

            // Act
            var records = parser.Parse(stream);

            // Assert
            Assert.Throws<CsvHelper.MissingFieldException>(() =>
            {
                foreach (var record in records)
                {
                    ;
                }
            });
        }

        [Theory]
        [InlineData("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef,CaseId,ParticipantId,2021-05-15,2021-04-01/2021-04-15 2021-03-01/2021-03-30 2021-02-01/2021-02-28 2021-01-01/2021-01-30,true")] //extra date range
        public void InvalidDateRanges(String inline)
        {
            // Arrange
            var stream = CsvFixture(new string[] { inline });
            var parser = new ParticipantCsvStreamParser();

            // Act
            var records = parser.Parse(stream);

            // Assert
            Assert.Throws<CsvHelper.FieldValidationException>(() =>
            {
                foreach (var record in records)
                {
                    ;
                }
            });
        }

        [Theory]
        [InlineData("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef,CaseId,ParticipantId,2021-05-15,2021-04-01/2021-04-15 2021-03-01/2021-03-30 2021-02-01/2021-02-28,true")] //3 date ranges
        [InlineData("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef,CaseId,ParticipantId,2021-05-15,2021-04-01/2021-04-15 2021-03-01/2021-03-30,true")] //2 date ranges
        [InlineData("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef,CaseId,ParticipantId,2021-05-15,2021-04-01/2021-04-15,true")] //1 date range
        public void ValidDateRanges(String inline)
        {
            // Arrange
            var stream = CsvFixture(new string[] { inline });
            var parser = new ParticipantCsvStreamParser();

            // Act
            var records = parser.Parse(stream);

            // Assert
            Assert.NotNull(records);
        }

        [Theory]
        [InlineData("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef,CaseId,ParticipantId")]
        public void OnlyRequiredColumns(String inline)
        {
            // Arrange
            var stream = CsvFixture(new string[] { inline }, requiredOnly: true);
            var parser = new ParticipantCsvStreamParser();

            // Act
            var records = parser.Parse(stream).ToList();

            // Assert
            Assert.Single(records);
            Assert.Equal(LDS_HASH, records.First().LdsHash);
            Assert.Equal("CaseId", records.First().CaseId);
            Assert.Equal("ParticipantId", records.First().ParticipantId);
            Assert.Null(records.First().ParticipantClosingDate);
            Assert.Null(records.First().VulnerableIndividual);
            Assert.Empty(records.First().RecentBenefitIssuanceDates);
        }

        [Fact]
        public void NullInputStream()
        {
            // Arrange
            Stream stream = null;
            var parser = new ParticipantCsvStreamParser();

            // Act / Assert
            Assert.Throws<ArgumentNullException>(() => parser.Parse(stream));
        }

        [Fact]
        public void EmptyInputStream()
        {
            // Arrange
            var stream = CsvFixture(new string[] { }, false, false);
            var parser = new ParticipantCsvStreamParser();

            // Act
            var records = parser.Parse(stream);

            // Assert
            Assert.Empty(records);
        }
        [Theory]
        [InlineData("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef,CaseId,ParticipantId,2020-10,,")]
        public void ExpectLegacyColumns(String inline)
        {
            // Arrange
            var stream = CsvFixture(new string[] { inline }, isLegacy: true);
            var parser = new ParticipantCsvStreamParser();

            // Act
            var records = parser.Parse(stream).ToList();

            // Assert
            Assert.Single(records);
            Assert.Equal(LDS_HASH, records.First().LdsHash);
            Assert.Equal("CaseId", records.First().CaseId);
            Assert.Equal("ParticipantId", records.First().ParticipantId);
            Assert.Null(records.First().ParticipantClosingDate);
            Assert.Null(records.First().VulnerableIndividual);
            Assert.Empty(records.First().RecentBenefitIssuanceDates);
        }

        [Theory]
        [InlineData("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef,CaseId#,ParticipantId,,,")] //  CaseId is not alphanumeric
        [InlineData("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef,CaseId,ParticipantId#,,")] //  ParticipantId is not alphanumeric
        public void ExpectAlphanumericValidationError(String inline)
        {
            // Arrange
            var stream = CsvFixture(new string[] { inline });
            var parser = new ParticipantCsvStreamParser();

            // Act
            var records = parser.Parse(stream);

            // Assert
            Assert.Throws<CsvHelper.FieldValidationException>(() =>
            {
                foreach (var record in records)
                {
                    ;
                }
            });
        }

        [Theory]
        [InlineData("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef,CaseId12345678901234567890,ParticipantId,,,")] //  CaseId is more than 20 chars
        [InlineData("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef,CaseId,ParticipantIdCaseId12345678901234567890,,")] //  ParticipantId  is more than 20 chars
        public void MaxLengthValidationError(String inline)
        {
            // Arrange
            var stream = CsvFixture(new string[] { inline });
            var parser = new ParticipantCsvStreamParser();

            // Act
            var records = parser.Parse(stream);

            // Assert
            Assert.Throws<CsvHelper.FieldValidationException>(() =>
            {
                foreach (var record in records)
                {
                    ;
                }
            });
        }
        [Theory]
        [InlineData("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef,CaseId-44,ParticipantId_1,,,")] //  CaseId is not alphanumeric
        public void ExpectAlphanumericValidationWithHypenAndDash(String inline)
        {
            // Arrange
            var stream = CsvFixture(new string[] { inline });
            var parser = new ParticipantCsvStreamParser();

            // Act
            var records = parser.Parse(stream).ToList(); ;

            // Assert
            Assert.Single(records);
            Assert.Equal(LDS_HASH, records.First().LdsHash);
            Assert.Equal("CaseId-44", records.First().CaseId);
            Assert.Equal("ParticipantId_1", records.First().ParticipantId);
            Assert.Null(records.First().ParticipantClosingDate);
            Assert.Null(records.First().VulnerableIndividual);
            Assert.Empty(records.First().RecentBenefitIssuanceDates);
        }

        [Theory]
        [InlineData("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef,CaseId-44,ParticipantId_1,,,")]
        [InlineData(",caseid1,participantid1,2021-05-15,2021-04-01/2021-04-15 2021-03-01/2021-03-30 2021-02-01/2021-02-28,true")]
        [InlineData("a3cab51dd68da2ac3e5508c8b0ee514ada03b9f166f7035b4ac26d9c56aa7bf9d6271e44c0064337a01b558ff63fd282de14eead7e8d5a613898b700589bcdec,,participantid1,2021-05-15,2021-04-01/2021-04-15 2021-03-01/2021-03-30 2021-02-01/2021-02-28,true")]
        [InlineData("a3cab51dd68da2ac3e5508c8b0ee514ada03b9f166f7035b4ac26d9c56aa7bf9d6271e44c0064337a01b558ff63fd282de14eead7e8d5a613898b700589bcdec,caseid1,,2021-05-15,2021-04-01/2021-04-15 2021-03-01/2021-03-30 2021-02-01/2021-02-28,true")]
        [InlineData(",,,2021-05-15,2021-04-01/2021-04-15 2021-03-01/2021-03-30 2021-02-01/2021-02-28,true")]
        public void ExpectToHavePllInfomation(string inline)
        {
            // Arrange
            var lds_hash = inline.Split(',')[0];
            var case_id = inline.Split(',')[1];
            var participant_id = inline.Split(',')[2];

            var stream = CsvFixture(new string[] { inline });
            var parser = new ParticipantCsvStreamParser();

            // Act
            var pii = parser.GetPersonallyIdentifiableInformation(stream);

            // Assert
            if (!string.IsNullOrEmpty(lds_hash))
                Assert.Contains(lds_hash, pii);
            else
                Assert.DoesNotContain(lds_hash, pii);

            if (!string.IsNullOrEmpty(case_id))
                Assert.Contains(case_id, pii);
            else
                Assert.DoesNotContain(case_id, pii);

            if (!string.IsNullOrEmpty(participant_id))
                Assert.Contains(participant_id, pii);
            else
                Assert.DoesNotContain(participant_id, pii);
        }
    }
}
