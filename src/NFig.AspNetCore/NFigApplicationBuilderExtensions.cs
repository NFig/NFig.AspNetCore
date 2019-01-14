using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NFig;
using NFig.AspNetCore;

namespace Microsoft.AspNetCore.Builder
{
    public static class NFigApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds NFig to the application and configures the store.
        /// </summary>
        /// <param name="configureStore">Action used to configure the NFig backing store.</param>
        public static IApplicationBuilder UseNFig<TSettings, TTier, TDataCenter>(this IApplicationBuilder app, Action<IConfiguration, NFigSettingsBuilder<TSettings, TTier, TDataCenter>> configureStore)
            where TSettings : class, INFigSettings<TTier, TDataCenter>, new()
            where TTier : struct
            where TDataCenter : struct
        {
            var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
            var settingsBuilder = new NFigSettingsBuilder<TSettings, TTier, TDataCenter>();
            configureStore(configuration, settingsBuilder);
            NFigSettingsCache.GetOrAdd(
                () => settingsBuilder.Build(
                    (_, newSettings) => 
                    {
                        if (NFigSettingsCache.TryGet<TSettings, TTier, TDataCenter>(out var settingsWithStore))
                        {
                            settingsWithStore.UpdateSettings(newSettings);
                        }
                    }
                )
            );
            return app;
        }
    }
}
