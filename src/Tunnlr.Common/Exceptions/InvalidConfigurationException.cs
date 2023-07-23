using System.Runtime.Serialization;

namespace Tunnlr.Common.Exceptions;

[Serializable]
public class InvalidConfigurationException : Exception
{
    public InvalidConfigurationException()
    {
    }

    protected InvalidConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public InvalidConfigurationException(string? message) : base(message)
    {
    }

    public InvalidConfigurationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}