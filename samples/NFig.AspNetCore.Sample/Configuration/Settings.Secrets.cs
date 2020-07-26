using System.ComponentModel;

namespace NFig.AspNetCore.Sample.Configuration
{
    public partial class Settings
    {
        [SettingsGroup]
        public SecretSettings Secrets { get; private set; }

        public class SecretSettings
        {
            [Setting("")]
            [SecretValue(Tier.Local)]
            [SecretValue(Tier.Dev)]
            [SecretValue(Tier.Test)]
            [SecretValue(Tier.Prod)]
            public string Username { get; private set; }

            [Setting("")]
            [SecretValue(Tier.Local)]
            [SecretValue(Tier.Dev)]
            [SecretValue(Tier.Test)]
            [SecretValue(Tier.Prod)]
            public string Password { get; private set; }

            public void CopyFrom(SecretSettings other)
            {
                Username = other.Username;
                Password = other.Password;
            }
        }
    }
}