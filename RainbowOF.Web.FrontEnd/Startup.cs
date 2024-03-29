using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RainbowOF.Data.SQL;
using RainbowOF.Tools;
using RainbowOF.Repositories.Common;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Blazored.Toast;
using RainbowOF.Tools.Services;
using RainbowOF.ViewModels;
using Blazorise.RichTextEdit;
using RainbowOF.Integration.Repositories.Classes;

namespace RainbowOF.Web.FrontEnd
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();

            /// Blazorise stuff
            services.AddBlazorise(options =>
                  {
                      options.ChangeTextOnKeyPress = true; // optional
                  })
                  .AddBootstrapProviders()
                  .AddFontAwesomeIcons();
            services.AddBlazoriseRichTextEdit(options =>
            {
                options.UseShowTheme = true;
                options.UseBubbleTheme = true;
            });
            // Auto mapper stuff
            services.AddAutoMapper(typeof(ViewMappingProfile), typeof(IntegrationMappingProfile));

            // DBContext Stuff
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("SQLConnection"));
                options.EnableDetailedErrors(true);

                //---- Only use if needed                options.EnableSensitiveDataLogging(true);
                //                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            services.AddScoped<IAppUnitOfWork, AppUnitOfWork>();
            services.AddScoped<ApplicationState>();         //-> used to store any global states
            services.AddLogging();
            services.AddSingleton<ILoggerManager, LoggerManager>();

            services.AddBlazoredToast();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        //public void Configure(IComponentsApplicationBuilder app, IWebHostEnvironment env)
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();

            //app.ApplicationServices
            //    .UseBootstrapProviders()
            //    .UseFontAwesomeIcons();



            // Blazorise
            //            app.ApplicationServices.
            //  .UseBootstrapProviders()
            //  .UseFontAwesomeIcons();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
