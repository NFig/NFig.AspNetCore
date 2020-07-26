using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NFig.AspNetCore.Sample.Configuration;
using static NFig.AspNetCore.Sample.Configuration.Settings;

namespace NFig.AspNetCore.Sample
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddNFig<Settings, Tier, DataCenter>(
                c =>
                {
                    // just as an example, reverse default colors
                    c.TierColors = new Dictionary<Tier, Color>
                    {
                        [Tier.Local] = Color.Red,
                        [Tier.Dev] = Color.DarkOrange,
                        [Tier.Test] = Color.SteelBlue,
                        [Tier.Prod] = Color.ForestGreen
                    };
                }
            );
            
            services
                .AddOptions<SecretSettings>()
                .Configure(s =>
                {
                    s.CopyFrom(
                        _configuration.GetSection("Secrets").Get<SecretSettings>(o => o.BindNonPublicProperties = true)
                    );
                });
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app
                .UseDeveloperExceptionPage()
                .UseRouting()
                .UseNFig<Settings, Tier, DataCenter>(
                    (cfg, builder) =>
                    {
                        var settings = cfg.Get<AppSettings>();
                        var connectionString = cfg.GetConnectionString("Redis");

                        builder.UseRedis(settings.ApplicationName, settings.Tier, settings.DataCenter, connectionString);
                    }
                )
                .UseEndpoints(
                    x => x.MapControllers()
                );
        }
    }
}
