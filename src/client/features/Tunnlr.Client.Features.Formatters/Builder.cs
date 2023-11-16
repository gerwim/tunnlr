using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Tunnlr.Client.Core.Formatters;
using Tunnlr.Common.DependencyInjection;

namespace Tunnlr.Client.Features.Formatters;

public class Builder : IBuilder
{
    public void Build(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IRequestFormatter, JsonFormatter>();
    }
}