using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NFig.AspNetCore.Tests
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected IConfiguration Configuration { get; }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddNFig<Settings, Tier, DataCenter>();
        }

        public virtual void Configure(IApplicationBuilder app)
        {
            app.UseNFig<Settings, Tier, DataCenter>(
                (configuration, builder) =>
                {
                    builder.UseFactory(
                        onSettingsUpdated =>
                        {
                            var store = new NFigMemoryStore<Settings, Tier, DataCenter>(new SettingsFactory<Settings, Tier, DataCenter>());
                            var settings = store.GetAppSettings(Settings.ApplicationName, Settings.Tier, Settings.DataCenter);
                            store.Changed += (sender, e) =>
                            {
                                if (sender is NFigStore<Settings, Tier, DataCenter> changeStore)
                                {
                                    onSettingsUpdated(null, changeStore.GetAppSettings("Tests", Tier.Local, DataCenter.Local));
                                }
                            };
                            return new NFigSettingsWithStore<Settings, Tier, DataCenter>(settings, store);
                        });
                });
        }
    }
}
