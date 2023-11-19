using Tunnlr.Common.Options;

namespace Tunnlr.Client.Web;

public class Options : IOptions
{
    public const string OptionKey = "Tunnlr:Client";

    public string? ApiServer { get; set; }
    public string? ListenAddress { get; set; }
    public int? ListenPort { get; set; }
    public bool? UnsafeUseInsecureChannelCallCredentials { get; set; }
}