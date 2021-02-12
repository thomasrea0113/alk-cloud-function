using System;
using System.Net.Http.Headers;
using System.Net.Mime;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ALKFunctions.Services
{
    public static class ServiceExtensions
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
                .AddLogging(b => b.AddConsole().AddConfiguration(config.GetSection("Logging")))
                .AddYouTubeService();


            services.AddOptions<AppConfig>().Bind(config.GetSection(nameof(AppConfig)));
            services.AddPushBulletSmsSender();

            return services.BuildServiceProvider();
        }

        public static IHttpClientBuilder AddPushBulletSmsSender(this IServiceCollection services)
            => services.AddHttpClient<ISmsSender, PushBulletSmsSender>((services, client) =>
            {
                var pushBulletApiKey = services.GetRequiredService<IOptions<AppConfig>>().Value.PushBullet?.ApiKey
                     ?? throw new NullReferenceException(nameof(PushBulletConfig.ApiKey));
                client.BaseAddress = new Uri("https://api.pushbullet.com/v2/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
                client.DefaultRequestHeaders.Add("Access-Token", pushBulletApiKey);
            });

        public static IServiceCollection AddYouTubeService(this IServiceCollection services)
            => services.AddScoped(p =>
        {
            var config = p.GetRequiredService<IOptions<AppConfig>>().Value;
            return new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = config.YouTube?.ApiKey ?? throw new NullReferenceException(nameof(YouTubeConfig.ApiKey))
            });
        });
    }
}