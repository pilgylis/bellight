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
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                formatter.Serialize(ms, item);
#pragma warning restore SYSLIB0011 // Type or member is obsolete
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
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                return (T)formatter.Deserialize(ms);
#pragma warning restore SYSLIB0011 // Type or member is obsolete
            }
            finally
            {
                ms.Close();
            }
        }
    }
}
