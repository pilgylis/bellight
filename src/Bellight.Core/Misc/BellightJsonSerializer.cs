using System.Text.Json;

namespace Bellight.Core.Misc
{
    public class BellightJsonSerializer: ISerializer
    {
        public JsonSerializerOptions Settings { get; set; } = DefaultJsonSerializerSettings;
        public string SerializeObject(object value)
        {
            return JsonSerializer.Serialize(value, Settings);
        }

        public T DeserializeObject<T>(string value)
        {
            return JsonSerializer.Deserialize<T>(value, Settings)!;
        }

        public object DeserializeObject(string value, Type type)
        {
            return JsonSerializer.Deserialize(value, type, Settings)!;
        }

        public object DeserializeObject(string value, string typeName)
        {
            var type = Type.GetType(typeName);
            return DeserializeObject(value, type!);
        }

        public string TrySerializeObject(object value)
        {
            return Try(() => SerializeObject(value));
        }

        public T TryDeserializeObject<T>(string value)
        {
            return Try(() => DeserializeObject<T>(value));
        }

        public object TryDeserializeObject(string value, Type type)
        {
            return Try(() => DeserializeObject(value, type));
        }

        public object TryDeserializeObject(string value, string typeName)
        {
            return Try(() => DeserializeObject(value, typeName));
        }

        public static JsonSerializerOptions DefaultJsonSerializerSettings
        {
            get
            {
                return new JsonSerializerOptions
                {
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };
            }
        }

        private T Try<T>(Func<T> func)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                StaticLog.Error(ex, ex.Message);
                return default(T)!;
            }
        }
    }
}
