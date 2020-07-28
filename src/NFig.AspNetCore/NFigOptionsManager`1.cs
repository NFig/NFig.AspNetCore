using Microsoft.Extensions.Options;

namespace NFig.AspNetCore
{
    /// <summary>
    /// Implementation of <see cref="IOptions{TOptions}"/> and <see cref="IOptionsSnapshot{TOptions}"/>
    /// that always gets the latest copy of settings from NFig. This mitigates caching that the options
    /// framework performs on our behalf.
    /// </summary>
    internal class NFigOptionsManager<TOptions> : IOptions<TOptions>, IOptionsSnapshot<TOptions> where TOptions : class, new()
    {
        private readonly IOptionsMonitor<TOptions> _monitor;

        public NFigOptionsManager(IOptionsMonitor<TOptions> monitor)
        {
            _monitor = monitor;
        }

        public TOptions Value => Get(Options.DefaultName);

        public TOptions Get(string name) => _monitor.Get(name);
    }
}
