using System;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace NFig.AspNetCore
{
   /// <summary>
    /// Implements <see cref="IChangeToken"/>
    /// </summary>
    internal class NFigChangeTokenSource<TSettings, TTier, TDataCenter, TOptions> : IOptionsChangeTokenSource<TOptions>
        where TSettings : class, INFigSettings<TTier, TDataCenter>, new()
        where TTier : struct, Enum
        where TDataCenter : struct, Enum
    {
        public string Name => Options.DefaultName;

        public IChangeToken GetChangeToken()
        {
            if (!NFigSettingsCache.TryGet<TSettings, TTier, TDataCenter>(out var settingsWithStore))
            {
                throw new InvalidOperationException($"Options for NFig settings of type {typeof(TSettings)} are not configured");
            }

            return settingsWithStore.ChangeToken;
        }
    }
}
