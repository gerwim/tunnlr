using System.Runtime.Serialization;

namespace Tunnlr.Common.Exceptions;

[Serializable]
public class InvalidHttpMethodException : Exception
{
    public InvalidHttpMethodException()
    {
    }

    protected InvalidHttpMethodException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public InvalidHttpMethodException(string? message) : base(message)
    {
    }

    public InvalidHttpMethodException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}