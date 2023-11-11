using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Tunnlr.Client.Core.Services;
using Tunnlr.Client.Web;
using Tunnlr.Client.Web.Extensions;
using Tunnlr.Client.Web.Persistence;
using Tunnlr.Client.Web.Services;
using Tunnlr.Common.DependencyInjection;
using Tunnlr.Common.Exceptions;
using Tunnlr.Common.Options;
using Tunnlr.Common.Protobuf;

var builder = WebApplication.CreateBuilder(args);

StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

// Configure DbContext
builder.Services.AddDbContext<TunnlrDbContext>(options =>
{
    var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Tunnlr");
    Directory.CreateDirectory(directory);
    var connectionStringBuilder = new SqliteConnectionStringBuilder($"Data Source={Path.Combine(directory, "Tunnlr.Client.Web.db")}"); 
    options.UseSqlite(connectionStringBuilder.ToString(), sqliteOptions =>
    {
        sqliteOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
    });

#if DEBUG
    options.EnableDetailedErrors();
    options.EnableSensitiveDataLogging();
#endif
    // Set default tracking
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<TunnelService>();
builder.Services.AddScoped<GeneralService>();
builder.Services.AddMudServices();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
var auth0Options = builder.Configuration.GetRequiredSection(Auth0Options.OptionKey).RegisterOptions<Auth0Options>(builder);

builder.AddGrpcClient<Tunnels.TunnelsClient>();
builder.AddGrpcClient<Requests.RequestsClient>();
builder.AddGrpcClient<General.GeneralClient>();

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

// Run all builders
Builders.RunAll(builder);

var app = builder.Build();

// Apply database migrations
using (var scope = app.Services.CreateScope())
{
    var dataContext = scope.ServiceProvider.GetRequiredService<TunnlrDbContext>();
    dataContext.Database.Migrate();
    dataContext.SaveChanges();
}

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

#if !DEBUG
    Browser.Start();
#endif

app.Run();