using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using NFig;
using NFig.AspNetCore;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class NFigServiceCollectionExtensions
    {
        /// <summary>
        /// Adds services needed to support resolution of configuration data
        /// from an NFig store.
        /// </summary>
        public static IServiceCollection AddNFig<TSettings, TTier, TDataCenter>(this IServiceCollection services)
            where TSettings : class, INFigSettings<TTier, TDataCenter>, new()
            where TTier : struct
            where TDataCenter : struct
        {
            Func<TSettings, object> CreateGetterMethod(PropertyAndParent propertyInfo)
            {
                var list = ImmutableArray.CreateBuilder<PropertyInfo>();
                var currentProperty = propertyInfo;
                while (currentProperty != null)
                {
                    list.Add(currentProperty.Property);
                    currentProperty = currentProperty.Parent;
                }

                var dynamicMethod = new DynamicMethod(
                    "RetrieveSetting_" + propertyInfo.Property.Name,
                    typeof(object),
                    new[] { typeof(TSettings) },
                    typeof(NFigServiceCollectionExtensions).Module,
                    true
                );

                var il = dynamicMethod.GetILGenerator();

                // arg 0 = TSettings settings

                // start with the TSettings object
                il.Emit(OpCodes.Ldarg_0); // [settings]

                // loop through any levels of nesting
                // the list is in bottom-to-top order, so we have to iterate in reverse
                for (var i = list.Count - 1; i >= 1; i--)
                {
                    il.Emit(OpCodes.Callvirt, list[i].GetMethod); // [nested-property-obj]
                }

                // stack should now be [parent-obj-of-setting]
                il.Emit(OpCodes.Callvirt, propertyInfo.Property.GetMethod); // [setting-value]
                il.Emit(OpCodes.Ret);

                return (Func<TSettings, object>)dynamicMethod.CreateDelegate(typeof(Func<TSettings, object>));
            }

            // iteratively walk the settings and find all the setting groups within
            // it... This allows individual parts of the settings to be resolved using
            // the IOptions<T>...
            ImmutableArray<NFigSettingGroupMetadata<TSettings, TTier, TDataCenter>> FindSettingGroups()
            {
                var settingGroupMetadata = ImmutableArray.CreateBuilder<NFigSettingGroupMetadata<TSettings, TTier, TDataCenter>>();
                var queue = new Queue<(Type Type, PropertyAndParent Parent)>(new[] { (typeof(TSettings), (PropertyAndParent)null) });
                while (queue.Count > 0)
                {
                    var typeInfo = queue.Dequeue();
                    var settingGroups = typeInfo.Type.GetProperties().Where(x => x.IsDefined(typeof(SettingsGroupAttribute), false));
                    foreach (var settingGroup in settingGroups)
                    {
                        var settingTypeInfo = (
                            Type: settingGroup.PropertyType,
                            Parent: new PropertyAndParent(settingGroup, typeInfo.Parent)
                        );

                        settingGroupMetadata.Add(
                            new NFigSettingGroupMetadata<TSettings, TTier, TDataCenter>(
                                settingTypeInfo.Type, CreateGetterMethod(settingTypeInfo.Parent)
                            )
                        );

                        queue.Enqueue(settingTypeInfo);
                    }
                }

                return settingGroupMetadata.ToImmutable();
            }

            var serviceProviderMethod = typeof(ServiceProviderServiceExtensions)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(m => m.ContainsGenericParameters && m.Name == nameof(ServiceProviderServiceExtensions.GetServices));

            // once we've found all the setting groups make sure that the container knows about them all
            // here we register:
            // - IOptions<TOptions> & IOptionsSnapshot<TOptions>
            //   We manually register both of these as singletons because NFig 
            //   manually manages the lifetime of its settings and when they are reloaded
            // - IOptionsFactory<TOptions>
            //   A singleton that always resolves to the instance of the type in NFig
            // - IOptionsChangeTokenSource<TOptions>
            //   Ensures that the IOptionsMonitor<TOptions> flushes its cache whenever we reload settings
            foreach (var settingGroup in FindSettingGroups())
            {
                var factoryInterface = typeof(IOptionsFactory<>).MakeGenericType(settingGroup.Type);
                var optionsInterface = typeof(IOptions<>).MakeGenericType(settingGroup.Type);
                var snapshotInterface = typeof(IOptionsSnapshot<>).MakeGenericType(settingGroup.Type);
                var changeTokenSourceInterface = typeof(IOptionsChangeTokenSource<>).MakeGenericType(settingGroup.Type);
                var configureOptionsInterface = typeof(IConfigureOptions<>).MakeGenericType(settingGroup.Type);
                var postConfigureOptionsInterface = typeof(IPostConfigureOptions<>).MakeGenericType(settingGroup.Type);

                var factoryImpl = typeof(NFigOptionsFactory<,,,>).MakeGenericType(typeof(TSettings), typeof(TTier), typeof(TDataCenter), settingGroup.Type);
                var managerImpl = typeof(NFigOptionsManager<>).MakeGenericType(settingGroup.Type);
                var changeTokenSourceImpl = typeof(NFigChangeTokenSource<,,,>).MakeGenericType(typeof(TSettings), typeof(TTier), typeof(TDataCenter), settingGroup.Type);

                services.AddSingleton(optionsInterface, managerImpl);
                services.AddSingleton(snapshotInterface, managerImpl);
                services.AddSingleton(changeTokenSourceInterface, changeTokenSourceImpl);

                var configureOptionsProvider = serviceProviderMethod.MakeGenericMethod(configureOptionsInterface);
                var postConfigureOptionsProvider = serviceProviderMethod.MakeGenericMethod(postConfigureOptionsInterface);
                var implementationCtor = factoryImpl.GetConstructor(
                    new[]
                    {
                        typeof(Func<TSettings, object>), 
                        typeof(IEnumerable<>).MakeGenericType(configureOptionsInterface),
                        typeof(IEnumerable<>).MakeGenericType(postConfigureOptionsInterface)
                    });

                var accessor = settingGroup.Accessor;

                services.AddSingleton(
                    factoryInterface, 
                    serviceProvider => 
                    {
                        var args = new[] { serviceProvider };
                        var configureOptions = configureOptionsProvider.Invoke(null, args);
                        var postConfigureOptions = postConfigureOptionsProvider.Invoke(null, args);
                        return implementationCtor.Invoke(
                            new[]
                            { 
                                accessor,
                                configureOptions, 
                                postConfigureOptions 
                            });
                    }
                );
            }

            // and finally add the settings themselves
            services.AddSingleton<IOptionsFactory<TSettings>>(
                serviceProvider => 
                {
                    var configureOptions = serviceProvider.GetServices<IConfigureOptions<TSettings>>();
                    var postConfigureOptions = serviceProvider.GetServices<IPostConfigureOptions<TSettings>>();
                    return new NFigOptionsFactory<TSettings, TTier, TDataCenter, TSettings>(
                        settings => settings, configureOptions, postConfigureOptions
                    );
                }
            );

            services.AddSingleton<IOptionsChangeTokenSource<TSettings>, NFigChangeTokenSource<TSettings, TTier, TDataCenter, TSettings>>();
            return services;
        }

        private class PropertyAndParent
        {
            public PropertyAndParent(PropertyInfo propertyInfo, PropertyAndParent parent)
            {
                Property = propertyInfo;
                Parent = parent;
            }

            public PropertyAndParent Parent { get; set; }
            public PropertyInfo Property { get; set; }
        }
    }
}
