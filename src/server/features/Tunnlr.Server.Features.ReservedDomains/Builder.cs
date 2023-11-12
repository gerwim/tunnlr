using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Tunnlr.Common.DependencyInjection;

namespace Tunnlr.Server.Features.ReservedDomains;

public class Builder : IBuilder
{
    public void Build(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IReservedDomainsService, ReservedDomainsService>();
    }
}