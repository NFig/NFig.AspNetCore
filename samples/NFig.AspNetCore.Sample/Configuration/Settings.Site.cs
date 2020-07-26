using System.ComponentModel;

namespace NFig.AspNetCore.Sample.Configuration
{
    public partial class Settings
    {
        [SettingsGroup]
        public SiteSettings Site { get; private set; }

        public class SiteSettings
        {
            [Setting("")]
            [TieredDefaultValue(Tier.Local, "https://aspnetcore.nfig.local")]
            [TieredDefaultValue(Tier.Dev, "https://aspnetcore.nfig.dev")]
            [TieredDefaultValue(Tier.Test, "https://aspnetcore.nfig.test")]
            [TieredDefaultValue(Tier.Prod, "https://aspnetcore.nfig.com")]
            [Description("Base URL for the site")]
            public string BaseUrl { get; private set; }

            [SettingsGroup]
            public NestedSettings Nested { get; private set; }

            public class NestedSettings
            {
                [Setting(false)]
                public bool Enabled { get; private set; }
            }
        }
    }
}
