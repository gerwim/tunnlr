using System.Reflection;
using Grpc.Core;
using Semver;
using Tunnlr.Common.Protobuf;

namespace Tunnlr.Server.Proxy.Services;

public class GeneralService : General.GeneralBase
{
    public override Task<ValidateServerResponse> ValidateServerVersion(ValidateServerRequest request, ServerCallContext context)
    {
        var myVersion = SemVersion.Parse(Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion, SemVersionStyles.Strict);
        var clientVersion = SemVersion.Parse(request.ClientVersion, SemVersionStyles.Strict);

        var compareVersions = SemVersion.ComparePrecedence(myVersion, clientVersion); // 1 = server version is higher, 0 is equal, -1 client is higher

        switch (compareVersions)
        {
            case 1:
                return Task.FromResult(new ValidateServerResponse
                {
                    UpdateStatus = UpdateStatus.UpdateAvailable,
                });
            case 0:
                return Task.FromResult(new ValidateServerResponse
                {
                    UpdateStatus = UpdateStatus.Ok,
                });
            case -1:
                return Task.FromResult(new ValidateServerResponse
                {
                    UpdateStatus = UpdateStatus.Unsupported,
                });
            default:
                return Task.FromResult(new ValidateServerResponse
                {
                    UpdateStatus = UpdateStatus.Unknown,
                });
        }
    }
}