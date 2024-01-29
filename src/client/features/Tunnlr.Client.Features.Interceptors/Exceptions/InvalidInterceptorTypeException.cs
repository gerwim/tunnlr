using System.Runtime.Serialization;

namespace Tunnlr.Client.Features.Interceptors.Exceptions;

[Serializable]
public class InvalidInterceptorTypeException : Exception
{
    public InvalidInterceptorTypeException()
    {
    }

    protected InvalidInterceptorTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public InvalidInterceptorTypeException(string? message) : base(message)
    {
    }

    public InvalidInterceptorTypeException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}