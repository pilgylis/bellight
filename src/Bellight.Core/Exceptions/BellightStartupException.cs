using System.Runtime.Serialization;

namespace Bellight.Core.Exceptions;

[Serializable]
public class BellightStartupException : Exception
{
    public BellightStartupException()
    {
    }

    public BellightStartupException(string message) : base(message)
    {
    }

    public BellightStartupException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected BellightStartupException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        : base(serializationInfo, streamingContext)
    {
    }
}
