using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Tunnlr.Common.DependencyInjection;

namespace Tunnlr.Client.Core.Formatters;

public class Builder : IBuilder
{
    public void Build(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IBaseRequestFormatter, RequestFormatter>();
        builder.Services.AddScoped<IBaseResponseFormatter, ResponseFormatter>();
    }
}