using System.Reflection;
using Tunnlr.Common.Protobuf;

namespace Tunnlr.Client.Core.Services;

public class GeneralService
{
    private readonly General.GeneralClient _generalClient;

    public GeneralService(General.GeneralClient generalClient)
    {
        _generalClient = generalClient;
    }

    public async Task<UpdateStatus> CheckUpdateStatus()
    {
        var response = await _generalClient.ValidateServerVersionAsync(new ValidateServerRequest
        {
            ClientVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
        });
        return response.UpdateStatus;
    }
}