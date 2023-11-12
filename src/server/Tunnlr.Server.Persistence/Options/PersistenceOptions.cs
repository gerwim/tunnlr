using Tunnlr.Common.Options;

namespace Tunnlr.Server.Persistence.Options;

public class PersistenceOptions : IOptions
{
    public const string OptionKey = "Tunnlr:Server:Persistence";
    
    public string? Provider { get; set; }
    public string? ConnectionString { get; set; }
}