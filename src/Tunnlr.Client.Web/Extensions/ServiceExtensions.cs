using Tunnlr.Common.Exceptions;

namespace Tunnlr.Client.Web.Extensions;

public static class ServiceExtensions
{
    public static void AddGrpcClient<T>(this WebApplicationBuilder builder) where T : class
    {
        builder.Services.AddGrpcClient<T>(options =>
        {
            options.Address = new Uri(builder.Configuration.GetRequiredSection("Tunnlr:Client")
                                          .GetValue<string>("ApiServer") ??
                                      throw new InvalidConfigurationException(
                                          "Option value Tunnlr:Client:ApiServer is missing"));
        });
    }
}