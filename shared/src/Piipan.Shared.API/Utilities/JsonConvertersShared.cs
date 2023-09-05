using Newtonsoft.Json.Converters;

namespace Piipan.Shared.API.Utilities
{
    public class JsonConvertersShared
    {
        public class DateTimeConverter : IsoDateTimeConverter
        {
            public DateTimeConverter()
            {
                base.DateTimeFormat = "yyyy-MM-dd";
            }
        }
    }
}
