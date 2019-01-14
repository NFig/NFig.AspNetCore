using static NFig.AspNetCore.Sample.Configuration.Settings;

namespace NFig.AspNetCore.Sample.Views.Home
{
    public class IndexModel
    {
        public IndexModel(
            SiteSettings siteSettings, 
            SiteSettings.NestedSettings nestedSettings,
            SecretSettings secretSettings
        )
        {
            SiteSettings = siteSettings;
            NestedSettings = nestedSettings;
            SecretSettings = secretSettings;
        }

        public SiteSettings SiteSettings { get;  }
        public SiteSettings.NestedSettings NestedSettings { get; }
        public SecretSettings SecretSettings { get; }
    }
}
