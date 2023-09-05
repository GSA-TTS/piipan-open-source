using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentValidation;
using Moq;
using Piipan.Etl.Func.BulkUpload.Models;
using Piipan.Etl.Func.BulkUpload.Validators;
using Xunit;

namespace Piipan.Etl.Func.BulkUpload.Tests
{
    public class CsvValidatorTest
    {
        [Fact]
        public void ValidateCsvDoc_ShouldReturnValidResult()
        {
            // Arrange
            var fileName = "example.csv";
            var eTag = "x0sf5458s548";
            var cvsValidator = new CsvValidator(new ParticipantValidator());

            //Act
            var res = cvsValidator.ValidateCsvDoc(new MemoryStream(File.ReadAllBytes("example.csv")), fileName, eTag);

            //Assert
            Assert.NotNull(res);
            Assert.True(res.IsValid);
            Assert.True(res.Errors.Count == 0);
            Assert.True(res.FileName == fileName);
            Assert.True(res.ETag == eTag);
        }
        [Fact]
        public void ValidateCsvDoc_ShouldReturnErrors()
        {
            // Arrange
            var fileName = "exampleWithBadRows.csv";
            var eTag = "x0sf5458s548";
            var cvsValidator = new CsvValidator(new ParticipantValidator());

            //Act
            var res = cvsValidator.ValidateCsvDoc(new MemoryStream(File.ReadAllBytes("exampleWithBadRows.csv")), fileName, eTag);

            //Assert
            Assert.NotNull(res);

            Assert.False(res.IsValid);
            Assert.True(res.Errors.Count == 5);

            //error in row number 4
            Assert.True(res.Errors.ContainsKey(4));
            Assert.True(res.Errors[4].Count == 1);
            Assert.Contains(res.Errors[4], c => c.ErrorMessage == "The specified condition was not met for 'Recent Benefit Issuance Dates'.");
            Assert.Contains(res.Errors[4], c => c.PropertyName == "RecentBenefitIssuanceDates");

            //error in row number 20
            Assert.True(res.Errors.ContainsKey(20));
            Assert.True(res.Errors[20].Count == 1);
            Assert.Contains(res.Errors[20], c => c.ErrorMessage == "'Lds Hash' is not in the correct format.");
            Assert.Contains(res.Errors[20], c => c.PropertyName == "LdsHash");

            //error in row number 22
            Assert.True(res.Errors.ContainsKey(22));
            Assert.True(res.Errors[22].Count == 1);
            Assert.Contains(res.Errors[22], c => c.ErrorMessage == "'Lds Hash' is not in the correct format.");
            Assert.Contains(res.Errors[22], c => c.PropertyName == "LdsHash");

            //error in row number 25
            Assert.True(res.Errors.ContainsKey(25));
            Assert.True(res.Errors[25].Count == 2);
            Assert.Contains(res.Errors[25], c => c.ErrorMessage == "'Participant Id' is not in the correct format.");
            Assert.Contains(res.Errors[25], c => c.PropertyName == "ParticipantId");
            Assert.Contains(res.Errors[25], c => c.ErrorMessage == "'Case Id' is not in the correct format.");
            Assert.Contains(res.Errors[25], c => c.PropertyName == "CaseId");

            //error in row number 29
            Assert.True(res.Errors.ContainsKey(29));
            Assert.True(res.Errors[29].Count == 1);
            Assert.Contains(res.Errors[29], c => c.ErrorMessage == "'Lds Hash' is not in the correct format.");
            Assert.Contains(res.Errors[29], c => c.PropertyName == "LdsHash");

            Assert.True(res.FileName == fileName);
            Assert.True(res.ETag == eTag);
        }

        [Fact]
        public void ValidateCsvDoc_ShouldReturnErrorWhenHeaederIsMissing()
        {
            // Arrange
            var fileName = "exampleWithInvalidMissingHeader.csv";
            var eTag = "x0sf5458s548";
            var cvsValidator = new CsvValidator(new ParticipantValidator());

            //Act
            var res = cvsValidator.ValidateCsvDoc(new MemoryStream(File.ReadAllBytes("exampleWithInvalidMissingHeader.csv")), fileName, eTag);

            //Assert
            Assert.NotNull(res);

            Assert.False(res.IsValid);
            Assert.True(res.Errors.Count == 1);

            //error for header
            Assert.True(res.Errors.ContainsKey(2));
            Assert.True(res.Errors[2].Count == 1);
            Assert.Contains(res.Errors[2], c => c.ErrorMessage == "Field with name 'lds_hash' does not exist.");
            Assert.Contains(res.Errors[2], c => c.PropertyName == "Upload File Validation Failure");

            Assert.True(res.FileName == fileName);
            Assert.True(res.ETag == eTag);
        }

        [Fact]
        public void ValidateCsvDoc_ShouldReturnGenericErrorWhenGenericExceptionEncountered()
        {
            // Arrange
            var fileName = "exampleWithBadRows.csv";
            var eTag = "x0sf5458s548";
            Mock<IValidator<ParticipantCsv>> mockValidator = new Mock<IValidator<ParticipantCsv>>();

            mockValidator.Setup(x => x.Validate(It.IsAny<ParticipantCsv>())).Throws(new System.Exception());

            var cvsValidator = new CsvValidator(mockValidator.Object);

            //Act
            var res = cvsValidator.ValidateCsvDoc(new MemoryStream(File.ReadAllBytes("exampleWithBadRows.csv")), fileName, eTag);

            //Assert
            Assert.NotNull(res);

            Assert.False(res.IsValid);
            Assert.True(res.Errors.Count == 1);

            //error for header
            Assert.True(res.Errors.ContainsKey(2));
            Assert.True(res.Errors[2].Count == 1);
            Assert.Contains(res.Errors[2], c => c.ErrorMessage == "Failed to parse & validate uploaded csv file");
            Assert.Contains(res.Errors[2], c => c.PropertyName == "Upload File Validation Failure");

            Assert.True(res.FileName == fileName);
            Assert.True(res.ETag == eTag);
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData("10-10-2022", false)]
        [InlineData("10/10/2022", false)]
        [InlineData("10/22/2022", false)]
        [InlineData("10/10-2022", false)]
        [InlineData("10/52", false)]
        [InlineData("test", false)]
        [InlineData("2022/10/10", false)]
        [InlineData("2022-10-10", true)]
        public void IsValidParticipantClosingDate_ShouldBeValid(string value, bool correctResult)
        {
            //Arrange
            DateValidateHelper helper = new();

            //Act
            var result = helper.IsValidParticipantClosingDate(value);

            //Assert
            Assert.True(correctResult == result);
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData("2021-04-01/2021-04-15 2021-03-01/2021-03-30 2021-02-01/2021-02-28  2022-02-01/2022-02-28", false)]
        [InlineData("2021-04-01/2021-04-15 2021-03-01/2021-03-30 2021-02-01/2021-02-28", true)]
        [InlineData("2021-04-01/2021-04-15 2021-03-01/2021-03-30", true)]
        [InlineData("2021-06-01/2021-04-15 2021-07-01/2021-03-30 2021-02-01/2021-02-28", false)]
        [InlineData("10/22/2022", false)]
        [InlineData("10/10-2022", false)]
        [InlineData("10/52", false)]
        [InlineData("test", false)]
        [InlineData("2022/10/10", false)]
        [InlineData("2022-10-10", false)]
        [InlineData("2022/", false)]
        public void IsValidRecentBenefitIssuanceDates_ShouldBeValid(string value, bool correctResult)
        {
            //Arrange
            DateValidateHelper helper = new();

            //Act
            var result = helper.IsValidRecentBenefitIssuanceDates(value);

            //Assert
            Assert.True(correctResult == result);
        }

        [Theory]
        [InlineData(5000)]
        [InlineData(1001)]
        [InlineData(1000)]
        [InlineData(999)]
        [InlineData(1)]
        public void ValidateCsvDoc_ShouldReachMaxNumberOfErrors(int badRowsNumber)
        {
            //Arrange
            var participantsWithErrors = new List<string>();

            for (int i = 0; i < badRowsNumber; i++)
            {
                participantsWithErrors.Add("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcf" +
                    "c7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef#$," +
                    "CaseId," +
                    "ParticipantId," +
                    "2020-10-10," +
                    "2021-05-01/2021-05-02 2021-06-01/2021-06-02," +
                    "true");
            }
            var cvsValidator = new CsvValidator(new ParticipantValidator());

            using (var stream = CsvFixture(participantsWithErrors))
            {
                //Act
                var res = cvsValidator.ValidateCsvDoc(stream, "test", "test");

                //Assert
                Assert.NotNull(res);
                Assert.False(res.IsValid);

                if (badRowsNumber >= 1000)
                {
                    Assert.True(res.Errors.Count == 1000);
                    Assert.True(res.ErrorInfo == "The maximum number of errors has been reached. Please retry your upload after addressing these errors.");
                }
                else
                {
                    Assert.True(res.Errors.Count == badRowsNumber);
                    Assert.True(res.ErrorInfo == null);
                }
            }
        }

        [Theory]
        [InlineData("testtesttest12", true)]
        [InlineData("123456789123456789123456789", false)]
        [InlineData("#^&*(", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void ValidateAlphaNumericWithLength_ShouldBeValid(string field, bool expectedResult)
        {
            //Arrange
            var validator = new DateValidateHelper();

            //Act
            var res = validator.ValidateAlphaNumericWithLength(field);

            //Assert
            Assert.True(res == expectedResult);
        }

        [Theory]
        [InlineData("a3cab51dd68da2ac3e5508c8b0ee514ada03b9f166f7035b4ac26d9c56aa7bf9d6271e44c0064337a01b558ff63fd282de14eead7e8d5a613898b700589bcdec", true)]
        [InlineData("a3cab51dd68da2ac3e5508c8b0ee514ada03b9f166f7035b4ac26d9c56aa7bf9d6271e44c0064337a01b558ff63fd282de14eead7e8d5a613898b700589bcdec4", false)]
        [InlineData("a3cab51dd68da2ac3e5508c8b0ee514ada03b9f166f7035b4ac26d9c56aa7bf9d6271e44c0064337a01b558ff63fd282de14eead7e8d5a613898b700589bcde", false)]
        [InlineData("a3cab51dd68da2ac3e5508c8b0ee514ada03b9f166f7035b4ac26d9c56aa7bf9d6271e44c0064337a01b558ff63fd282de14eead7e8d5a613898b7$0589bcde", false)]
        [InlineData("a3cab51dd68da2ac3e5508c8b0ee514ada03b9f166f7035b4ac26d9c56aa7bf9d6271e44c0064337a01b558ff63fd282de14eead7e8d5a613898b7!0589bcde", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void ValidateLdsHash_ShouldBeValid(string field, bool expectedResult)
        {
            //Arrange
            var validator = new DateValidateHelper();

            //Act
            var res = validator.ValidateLdsHash(field);

            //Assert
            Assert.True(res == expectedResult);
        }

        [Theory]
        [InlineData("testtesttest12", true)]
        [InlineData("123456789123456789123456789", false)]
        [InlineData("#^&*(", false)]
        [InlineData("", true)]
        [InlineData(null, true)]
        public void ValidateCaseId_ShouldBeValid(string field, bool expectedResult)
        {
            //Arrange
            var validator = new DateValidateHelper();

            //Act
            var res = validator.ValidateCaseId(field);

            //Assert
            Assert.True(res == expectedResult);
        }

        private Stream CsvFixture(List<string> records)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("lds_hash,case_id,participant_id,participant_closing_date,recent_benefit_issuance_dates,vulnerable_individual");

            foreach (var record in records)
            {
                writer.WriteLine(record);
            }
            writer.Flush();
            stream.Position = 0;

            return stream;
        }
    }
}
