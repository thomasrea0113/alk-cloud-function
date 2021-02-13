using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ALK.Services;
using Google.Apis.YouTube.v3;
using Google.Cloud.Functions.Framework;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ALK
{
    public class Function : IHttpFunction
    {
#pragma warning disable CS8618

        [Inject]
        public IOptions<AppConfig> Config { get; set; }

        [Inject]
        public ISmsSender SmsSender { get; set; }

        [Inject]
        public ILogger<Function> Logger { get; set; }

        [Inject]
        public YouTubeService TheTubes { get; set; }

        [Inject]
        public IJsonStreamSerializer Serializer { get; set; }

        public IServiceProvider Services { get; }

        public Function()
        {
            Services = ServiceExtensions.BuildServiceProvider();

            var needsInjection = GetType().GetProperties()
                .Where(p => p.GetCustomAttribute<InjectAttribute>() != null);

            foreach (var prop in needsInjection)
                prop.SetValue(this, Services.GetRequiredService(prop.PropertyType));
        }
#pragma warning restore CS8618

        public async Task HandleAsync(HttpContext context)
        {
            var config = await Serializer.DeserializeStreamAsync<AppConfig>(context.Request.Body)
                .ConfigureAwait(false);

            var skipSend = config.PushBullet?.SkipSend ?? Config.Value.PushBullet?.SkipSend
                ?? throw new NullReferenceException(nameof(AppConfig.PushBullet));
            var allansNumber = config.AllansNumber ?? Config.Value.AllansNumber
                ?? throw new NullReferenceException(nameof(AppConfig.AllansNumber));
            var siteUri = config.SiteUri ?? Config.Value.SiteUri
                ?? throw new NullReferenceException(nameof(AppConfig.SiteUri));

            var random = new Random(Guid.NewGuid().GetHashCode());

            var query = TheTubes.Search.List("snippet");
            query.Q = "Knife Sharpening";
            query.Type = "video";
            var videos = await GetVideoUrls().ToArrayAsync().ConfigureAwait(false);
            var video = videos.Skip((int)(random.NextDouble() * videos.Length)).First();

            var message = $"Checkout this cool knife video!\n\n{video}";
            var message2 = $"If you're tired of these messages, you can unsubscribe using this link: {siteUri}";

            var ei = new EventId(69, "SmsSentToAllan");

            if (!skipSend)
                await SmsSender.SendAsync(message, allansNumber).ConfigureAwait(false);
            Logger.LogInformation(ei, "sms sent to allan at {number}: {message}", allansNumber, message);

            // need a delay between SMS message or the second may not go through
            await Task.Delay(1000);

            if (!skipSend)
                await SmsSender.SendAsync(message2, allansNumber).ConfigureAwait(false);
            Logger.LogInformation(ei, "sms sent to allan at {number}: {message}", allansNumber, message2);
        }

        private async IAsyncEnumerable<string> GetVideoUrls()
        {
            var pageCount = Config.Value.YouTube?.PageCount
                ?? throw new NullReferenceException(nameof(AppConfig.YouTube));

            var query = TheTubes.Search.List("snippet");
            query.Q = "Knife Sharpening";
            query.Type = "video";
            query.MaxResults = 50;

            string? page = null;
            foreach (var i in Enumerable.Range(0, pageCount))
            {
                query.PageToken = page;

                var results = await query.ExecuteAsync().ConfigureAwait(false);

                foreach (var result in results.Items)
                    yield return $"https://www.youtube.com/watch?v={result.Id.VideoId}";

                page = results.NextPageToken;
            }
        }
    }
}