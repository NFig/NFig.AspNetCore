using NFig;

namespace NFig.AspNetCore.Sample.Configuration
{
    public partial class Settings
    {
        internal class SecretValueAttribute : DefaultSettingValueAttribute
        {
            public SecretValueAttribute(Tier tier)
            {
                AllowOverrides = false;
                DefaultValue = "<from secret store>";
                Tier = tier;
            }
        }

        internal class DataCenterDefaultValueAttribute : DefaultSettingValueAttribute
        {
            public DataCenterDefaultValueAttribute(DataCenter dataCenter, object defaultValue, bool allowOverrides = true)
            {
                DataCenter = dataCenter;
                DefaultValue = defaultValue;
                AllowOverrides = allowOverrides;
            }
        }

        internal class TieredDefaultValueAttribute : DefaultSettingValueAttribute
        {
            public TieredDefaultValueAttribute(Tier tier, object defaultValue, bool allowOverrides = true)
            {
                Tier = tier;
                DefaultValue = defaultValue;
                AllowOverrides = allowOverrides;
            }
        }

        internal class TieredDataCenterDefaultValueAttribute : DefaultSettingValueAttribute
        {
            public TieredDataCenterDefaultValueAttribute(Tier tier, DataCenter dataCenter, object defaultValue, bool allowOverrides = true)
            {
                Tier = tier;
                DataCenter = dataCenter;
                DefaultValue = defaultValue;
                AllowOverrides = allowOverrides;
            }
        }
    }
}
