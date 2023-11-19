using System.Diagnostics.CodeAnalysis;
using Tunnlr.Common.Protobuf;

namespace Tunnlr.Client.Core.Models;

public class Response : RequestBase, IResponse
{
    [SetsRequiredMembers]
    public Response(HttpResponse httpResponse)
    {
        HttpResponse = httpResponse;
    }
    
    public required HttpResponse HttpResponse { get; set; }
}