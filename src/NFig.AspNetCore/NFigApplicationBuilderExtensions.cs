using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NFig;
using NFig.AspNetCore;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Extension methods used to configure NFig.
    /// </summary>
    public static class NFigApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds NFig to the application and configures the store.
        /// </summary>
        /// <param name="app"><see cref="IApplicationBuilder"/> to add NFig to.</param>
        /// <param name="configureStore">Action used to configure the NFig backing store.</param>
        public static IApplicationBuilder UseNFig<TSettings, TTier, TDataCenter>(this IApplicationBuilder app, Action<IConfiguration, NFigSettingsBuilder<TSettings, TTier, TDataCenter>> configureStore)
            where TSettings : class, INFigSettings<TTier, TDataCenter>, new()
            where TTier : struct, Enum
            where TDataCenter : struct, Enum
        {
            if (configureStore == null) throw new ArgumentNullException(nameof(configureStore));
            
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
