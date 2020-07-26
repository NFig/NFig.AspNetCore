using System;

namespace NFig.AspNetCore
{

    /// <summary>
    /// Delegate used when settings are updated by NFig.
    /// </summary>
    /// <param name="ex">
    /// Exception raised during the update process.
    /// </param>
    /// <param name="settings">
    /// Settings produced by the update process.
    /// </param>
    public delegate void SettingsUpdateDelegate<TSettings, TTier, TDataCenter>(Exception ex, TSettings settings)
        where TSettings : class, INFigSettings<TTier, TDataCenter>, new()
        where TTier : struct
        where TDataCenter : struct;
}
