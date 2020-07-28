using System;

namespace NFig.AspNetCore.Tests
{
    internal class Settings : INFigSettings<Tier, DataCenter>
    {
        internal const string ApplicationName = "Tests";
        internal const Tier Tier = Tests.Tier.Local;
        internal const DataCenter DataCenter = Tests.DataCenter.Local;
        
        [SettingsGroup]
        public FeatureFlagSettings FeatureFlags { get; private set; }

        public class FeatureFlagSettings
        {
            [Setting(false)]
            public bool FoobarEnabled { get; private set; }
        }

        string INFigSettings<Tier, DataCenter>.ApplicationName { get; set; } = ApplicationName;
        string INFigSettings<Tier, DataCenter>.Commit { get; set; } = Guid.NewGuid().ToString("N");
        Tier INFigSettings<Tier, DataCenter>.Tier { get; set; } = Tier;
        DataCenter INFigSettings<Tier, DataCenter>.DataCenter { get; set; } = DataCenter;
    }

    internal enum Tier
    {
        Local,
        Dev,
        Test,
        Prod,
    }

    internal enum DataCenter
    {
        Local,
        London,
        NewYork,
    }
}
