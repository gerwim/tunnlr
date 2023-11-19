using System.Diagnostics.CodeAnalysis;
using Tunnlr.Client.Core.Contracts.Models;
using Tunnlr.Common.Protobuf;

namespace Tunnlr.Client.Core.Models;

public class Request : RequestBase, IRequest
{
    [SetsRequiredMembers]
    public Request(HttpRequest httpRequest)
    {
        HttpRequest = httpRequest;
        Title = $"{httpRequest.HttpMethod} {(new Uri(httpRequest.TargetUri).PathAndQuery)}";
    }
    
    public required HttpRequest HttpRequest { get; set; }
}