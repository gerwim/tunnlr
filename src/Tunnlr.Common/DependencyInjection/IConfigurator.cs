using Microsoft.AspNetCore.Builder;

namespace Tunnlr.Common.DependencyInjection;

public interface IConfigurator
{
    public void Configure(WebApplication app);
}