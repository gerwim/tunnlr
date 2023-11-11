using GerwimFeiken.Cache;
using GerwimFeiken.Cache.InMemory;
using GerwimFeiken.Cache.InMemory.Options;
using Tunnlr.Common.Options;
using Tunnlr.Server.Core.Authentication;
using Tunnlr.Server.Proxy;
using Tunnlr.Server.Proxy.GrpcServices;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

builder.Services.AddScoped<RequestsGrpcService>();
builder.Services.AddScoped<TunnelsGrpcService>();
builder.Services.AddScoped<GeneralGrpcService>();

builder.Services.AddSingleton<ICache>(new InMemoryCache(new InMemoryOptions
{
    DefaultExpirationTtl = 3600
}));
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddHttpClient<IAuthenticationService>();
builder.Configuration.GetRequiredSection(Auth0Options.OptionKey).RegisterOptions<Auth0Options>(builder);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.MapGrpcService<RequestsGrpcService>();
app.MapGrpcService<TunnelsGrpcService>();
app.MapGrpcService<GeneralGrpcService>();
// Configure the HTTP request pipeline.
// app.MapGet("/",
//     () =>
//         "No tunnel has been enabled for this domain.");
app.UseMiddleware<TunnelMiddleware>();
app.Run();