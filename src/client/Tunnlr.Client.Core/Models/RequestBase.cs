namespace Tunnlr.Client.Core.Models;

public class RequestBase
{
    public DateTime DateTime { get; } = DateTime.Now;
    
    public List<byte> Body { get; } = new();
    public bool BodyTruncated { get; set; }
    
    public void WriteToBody(IEnumerable<byte> bytes)
    {
        if (Body.Count <= 5242880) // allow max 5 MB to be written to the body
        {
            Body.AddRange(bytes);
        }
        else
        {
            BodyTruncated = true;
        }
    }
}