using Newtonsoft.Json;
using System;

namespace Bellight.Core.Misc
{
    public interface ISerializer : ITransientDependency
    {
        JsonSerializerSettings Settings { get; set; }
        string SerializeObject(object value);

        T DeserializeObject<T>(string value);

        object DeserializeObject(string value, Type type);
        object DeserializeObject(string value, string typeName);

        string TrySerializeObject(object value);

        T TryDeserializeObject<T>(string value);

        object TryDeserializeObject(string value, Type type);
        object TryDeserializeObject(string value, string typeName);
    }
}
