using System;
using Microsoft.Extensions.DependencyInjection;
using NFig.Redis;
using StackExchange.Redis;

namespace NFig.AspNetCore
{
    /// <summary>
    /// Builds the configuration used to connect to an NFig store.
    /// </summary>
    public class NFigSettingsBuilder<TSettings, TTier, TDataCenter>
        where TSettings : class, INFigSettings<TTier, TDataCenter>, new()
        where TTier : struct, Enum
        where TDataCenter : struct, Enum
    {
        private Func<SettingsUpdateDelegate<TSettings, TTier, TDataCenter>, NFigSettingsWithStore<TSettings, TTier, TDataCenter>> _settingsFactory;
        
        /// <summary>
        /// Configures an NFig store that is backed by Redis.
        /// </summary>
        /// <param name="applicationName">
        /// Name of the application that the store is being used by.
        /// </param>
        /// <param name="tier">
        /// Tier that the application resides in.
        /// </param>
        /// <param name="dataCenter">
        /// Data center that the application resides in.
        /// </param>
        /// <param name="connectionString">
        /// Connection string to the Redis instance that the NFig store should connect to.
        /// </param>
        public NFigSettingsBuilder<TSettings, TTier, TDataCenter> UseRedis(string applicationName, TTier tier, TDataCenter dataCenter, string connectionString)
        {
            if (string.IsNullOrEmpty(applicationName))
            {
                throw new ArgumentNullException(nameof(applicationName));
            }

            return UseFactory(
                onSettingsUpdated =>
                {
                    var options = ConfigurationOptions.Parse(connectionString);
                    options.ClientName = $"{applicationName}.{Environment.MachineName}";
                    var connectionMultiplexer = ConnectionMultiplexer.Connect(options);
                    var store = NFigRedisStore<TSettings, TTier, TDataCenter>.FromConnectionMultiplexer(connectionMultiplexer);
                    var settings = store.GetAppSettings(applicationName, tier, dataCenter);
                    store.SubscribeToAppSettings(applicationName, tier, dataCenter, (ex, newSettings, _) => onSettingsUpdated(ex, newSettings));
                    return new NFigSettingsWithStore<TSettings, TTier, TDataCenter>(settings, store);
                });
        }

        /// <summary>
        /// Configures an NFig store using a factory method.
        /// </summary>
        /// <param name="settingsFactory">
        /// A factory method that provides an NFig store and an initial version of settings
        /// contained within it.
        /// </param>
        public NFigSettingsBuilder<TSettings, TTier, TDataCenter> UseFactory(Func<SettingsUpdateDelegate<TSettings, TTier, TDataCenter>, NFigSettingsWithStore<TSettings, TTier, TDataCenter>> settingsFactory)
        {
            _settingsFactory = settingsFactory;
            return this;
        }

        /// <summary>
        /// Builds the NFig store and the initial version of settings.
        /// </summary>
        /// <param name="onSettingsChanged">
        /// Delegate called whenever settings within the store are changed.
        /// </param>
        public NFigSettingsWithStore<TSettings, TTier, TDataCenter> Build(SettingsUpdateDelegate<TSettings, TTier, TDataCenter> onSettingsChanged)
        {
            return _settingsFactory(onSettingsChanged);
        }
    }
}
