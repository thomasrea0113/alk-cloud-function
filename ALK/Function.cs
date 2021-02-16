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

        private readonly string[] _greetings = {
            "How ya durrin",
            "How you is",
            "What it do",
            "What's good wit ya",
            "howdy",
            "What it is",
            "Suh",
        };

        private readonly string[] _names = {
            "Hombre",
            "brutta",
            "bruh",
            "Darlin",
            "Browski",
        };

        public async Task HandleAsync(HttpContext context)
        {
            var requestConfig = await Serializer.DeserializeStreamAsync<AppConfig>(context.Request.Body)
                .ConfigureAwait(false);

            // due to API limits, we may want to temporarily disable api requests
            if (requestConfig.PushBullet?.SkipSend is bool smsSend)
                SmsSender.SkipSend = smsSend;

            var skipYouTubeSend = requestConfig.YouTube?.SkipSend ?? Config.Value.YouTube?.SkipSend
                ?? throw new NullReferenceException(nameof(AppConfig.YouTube));

            var pageCount = requestConfig.YouTube?.PageCount ?? Config.Value.YouTube?.PageCount
                ?? throw new NullReferenceException(nameof(AppConfig.YouTube));
            var numbers = requestConfig.Numbers ?? Config.Value.Numbers
                ?? throw new NullReferenceException(nameof(AppConfig.Numbers));
            var siteUri = requestConfig.SiteUri ?? Config.Value.SiteUri
                ?? throw new NullReferenceException(nameof(AppConfig.SiteUri));

            var random = new Random(Guid.NewGuid().GetHashCode());

            var videos = await GetVideoUrls(pageCount, skipYouTubeSend).ToArrayAsync().ConfigureAwait(false);
            var video = videos.RandomItem();

            var message = $"{_greetings.RandomItem()}, {_names.RandomItem()}! Checkout this cool knife sharpening video!\n\n{video}";
            var message2 = $"If you're tired of these messages, you can unsubscribe using this link: {siteUri}";

            // send each message as a separate sms, rather than a single mms
            foreach (var number in numbers)
            {
                var result = await SmsSender.SendAsync(message, number).ConfigureAwait(false);
                await SmsSender.AwaitCompleteAsync(result).ConfigureAwait(false);

                // need a delay between SMS message or the second may not go through
                await Task.Delay(1000);

                await SmsSender.SendAsync(message2, number).ConfigureAwait(false);

                await Task.Delay(1000);
            }
        }

        private async IAsyncEnumerable<string> GetVideoUrls(int pageCount, bool skipSend)
        {
            var maxResults = 50;
            var query = TheTubes.Search.List("snippet");
            query.Q = "Knife Sharpening";
            query.Type = "video";
            query.MaxResults = maxResults;

            string ToUrl(string videoId) => $"https://www.youtube.com/watch?v={videoId}";

            if (!skipSend)
            {
                string? page = null;
                foreach (var i in Enumerable.Range(0, pageCount))
                {
                    query.PageToken = page;

                    var results = await query.ExecuteAsync().ConfigureAwait(false);

                    foreach (var result in results.Items)
                        yield return ToUrl(result.Id.VideoId);

                    page = results.NextPageToken;
                }
                yield break;
            }

            foreach (var i in Enumerable.Range(1, pageCount * maxResults))
                yield return ToUrl($"Video{i}");
        }
    }
}