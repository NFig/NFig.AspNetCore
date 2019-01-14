using System.Threading;
using Microsoft.Extensions.Primitives;

namespace NFig.AspNetCore
{

    /// <summary>
    /// Exposes an NFig store and the current version of its settings.
    /// </summary>
    public class NFigSettingsWithStore<TSettings, TTier, TDataCenter>
        where TSettings : class, INFigSettings<TTier, TDataCenter>, new()
        where TTier : struct
        where TDataCenter : struct
    {
        private NFigChangeToken _changeToken;

        public NFigSettingsWithStore(TSettings settings, NFigStore<TSettings, TTier, TDataCenter> store)
        {
            Settings = settings;
            Store = store;

            _changeToken = new NFigChangeToken();
        }

        public NFigStore<TSettings, TTier, TDataCenter> Store { get; }
        public TSettings Settings { get; private set; }
        internal IChangeToken ChangeToken => _changeToken;

        public void UpdateSettings(TSettings settings)
        {
            Settings = settings;

            // notify listeners that things changed
            var previousToken = Interlocked.Exchange(ref _changeToken, new NFigChangeToken());
            previousToken.OnReload();
        }
    }
}
