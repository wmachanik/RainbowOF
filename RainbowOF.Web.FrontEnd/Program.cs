// ---> Blazorise stuff from  https://blazorise.com/docs/start
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Blazorise.RichTextEdit;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using RainbowOF.Data.SQL;
using RainbowOF.Integration.Repositories.Classes;
using RainbowOF.Repositories.Common;
using RainbowOF.Tools;
using RainbowOF.Tools.Services;
// ---> App stuff
using RainbowOF.ViewModels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
//builder.Services.AddSingleton<WeatherForecastService>();

// ---> Blazorise stuff from  https://blazorise.com/docs/start
builder.Services
    .AddBlazorise(options =>
    {
        options.Immediate = true;
    })
    .AddBootstrapProviders()
    .AddFontAwesomeIcons();

//--> other services that using to be in Startup

builder.Services.AddBlazoriseRichTextEdit(options =>
{
    options.UseShowTheme = true;
    options.UseBubbleTheme = true;
});
// Auto mapper stuff
builder.Services.AddAutoMapper(typeof(ViewMappingProfile), typeof(IntegrationMappingProfile));

// DBContext Stuff
// It is recommended that for Blazor we use DbContext Factory not DBContext builder.Services.AddDbContext<ApplicationDbContext>(options =>
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SQLConnection"),o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery));
    options.EnableDetailedErrors(true);
    options.EnableSensitiveDataLogging(true);  ///  DbContextOptionsBuilder.EnableSensitiveDataLogging to see conflicts
    //options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);    // disable tracking since this is mainly a read once site, with saving time unknown
        //---- Only use if needed                options.EnableSensitiveDataLogging(true);
    //                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

// NLog Stuff
var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");

// NLog: Setup NLog for Dependency injection
builder.Logging.ClearProviders();
//builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
builder.Host.UseNLog();

//App specific injections
//builder.Services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ApplicationState>();         //-> used to store any global states
builder.Services.AddLogging();
//builder.Services.AddTransient<GenericHelper>();
builder.Services.AddSingleton<ILoggerManager, LoggerManager>();

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

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
