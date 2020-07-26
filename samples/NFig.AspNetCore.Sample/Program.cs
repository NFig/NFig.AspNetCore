using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NFig.AspNetCore.Sample.Configuration;

namespace NFig.AspNetCore.Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(
                    (hostingContext, config) =>
                    {
                        config.AddJsonFile("appSettings.json", optional: false, reloadOnChange: true);
                    }
                )
                .UseStartup<Startup>();
        }
    }
}
