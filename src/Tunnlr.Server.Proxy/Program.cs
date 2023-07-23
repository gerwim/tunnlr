using Tunnlr.Server.Proxy;
using Tunnlr.Server.Proxy.Services;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

builder.Services.AddScoped<RequestsService>();
builder.Services.AddScoped<TunnelsService>();
builder.Services.AddScoped<GeneralService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.MapGrpcService<RequestsService>();
app.MapGrpcService<TunnelsService>();
app.MapGrpcService<GeneralService>();
// Configure the HTTP request pipeline.
// app.MapGet("/",
//     () =>
//         "No tunnel has been enabled for this domain.");
app.UseMiddleware<TunnelMiddleware>();
app.Run();