using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace NFig.AspNetCore
{
    /// <summary>
    /// Options used to customize how NFig is used within the application.
    /// </summary>
    public class NFigOptions<TTier, TDataCenter>
        where TTier : struct, Enum
        where TDataCenter : struct, Enum
    {
        private static readonly Color[] _defaultColors = new []
        {
            Color.ForestGreen,
            Color.SteelBlue,
            Color.DarkOrange,
            Color.Red,
        };
        
        /// <summary>
        /// Constructs a new instance of <see cref="NFigOptions{TTier,TDataCenter}"/>.
        /// </summary>
        public NFigOptions()
        {
            TierColors = Enum.GetValues(typeof(TTier))
                .Cast<TTier>()
                .Select((t, i) => (Tier: t, Color: _defaultColors[i % _defaultColors.Length]))
                .ToDictionary(x => x.Tier, x => x.Color);
        }

        /// <summary>
        /// Gets or sets the colors used to render different tiers in setting management pages.
        /// </summary>
        public Dictionary<TTier, Color> TierColors { get; set; }
    }
}
