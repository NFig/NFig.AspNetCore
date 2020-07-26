using System;
using System.Threading;
using Microsoft.AspNetCore.Hosting;

namespace NFig.AspNetCore.Tests
{
    public class NFigFixture<TStartup> : IDisposable where TStartup : Startup
    {
        public IWebHost Host { get; }
        
        public NFigFixture()
        {
            var host = new WebHostBuilder().UseStartup<TStartup>().UseServer(new NoOpServer()).Build();
            var t = host.StartAsync();
            // minimal attempt to make sure host starts successfully
            for (var i = 0; i < 5; i++)
            {
                Thread.Sleep(100);
                
                if (t.IsFaulted)
                {
                    // force exception to be thrown
                    t.Wait();
                }
            }
            
            Host = host;
        }

        public void Dispose() => Host.Dispose();
    }
}
