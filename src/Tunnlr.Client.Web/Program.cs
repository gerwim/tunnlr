using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Tunnlr.Client.Core.Services;
using Tunnlr.Client.Web;
using Tunnlr.Client.Web.Extensions;
using Tunnlr.Client.Web.Persistence;
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

builder.AddGrpcClient<Tunnels.TunnelsClient>();
builder.AddGrpcClient<Requests.RequestsClient>();
builder.AddGrpcClient<General.GeneralClient>();

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

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

#if !DEBUG
    Browser.Start();
#endif

app.Run();