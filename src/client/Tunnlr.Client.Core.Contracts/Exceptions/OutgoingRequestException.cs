using System.Runtime.Serialization;

namespace Tunnlr.Client.Core.Contracts.Exceptions;

[Serializable]
public class OutgoingRequestException : Exception
{
    public OutgoingRequestException()
    {
    }

    protected OutgoingRequestException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public OutgoingRequestException(string? message) : base(message)
    {
    }

    public OutgoingRequestException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}