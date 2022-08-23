using Bellight.Core.Misc;
using System.Text.Json;

namespace Bellight.Core;

public static class GenericExtensions
{
    public static string ToJson<T> (this T item)
    {
        return JsonSerializer.Serialize(item, BellightJsonSerializer.DefaultJsonSerializerSettings);
    }
}
