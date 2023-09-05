using Microsoft.AspNetCore.Http;

namespace Piipan.Metrics.Func.Api.Extensions
{
    public static class IQueryCollectionExtensions
    {
        public static string ParseString(this IQueryCollection query, string key)
        {
            return query[key];
        }

        public static int ParseInt(this IQueryCollection query, string key, int defaultValue = 0)
        {
            if (int.TryParse(query[key], out int result))
            {
                return result;
            }
            return defaultValue;
        }
    }
}