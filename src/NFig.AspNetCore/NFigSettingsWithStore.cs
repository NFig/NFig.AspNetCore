using System;
using System.Threading;
using Microsoft.Extensions.Primitives;

namespace NFig.AspNetCore
{

    /// <summary>
    /// Exposes an NFig store and the current version of its settings.
    /// </summary>
    public class NFigSettingsWithStore<TSettings, TTier, TDataCenter>
        where TSettings : class, INFigSettings<TTier, TDataCenter>, new()
        where TTier : struct, Enum
        where TDataCenter : struct, Enum
    {
        private NFigChangeToken _changeToken;

        /// <summary>
        /// Constructs a new instance of <see cref="NFigSettingsWithStore{TSettings,TTier,TDataCenter}"/>.
        /// </summary>
        /// <param name="settings">Settings used by the application.</param>
        /// <param name="store"><see cref="NFigStore{TSettings,TTier,TDataCenter}"/> used by the application.</param>
        public NFigSettingsWithStore(TSettings settings, NFigStore<TSettings, TTier, TDataCenter> store)
        {
            Settings = settings;
            Store = store;

            _changeToken = new NFigChangeToken();
        }

        /// <summary>
        /// Gets the <see cref="NFigStore{TSettings,TTier,TDataCenter}"/> used by the application.
        /// </summary>
        public NFigStore<TSettings, TTier, TDataCenter> Store { get; }
        
        /// <summary>
        /// Gets the settings used by the application.
        /// </summary>
        public TSettings Settings { get; private set; }
        
        internal IChangeToken ChangeToken => _changeToken;

        /// <summary>
        /// Updates the settings used by the application to a new instance. Usually fired when
        /// the settings are reloaded by the store.
        /// </summary>
        /// <param name="settings">
        /// New settings.
        /// </param>
        public void UpdateSettings(TSettings settings)
        {
            Settings = settings;

            // notify listeners that things changed
            var previousToken = Interlocked.Exchange(ref _changeToken, new NFigChangeToken());
            previousToken.OnReload();
        }
    }
}
