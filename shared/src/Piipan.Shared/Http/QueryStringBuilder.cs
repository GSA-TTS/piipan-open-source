using Microsoft.AspNetCore.WebUtilities;

namespace Piipan.Shared.Http
{
    public static class QueryStringBuilder
    {
        public static string ToQueryString<T>(T obj) where T : class, new()
        {
            string result = "";
            T defaultObj = new T();
            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                var incomingValue = propertyInfo.GetValue(obj);
                var defaultValue = propertyInfo.GetValue(defaultObj);
                if (incomingValue?.ToString() != defaultValue?.ToString())
                {
                    result = QueryHelpers.AddQueryString(result, propertyInfo.Name, incomingValue.ToString());
                }
            }
            return result;
        }
    }
}
