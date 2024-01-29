using Tunnlr.Common.Protobuf;

namespace Tunnlr.Client.Core.Contracts.Models;

public interface IRequest
{
    HttpRequest HttpRequest { get; set; }
    DateTime DateTime { get; }
    string? Title { get; set; }
    string? SubTitle { get; set; }
    List<byte> Body { get; }
    bool BodyTruncated { get; set; }
    void WriteToBody(IEnumerable<byte> bytes);
}