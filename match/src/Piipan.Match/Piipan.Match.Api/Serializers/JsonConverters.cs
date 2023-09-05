using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Piipan.Shared.API.Utilities;

namespace Piipan.Match.Api.Serializers
{
    public class JsonConverters
    {
        /// <summary>
        /// JSON.NET converter for serializing/deserializing a DateTime
        /// object using our desired YYYY-MM-DD format.
        /// </summary>
        /// <remarks>
        /// Applied to model properties as `[JsonConverter(typeof(DateTimeConverter))]`
        /// </remarks>
        public class DateTimeConverter : IsoDateTimeConverter
        {
            public DateTimeConverter()
            {
                base.DateTimeFormat = "yyyy-MM-dd";
            }
        }

        /// <summary>
        /// JSON.NET converter used for converting null, missing, or empty
        /// properties to a `null` value when deserializing JSON.
        /// </summary>
        /// <remarks>
        /// Applied to model properties as `[JsonConverter(typeof(NullConverter))]`.
        ///
        /// Intended for use when deserializing JSON in incoming request bodies.
        /// Null values are needed for optional fields to properly perform exact
        /// matches when querying the state-level database.
        ///
        /// Matches the behavoir of `Piipan.Etl.BulkUpload` which writes missing
        /// or empty optional fields to the databse as `DbNull.Value`.
        /// </remarks>
        public class NullConverter : JsonConverter
        {
            public override bool CanRead => true;
            public override bool CanWrite => false;

            public override bool CanConvert(Type objectType) => objectType == typeof(string);

            public override object ReadJson(
                JsonReader reader,
                Type objectType,
                object existingValue,
                JsonSerializer serializer
            )
            {
                return String.IsNullOrWhiteSpace((string)reader.Value) ? null : (string)reader.Value;
            }

            public override void WriteJson(
                JsonWriter writer,
                object value,
                JsonSerializer serializer
            )
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// JSON.NET converter
        /// Deserializes an array of month-only strings to
        /// an array of DateTimes, each set to last day of given month.
        /// Serializes an array of DateTimes into an array of a yearh-month strings
        /// using our desired ISO 8601 YYYY-MM format.
        /// </summary>
        /// <remarks>
        /// Applied to model properties as `[JsonConverter(typeof(MonthEndArrayConverter))]`
        /// </remarks>
        public class MonthEndArrayConverter : JsonConverter
        {
            public override bool CanRead => false;
            public override bool CanWrite => true;

            public override bool CanConvert(Type objectType) => objectType == typeof(IEnumerable<DateTime>);

            public override object ReadJson(
                JsonReader reader,
                Type objectType,
                object existingValue,
                JsonSerializer serializer
            )
            {
                throw new NotImplementedException();
            }

            public override void WriteJson(
                JsonWriter writer,
                object value,
                JsonSerializer serializer
            )
            {
                var dates = (IEnumerable<DateTime>)value;
                dates.ToList().Sort((x, y) => y.CompareTo(x));
                writer.WriteStartArray();
                foreach (var date in dates)
                {
                    writer.WriteValue(date.ToString("yyyy-MM"));
                }
                writer.WriteEndArray();
            }
        }
        public class DateRangeConverter : JsonConverter
        {
            public override bool CanRead => false;
            public override bool CanWrite => true;

            public override bool CanConvert(Type objectType) => objectType == typeof(IEnumerable<DateRange>);

            public override object ReadJson(
                JsonReader reader,
                Type objectType,
                object existingValue,
                JsonSerializer serializer
            )
            {
                throw new NotImplementedException();
            }

            public override void WriteJson(
                JsonWriter writer,
                object value,
                JsonSerializer serializer
            )
            {
                var dates = (IEnumerable<DateRange>)value;
                writer.WriteStartArray();
                foreach (var date in dates)
                {
                    serializer.Serialize(writer, date);
                }
                writer.WriteEndArray();
            }
        }

    } 

}
