using System.Runtime.Serialization;

namespace Tunnlr.Client.Web.Persistence.Exceptions;

[Serializable]
public class DbContextCreationException : Exception
{
    public DbContextCreationException()
    {
    }

    protected DbContextCreationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public DbContextCreationException(string? message) : base(message)
    {
    }

    public DbContextCreationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}