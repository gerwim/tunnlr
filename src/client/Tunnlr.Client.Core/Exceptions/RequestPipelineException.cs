using System.Runtime.Serialization;

namespace Tunnlr.Client.Core.Exceptions;

[Serializable]
public class RequestPipelineException : Exception
{
    public RequestPipelineException()
    {
    }

    protected RequestPipelineException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public RequestPipelineException(string? message) : base(message)
    {
    }

    public RequestPipelineException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}