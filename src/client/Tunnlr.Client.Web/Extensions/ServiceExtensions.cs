using Grpc.Core;
using Grpc.Net.Client.Configuration;
using Tunnlr.Client.Web.Services;
using Tunnlr.Common.Exceptions;

namespace Tunnlr.Client.Web.Extensions;

public static class ServiceExtensions
{
    public static void AddGrpcClient<T>(this WebApplicationBuilder builder) where T : class
    {
        var defaultMethodConfig = new MethodConfig
        {
            Names = { MethodName.Default },
            RetryPolicy = new RetryPolicy
            {
                MaxAttempts = 3,
                InitialBackoff = TimeSpan.FromMilliseconds(500),
                MaxBackoff = TimeSpan.FromSeconds(3),
                BackoffMultiplier = 1,
                RetryableStatusCodes =
                {
                    StatusCode.Unavailable,
                }
            }
        };

        builder.Services.AddGrpcClient<T>(options =>
        {
            options.Address = new Uri(builder.Configuration.GetRequiredSection("Tunnlr:Client")
                                          .GetValue<string>("ApiServer") ??
                                      throw new InvalidConfigurationException(
                                          "Option value Tunnlr:Client:ApiServer is missing"));
            options.ChannelOptionsActions.Add(channelOptions =>
            {
                channelOptions.ServiceConfig = new ServiceConfig { MethodConfigs = { defaultMethodConfig } };
            });
        }).AddCallCredentials(async (context, metadata, serviceProvider) =>
        {
            var authenticationService = serviceProvider.GetRequiredService<IAuthenticationService>();
            var token = await authenticationService.GetAccessToken();
            if (!string.IsNullOrEmpty(token))
            {
                metadata.Add("Authorization", $"Bearer {token}");
            }
        }).ConfigureChannel(options =>
        {
#if DEBUG
            options.UnsafeUseInsecureChannelCallCredentials = true;
#endif
        });
    }
}