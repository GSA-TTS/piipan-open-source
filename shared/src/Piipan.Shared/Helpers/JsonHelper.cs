using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;

namespace Piipan.Shared.Helpers
{
    public static class JsonHelper
    {
        public static T TryParse<T>(string jsonData) where T : new()
        {
            JSchemaGenerator generator = new JSchemaGenerator();
            JSchema parsedSchema = generator.Generate(typeof(T));
            JObject jObject = JObject.Parse(jsonData);

            return jObject.IsValid(parsedSchema) ?
                JsonConvert.DeserializeObject<T>(jsonData) : default(T);
        }
    }
}
