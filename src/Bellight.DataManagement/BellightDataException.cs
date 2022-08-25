﻿using System.Runtime.Serialization;

namespace Bellight.DataManagement;

[Serializable]
public class BellightDataException : Exception
{
    public BellightDataException()
    {
    }

    public BellightDataException(string? message) : base(message)
    {
    }

    public BellightDataException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected BellightDataException(SerializationInfo info, StreamingContext context)
            : base(info, context)
    {
    }
}