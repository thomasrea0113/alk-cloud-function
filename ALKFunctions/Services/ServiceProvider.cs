using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ALKFunctions
{
    public static class Services
    {
        public static IServiceProvider BuildServiceProvider()
        {
            var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

            var config = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json", optional: false)
                .AddJsonFile($"appSettings.secret.json", optional: false)
                .AddJsonFile($"appSettings.{env}.json", optional: true)
                .AddJsonFile($"appSettings.{env}.secret.json", optional: true)
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .Build();

            var services = new ServiceCollection()
                .AddSingleton<IConfiguration>(_ => config)
                .AddLogging(b => b.AddConsole().AddConfiguration(config.GetSection("Logging")));


            services.AddOptions<AppConfig>().Bind(config.GetSection("AppConfig"));

            return services.BuildServiceProvider();
        }
    }
}