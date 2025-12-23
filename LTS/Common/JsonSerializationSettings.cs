using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LTS.Common
{
    public static class JsonSerializationSettings
    {
        public static readonly JsonSerializerSettings TelemetrySettings = new JsonSerializerSettings
        {
            Converters = { new StringEnumConverter() },
        };
    }
}
