using System.Security.Claims;
using GerwimFeiken.Cache;
using GerwimFeiken.Cache.InMemory;
using GerwimFeiken.Cache.InMemory.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Tunnlr.Common.DependencyInjection;
using Tunnlr.Common.Exceptions;
using Tunnlr.Common.Options;
using Tunnlr.Server.Core.Authentication;
using Tunnlr.Server.Proxy;
using Tunnlr.Server.Proxy.GrpcServices;

var builder = WebApplication.CreateBuilder(args);

#if DEBUG
    IdentityModelEventSource.ShowPII = true;
#endif
// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
var auth0Options = builder.Configuration.GetRequiredSection(Auth0Options.OptionKey).RegisterOptions<Auth0Options>(builder);

var domain = $"https://{auth0Options.Domain ?? throw new InvalidConfigurationException($"{nameof(auth0Options.Domain)} is empty")}/";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = domain;
        options.Audience = auth0Options.Audience ?? throw new InvalidConfigurationException($"{nameof(auth0Options.Audience)} is empty");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = ClaimTypes.NameIdentifier
        };
    });

builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

builder.Services.AddScoped<RequestsGrpcService>();
builder.Services.AddScoped<TunnelsGrpcService>();
builder.Services.AddScoped<GeneralGrpcService>();
builder.Services.AddScoped<DomainsGrpcService>();

builder.Services.AddSingleton<ICache>(new InMemoryCache(new InMemoryOptions
{
    DefaultExpirationTtl = 3600
}));
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddHttpClient<IAuthenticationService>();
builder.Configuration.GetRequiredSection(Auth0Options.OptionKey).RegisterOptions<Auth0Options>(builder);

// Run all builders
Builders.RunAll(builder);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.MapGrpcService<RequestsGrpcService>();
app.MapGrpcService<TunnelsGrpcService>();
app.MapGrpcService<GeneralGrpcService>();
app.MapGrpcService<DomainsGrpcService>();

// Run all configurations
Configurators.RunAll(app);

// Configure the HTTP request pipeline.
// app.MapGet("/",
//     () =>
//         "No tunnel has been enabled for this domain.");
app.UseMiddleware<TunnelMiddleware>();
app.Run();