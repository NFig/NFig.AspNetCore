using System;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;

namespace NFig.AspNetCore
{
    /// <summary>
    /// Cache of the current version of a settings class so that it can be resolved 
    /// by other things in the configuration / options / middleware handlers.
    /// </summary>
    internal static class NFigSettingsCache
    {
        private static readonly ConcurrentDictionary<Type, object> _knownStores = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// Attempts to locate a <typeparamref name="TSettings"/> implementation that was registered
        /// by the <see cref="NFigApplicationBuilderExtensions.UseNFig{TSettings,TTier,TDataCenter}"/> method.
        /// </summary>
        /// <typeparam name="TSettings"></typeparam>
        /// <typeparam name="TTier"></typeparam>
        /// <typeparam name="TDataCenter"></typeparam>
        /// <param name="store"></param>
        /// <returns></returns>
        public static bool TryGet<TSettings, TTier, TDataCenter>(out NFigSettingsWithStore<TSettings, TTier, TDataCenter> store)
            where TSettings : class, INFigSettings<TTier, TDataCenter>, new()
            where TTier : struct, Enum
            where TDataCenter : struct, Enum
        {
            if (_knownStores.TryGetValue(typeof(TSettings), out var untypedStore))
            {
                store = (NFigSettingsWithStore<TSettings, TTier, TDataCenter>)untypedStore;
                return true;
            }

            store = default;
            return false;
        }

        public static NFigSettingsWithStore<TSettings, TTier, TDataCenter> GetOrAdd<TSettings, TTier, TDataCenter>(Func<NFigSettingsWithStore<TSettings, TTier, TDataCenter>> factory)
            where TSettings : class, INFigSettings<TTier, TDataCenter>, new()
            where TTier : struct, Enum
            where TDataCenter : struct, Enum => (NFigSettingsWithStore<TSettings, TTier, TDataCenter>)_knownStores.GetOrAdd(typeof(TSettings), _ => factory());
    }
}
