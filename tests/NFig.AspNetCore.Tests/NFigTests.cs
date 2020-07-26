using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace NFig.AspNetCore.Tests
{
    public class NFigTests : IClassFixture<NFigFixture<Startup>>
    {
        private readonly NFigFixture<Startup> _fixture;

        public NFigTests(NFigFixture<Startup> fixture)
        {
            _fixture = fixture;
        }
        
        [Fact]
        public void OptionsAreConfiguredCorrectly()
        {
            var services = _fixture.Host.Services;
            var settings = services.GetService<IOptions<Settings>>();
            Assert.NotNull(settings);
            var featureFlags = services.GetService<IOptions<Settings.FeatureFlagSettings>>();
            Assert.NotNull(featureFlags);
            Assert.Same(settings.Value.FeatureFlags, featureFlags.Value);
        }

        [Fact]
        public void OverridesArePropagated()
        {
            var services = _fixture.Host.Services;
            var featureFlags = services.GetService<IOptions<Settings.FeatureFlagSettings>>();
            
            Assert.NotNull(featureFlags);
            Assert.False(featureFlags.Value.FoobarEnabled);

            Assert.True(NFigSettingsCache.TryGet<Settings, Tier, DataCenter>(out var storeWithSettings));
            if (!(storeWithSettings.Store is NFigMemoryStore<Settings, Tier, DataCenter> memoryStore))
            {
                throw new Exception("Expected an NFigMemoryStore");
            }

            var settingName = nameof(Settings.FeatureFlags) + "." + nameof(Settings.FeatureFlagSettings.FoobarEnabled);
            memoryStore.SetOverride(Settings.ApplicationName, settingName, bool.TrueString, Settings.Tier, Settings.DataCenter);
            
            featureFlags = services.GetService<IOptions<Settings.FeatureFlagSettings>>();
            Assert.NotNull(featureFlags);
            Assert.True(featureFlags.Value.FoobarEnabled);
        }
        
        [Fact]
        public void RemovalsArePropagated()
        {
            var services = _fixture.Host.Services;

            Assert.True(NFigSettingsCache.TryGet<Settings, Tier, DataCenter>(out var storeWithSettings));
            if (!(storeWithSettings.Store is NFigMemoryStore<Settings, Tier, DataCenter> memoryStore))
            {
                throw new Exception("Expected an NFigMemoryStore");
            }

            var settingName = nameof(Settings.FeatureFlags) + "." + nameof(Settings.FeatureFlagSettings.FoobarEnabled);
            memoryStore.SetOverride(Settings.ApplicationName, settingName, bool.TrueString, Settings.Tier, Settings.DataCenter);
            
            var featureFlags = services.GetService<IOptions<Settings.FeatureFlagSettings>>();
            Assert.NotNull(featureFlags);
            Assert.True(featureFlags.Value.FoobarEnabled);
            
            memoryStore.ClearOverride(Settings.ApplicationName, settingName, Settings.Tier, Settings.DataCenter);
            featureFlags = services.GetService<IOptions<Settings.FeatureFlagSettings>>();
            Assert.NotNull(featureFlags);
            Assert.False(featureFlags.Value.FoobarEnabled);
        }
    }
}
