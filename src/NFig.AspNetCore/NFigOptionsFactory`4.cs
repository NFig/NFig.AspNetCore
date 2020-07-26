using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace NFig.AspNetCore
{
    /// <summary>
    /// Implementation of <see cref="IOptionsFactory{TOptions}"/> that resolves
    /// <see cref="IOptions{TOptions}"/>  implementations using the settings
    /// provided by an NFig store.
    /// </summary>
    internal class NFigOptionsFactory<TSettings, TTier, TDataCenter, TOptions> : IOptionsFactory<TOptions> 
        where TSettings : class, INFigSettings<TTier, TDataCenter>, new()
        where TTier : struct, Enum
        where TDataCenter : struct, Enum
        where TOptions : class, new()
    {
        private readonly Func<TSettings, object> _accessor;
        private readonly IEnumerable<IConfigureOptions<TOptions>> _setups;
        private readonly IEnumerable<IPostConfigureOptions<TOptions>> _postConfigures;

        /// <summary>
        /// Initializes a new instance with the specified options configurations.
        /// </summary>
        /// <param name="accessor">Accessor used to retrieve the settings from the </param>
        /// <param name="setups">The configuration actions to run.</param>
        /// <param name="postConfigures">The initialization actions to run.</param>
        public NFigOptionsFactory(Func<TSettings, object> accessor, IEnumerable<IConfigureOptions<TOptions>> setups, IEnumerable<IPostConfigureOptions<TOptions>> postConfigures)
        {
            _accessor = accessor;
            _setups = setups;
            _postConfigures = postConfigures;
        }

        public TOptions Create(string name)
        {
            if (name != Options.DefaultName)
            {
                throw new NotSupportedException("Named options are not supported by NFig");
            }

            if (!NFigSettingsCache.TryGet<TSettings, TTier, TDataCenter>(out var store))
            {
                throw new InvalidOperationException($"Options for NFig settings of type {typeof(TSettings)} are not configured");
            }

            var options = (TOptions)_accessor(store.Settings);
            foreach (var setup in _setups)
            {
                setup.Configure(options);
            }

            foreach (var post in _postConfigures)
            {
                post.PostConfigure(name, options);
            }

            // if (_validations != null)
            // {
            //     var failures = new List<string>();
            //     foreach (var validate in _validations)
            //     {
            //         var result = validate.Validate(name, options);
            //         if (result.Failed)
            //         {
            //             failures.Add(result.FailureMessage);
            //         }
            //     }
            //     if (failures.Count > 0)
            //     {
            //         throw new OptionsValidationException(name, typeof(TOptions), failures);
            //     }
            // }

            return options;
        }
    }
}
