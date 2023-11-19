using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using MudBlazor.Services;
using Tunnlr.Client.Core.RequestPipeline;
using Tunnlr.Client.Core.Services;
using Tunnlr.Client.Web;
using Tunnlr.Client.Web.Extensions;
using Tunnlr.Client.Web.Services;
using Tunnlr.Common.DependencyInjection;
using Tunnlr.Common.Exceptions;
using Tunnlr.Common.Options;
using Tunnlr.Common.Protobuf;

var builder = WebApplication.CreateBuilder(args);

StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<TunnelService>();
builder.Services.AddScoped<GeneralService>();
builder.Services.AddScoped<DomainsService>();
builder.Services.AddMudServices();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
var auth0Options = builder.Configuration.GetRequiredSection(Auth0Options.OptionKey).RegisterOptions<Auth0Options>(builder);

builder.AddGrpcClient<Tunnels.TunnelsClient>(true);
builder.AddGrpcClient<Requests.RequestsClient>(false);
builder.AddGrpcClient<General.GeneralClient>(false);
builder.AddGrpcClient<Domains.DomainsClient>(true);

// Configure authentication
builder.Services.ConfigureSameSiteNoneCookies();

builder.Services.AddAuth0WebAppAuthentication(options =>
{
    options.Domain = auth0Options.Domain ?? throw new InvalidConfigurationException($"{nameof(auth0Options.Domain)} is empty");
    options.ClientId = auth0Options.ClientId ?? throw new InvalidConfigurationException($"{nameof(auth0Options.ClientId)} is empty");
    options.ClientSecret = auth0Options.ClientSecret ?? throw new InvalidConfigurationException($"{nameof(auth0Options.ClientSecret)} is empty");
})
    .WithAccessToken(options =>
    {
        options.Audience = auth0Options.Audience ?? throw new InvalidConfigurationException($"{nameof(auth0Options.Audience)} is empty");
        options.UseRefreshTokens = true;
    });
builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();

builder.Services.AddRequestPipelineExecutor();

// Run all builders
Builders.RunAll(builder);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// Run all configurations
Configurators.RunAll(app);

var logger = app.Services.GetRequiredService<ILogger<Program>>();

var url = app.Configuration.GetRequiredSection("Kestrel:Endpoints:Web").GetValue<string>("Url");
if (string.IsNullOrWhiteSpace(url))
{
    logger.LogError("Can't find url to listen on");
    return;
}

if (!app.Environment.IsDevelopment())
{
    try
    {
        Browser.Start(url);
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Could not start a browser");
    }
}

Console.WriteLine($"Tunnlr started on {url}");

app.Run();