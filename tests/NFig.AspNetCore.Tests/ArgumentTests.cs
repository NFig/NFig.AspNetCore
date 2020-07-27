using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace NFig.AspNetCore.Tests
{
    public class ArgumentTests
    {
        [Fact]
        public void NullApplicationThrows()
        {
            Assert.Throws<ArgumentNullException>(
                "applicationName",
                () =>
                {
                    var configuration = new ConfigurationBuilder().Build();
                    var services = new ServiceCollection()
                        .AddSingleton<IConfiguration>(configuration)
                        .AddNFig<Settings, Tier, DataCenter>()
                        .BuildServiceProvider();

                    var appBuilder = new ApplicationBuilder(services);

                    appBuilder.UseNFig<Settings, Tier, DataCenter>(
                        (configuration, builder) =>
                        {
                            builder.UseRedis(null, Tier.Local, DataCenter.Local, null);
                        }
                    );
                }
            );
        }
    }
}
