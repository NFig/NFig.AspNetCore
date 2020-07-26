using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NFig.AspNetCore.Tests
{
    /// <summary>
    /// An in-memory NFig store. This store is primarily intended for testing.
    /// </summary>
    /// <typeparam name="TSettings">Type of settings to use.</typeparam>
    /// <typeparam name="TTier">The enum type used to represent the deployment tier.</typeparam>
    /// <typeparam name="TDataCenter">The enum type used to represent the data center.</typeparam>
    internal class NFigMemoryStore<TSettings, TTier, TDataCenter> : NFigStore<TSettings, TTier, TDataCenter>
        where TSettings : class, INFigSettings<TTier, TDataCenter>, new()
        where TTier : struct, Enum
        where TDataCenter : struct, Enum
    {
        private class Overrides : Dictionary<string, SettingValue<TTier, TDataCenter>>
        {
        }

        public event EventHandler Changed;

        private readonly struct OverrideKey : IEquatable<OverrideKey>
        {
            public OverrideKey(string appName, TTier tier, TDataCenter dataCenter)
            {
                AppName = appName;
                Tier = tier;
                DataCenter = dataCenter;
            }
            
            public string AppName { get; }
            public TTier Tier { get; }
            public TDataCenter DataCenter { get; }

            public override bool Equals(object? obj)
            {
                return obj is OverrideKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(AppName, Tier, DataCenter);
            }

            public bool Equals(OverrideKey other) => AppName == other.AppName && Tier.Equals(other.Tier) && DataCenter.Equals(other.DataCenter);
        }
        
        private readonly ConcurrentDictionary<string, List<OverrideKey>> _overridesByApp = new ConcurrentDictionary<string, List<OverrideKey>>();
        private readonly ConcurrentDictionary<OverrideKey, Overrides> _overrides = new ConcurrentDictionary<OverrideKey, Overrides>();
        
        public NFigMemoryStore(SettingsFactory<TSettings, TTier, TDataCenter> factory) : base(factory)
        {
        }

        public override TSettings GetAppSettings(string appName, TTier tier, TDataCenter dataCenter)
        {
            var overrides = Enumerable.Empty<SettingValue<TTier, TDataCenter>>();
            if (_overrides.TryGetValue(new OverrideKey(appName, tier, dataCenter), out var overridesBySetting))
            {
                overrides = overridesBySetting.Values;
            }

            var ex = Factory.TryGetAppSettings(out TSettings settings, tier, dataCenter, overrides);
            if (ex != null)
            {
                throw ex;
            }

            return settings;
        }

        public override void SetOverride(string appName, string settingName, string value, TTier tier, TDataCenter dataCenter)
        {
            var overrideKey = new OverrideKey(appName, tier, dataCenter);
            var overrideValue = new SettingValue<TTier, TDataCenter>(settingName, value, tier, dataCenter);
            _overrides.AddOrUpdate(
                overrideKey,
                key => new Overrides {[settingName] = overrideValue},
                (key, existing) =>
                {
                    existing[settingName] = overrideValue;
                    _overridesByApp.AddOrUpdate(
                        appName,
                        key => new List<OverrideKey> {overrideKey},
                        (key, existing) =>
                        {
                            existing.Add(overrideKey);
                            return existing;
                        }
                    );
                        
                    return existing;
                }
            );
            
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public override void ClearOverride(string appName, string settingName, TTier tier, TDataCenter dataCenter)
        {
            var overrideKey = new OverrideKey(appName, tier, dataCenter);
            if (_overrides.TryGetValue(overrideKey, out var overrides))
            {
                overrides.Remove(settingName);
            }
            
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public override string GetCurrentCommit(string appName)
        {
            if (_overridesByApp.TryGetValue(appName, out var overrideKeys))
            {
                var hashCode = 7;
                for (var i = 0; i < overrideKeys.Count; i++)
                {
                    hashCode ^= overrideKeys[i].GetHashCode();
                }

                return hashCode.ToString("x8");
            }

            return string.Empty;
        }

        public override SettingInfo<TTier, TDataCenter>[] GetAllSettingInfos(string appName)
        {
            if (!_overridesByApp.TryGetValue(appName, out var overrideKeys))
            {
                return Array.Empty<SettingInfo<TTier, TDataCenter>>();
            }

            var overrides = new List<SettingValue<TTier, TDataCenter>>();
            foreach (var overrideKey in overrideKeys)
            {
                overrides.AddRange(_overrides[overrideKey].Values);
            }
            
            return Factory.GetAllSettingInfos(overrides);
        }

        public override SettingInfo<TTier, TDataCenter> GetSettingInfo(string appName, string settingName)
        {
            var settingInfos = GetAllSettingInfos(appName);
            foreach (var settingInfo in settingInfos)
            {
                if (settingInfo.Name == settingName)
                {
                    return settingInfo;
                }
            }

            return null;
        }
    }
}
