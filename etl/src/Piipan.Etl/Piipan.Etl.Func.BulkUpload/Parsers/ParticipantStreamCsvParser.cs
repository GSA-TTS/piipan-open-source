using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Piipan.Etl.Func.BulkUpload.Models;
using Piipan.Etl.Func.BulkUpload.Validators;
using Piipan.Participants.Api.Models;
using Piipan.Shared.API.Utilities;
using Piipan.Shared.Helpers;

namespace Piipan.Etl.Func.BulkUpload.Parsers
{
    /// <summary>
    /// Maps and validates a record from a CSV file formatted in accordance with
    /// <c>/etl/docs/csv/import-schema.json</c> to a <c>IParticipant</c>.
    /// </summary>
    public class ParticipantMap : ClassMap<Participant>
    {
        private DateValidateHelper helper => new DateValidateHelper();
        public ParticipantMap()
        {
            Map(m => m.LdsHash).Name("lds_hash")
                .Validate(field => helper.ValidateLdsHash(field.Field));

            base.Map(m => m.CaseId).Name("case_id")
                .Validate(field => helper.ValidateCaseId(field.Field))
                .TypeConverterOption.NullValues(string.Empty).Optional();

            Map(m => m.ParticipantId).Name("participant_id")
                .Validate(field => helper.ValidateAlphaNumericWithLength(field.Field));

            Map(m => m.ParticipantClosingDate)
                .Name("participant_closing_date")
                .Validate(field => helper.IsValidParticipantClosingDate(field.Field))
                .TypeConverterOption.NullValues(string.Empty).TypeConverter<ToDatetimeConverter>().Optional();

            Map(m => m.RecentBenefitIssuanceDates)
               .Name("recent_benefit_issuance_dates")
               .Validate(field => helper.IsValidRecentBenefitIssuanceDates(field.Field))
              .TypeConverter<ToDateRangeArrayConverter>().Optional();

            Map(m => m.VulnerableIndividual)
                .Name("vulnerable_individual")
                .TypeConverterOption.NullValues(string.Empty).Optional();
        }
    }

    /// <summary>
    /// Converts list of month-only dates to last day of month when as DateTimes
    /// and to ISO 8601 year-months when as a string
    /// </summary>
    public class ToMonthEndArrayConverter : DefaultTypeConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (text == "") return new List<DateTime>();
            string[] allElements = text.Split(' ');
            DateTime[] elementsAsDateTimes = allElements.Select(s => MonthEndDateTime.Parse(s)).ToArray();
            return new List<DateTime>(elementsAsDateTimes);
        }
    }

    /// <summary>
    /// Converts ISO 8601 year-months-date to DateTime
    /// </summary>
  	public class ToDatetimeConverter : DefaultTypeConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            string[] formats = { "yyyy-MM-dd" };
            DateTime dateValue;
            var result = DateTime.TryParseExact(
                text,
                formats,
                new CultureInfo("en-US"),
                DateTimeStyles.None,
                out dateValue);
            return dateValue;
        }

        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            DateTime dt = (DateTime)value;
            return dt.ToString("yyyy-MM-dd");
        }
    }

    /// <summary>
    /// Converts to list of Date range - ISO 8601 year-months-date when as a string
    /// </summary>
    public class ToDateRangeArrayConverter : DefaultTypeConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (text == "") return new List<DateRange>();
            string[] allElements = text.Split(' ');
            List<DateRange> range = new List<DateRange>();
            foreach (string strRange in allElements)
            {
                DateTime[] elementsAsDateTimes = strRange.Split('/').Select(s => DateTime.Parse(s)).ToArray();
                range.Add(new DateRange(elementsAsDateTimes[0], elementsAsDateTimes[1]));
            }
            return range;
        }

        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            List<string> rangesAsStrings = new List<string>();
            List<DateRange> ranges = (List<DateRange>)value;
            foreach (DateRange range in ranges)
            {
                string start = range.Start.ToString("yyyy-MM-dd");
                string end = range.End.ToString("yyyy-MM-dd");
                string rangeString = $"{start}/{end}";
                rangesAsStrings.Add(rangeString);
            }

            var result = string.Join(" ", rangesAsStrings);
            return result;
        }
    }
    /// <summary>
    /// Parses or gets personally identifiable information from CSV file
    /// </summary>
    public class ParticipantCsvStreamParser : IParticipantStreamParser
    {
        /// <summary>
        /// Registers class mapper and sets Participant class parser for CSV file
        /// </summary>
        public IEnumerable<IParticipant> Parse(Stream input)
        {
            var reader = new StreamReader(input);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                BadDataFound = (args) =>
                {
                    throw new InvalidDataException($"Error parsing the CSV. Bad data found on row {args.Context.Parser.Row}");
                },
                ExceptionMessagesContainRawData = false
            };

            var csv = new CsvReader(reader, config);
            csv.Context.RegisterClassMap<ParticipantMap>();

            // Yields records as it is iterated over. We can't use "using" statement
            return csv.GetRecords<Participant>();
        }

        /// <summary>
        /// Finds personally identifiable information in lds_hash, case_id, participant_id columns
        /// and returns HashSet with PII from CSV file
        /// </summary>
        public HashSet<string> GetPersonallyIdentifiableInformation(Stream input)
        {
            using (var reader = new StreamReader(input))
            {
                var hashSet = new HashSet<string>();
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    TrimOptions = TrimOptions.Trim,
                    ExceptionMessagesContainRawData = false
                };

                var csv = new CsvReader(reader, config);

                csv.Read();
                csv.ReadHeader();

                while (csv.Read())
                {
                    var participant = csv.GetRecord<ParticipantCsv>();

                    if (!string.IsNullOrEmpty(participant.CaseId))
                        hashSet.Add(participant.CaseId);

                    if (!string.IsNullOrEmpty(participant.ParticipantId))
                        hashSet.Add(participant.ParticipantId);

                    if (!string.IsNullOrEmpty(participant.LdsHash))
                        hashSet.Add(participant.LdsHash);
                }
                return hashSet;
            }
        }
    }
}
