namespace Bellight.Core.Exceptions;

public class ProviderNotFoundException : Exception
{
    public ProviderNotFoundException() : base()
    {
    }

    public ProviderNotFoundException(string message) : base(message)
    {
    }

    public ProviderNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}