using Microsoft.AspNetCore.Builder;

namespace Tunnlr.Common.DependencyInjection;

public interface IBuilder
{
    public void Build(WebApplicationBuilder builder);
}