using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Tunnlr.Client.Core.Services;
using Tunnlr.Common.DependencyInjection;

namespace Tunnlr.Client.Core;

public class Builder : IBuilder
{
    public void Build(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<InterceptorService>();
    }
}