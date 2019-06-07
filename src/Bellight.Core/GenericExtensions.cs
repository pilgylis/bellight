using Bellight.Core.Misc;
using Newtonsoft.Json;

namespace Bellight.Core
{
    public static class GenericExtensions
    {
        public static string ToJson<T> (this T item)
        {
            return JsonConvert.SerializeObject(item, BellightJsonSerializer.DefaultJsonSerializerSettings);
        }
    }
}
