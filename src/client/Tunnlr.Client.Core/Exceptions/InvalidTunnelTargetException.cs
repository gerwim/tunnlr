using System.Runtime.Serialization;

namespace Tunnlr.Client.Core.Exceptions;

[Serializable]
public class InvalidTunnelTargetException : Exception
{
    public InvalidTunnelTargetException()
    {
    }

    protected InvalidTunnelTargetException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public InvalidTunnelTargetException(string? message) : base(message)
    {
    }

    public InvalidTunnelTargetException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}