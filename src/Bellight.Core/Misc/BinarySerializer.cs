using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Bellight.Core.Misc
{
    public static class BinarySerializer
    {
        public static byte[] Serialize(object item)
        {
            var ms = new MemoryStream();
            try
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, item);
                return ms.ToArray();
            }
            finally
            {
                ms.Close();
            }
        }

        public static T Deserialize<T>(byte[] byteArray)
        {
            var ms = new MemoryStream(byteArray);
            try
            {
                var formatter = new BinaryFormatter();
                return (T)formatter.Deserialize(ms);
            }
            finally
            {
                ms.Close();
            }
        }
    }
}
