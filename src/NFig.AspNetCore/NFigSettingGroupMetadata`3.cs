using System;

namespace NFig.AspNetCore
{
    /// <summary>
    /// Metadata about a property marked with NFig's <see cref="SettingsGroupAttribute"/>.
    /// </summary>
    internal class NFigSettingGroupMetadata<TSettings, TTier, TDataCenter>
        where TSettings : class, INFigSettings<TTier, TDataCenter>, new()
        where TTier : struct, Enum
        where TDataCenter : struct, Enum
    {
        public NFigSettingGroupMetadata(Type type, Func<TSettings, object> provider)
        {
            Type = type;
            Accessor = provider;
        }

        /// <summary>
        /// Gets the <see cref="Type"/> of the property providing the settings group.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets a func that can be used to retrieve the settings group.
        /// </summary>
        public Func<TSettings, object> Accessor { get; }
    }
}
