using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace ALK.Services
{
    public static class SmsSenderEvents
    {
        public static readonly EventId SmsSent = new EventId(69, nameof(SmsSent));
        public static readonly EventId SmsSkipped = new EventId(70, nameof(SmsSkipped));
    }

    public interface ISmsSender
    {
        bool SkipSend { get; set; }
        Task<PushBulletSendSmsRequest> SendAsync(string message, params string[] to);
        Task<bool> AwaitCompleteAsync(PushBulletSendSmsRequest message);
    }

    public class PushBulletSmsSender : ISmsSender
    {
        private readonly HttpClient _client;
        private readonly ILogger<ISmsSender> _logger;
        private readonly string _device;

        private readonly JsonSerializerSettings _serializerOptions = new()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            },
            Converters = { new StringEnumConverter(new SnakeCaseNamingStrategy(), false) },
            NullValueHandling = NullValueHandling.Ignore
        };

        public bool SkipSend { get; set; }

        public PushBulletSmsSender(HttpClient client, IOptionsMonitor<AppConfig> config, ILogger<ISmsSender> logger)
        {
            _client = client;
            _logger = logger;

            _device = config.CurrentValue.PushBullet?.DeviceIdentifier
                ?? throw new NullReferenceException(nameof(PushBulletConfig.DeviceIdentifier));
            SkipSend = config.CurrentValue.PushBullet?.SkipSend
                ?? throw new NullReferenceException(nameof(PushBulletConfig.SkipSend));
        }

        public async Task<PushBulletSendSmsRequest> SendAsync(string message, params string[] to)
        {
            var request = new PushBulletSendSmsRequest
            {
                Data = new()
                {
                    TargetDeviceIden = _device,
                    Addresses = to,
                    Message = message
                }
            };

            var stringNumbers = string.Join(", ", to);

            if (!SkipSend)
            {
                var response = await _client.PostAsync("texts", CreateContent(request))
                    .DeserializeBodyAsync<PushBulletSendSmsRequest>(settings: _serializerOptions)
                    .ConfigureAwait(false);

                _logger.LogInformation(SmsSenderEvents.SmsSent, "sms sent to {number}: {message}", stringNumbers, message);
                return response;
            }

            _logger.LogInformation(SmsSenderEvents.SmsSkipped, "skipped sending sms to {number}: {message}", stringNumbers, message);

            // The API returns the data data structure that it sends, so we can just reuse the request
            request.Data.Status = SmsStatus.Sent;
            request.Active = false;
            return request;
        }

        public Task<bool> AwaitCompleteAsync(PushBulletSendSmsRequest message)
        {
            // var response = await _client.PostAsync("texts", CreateContent(message))
            //     .DeserializeBodyAsync<PushBulletSendSmsRequest>(settings: _serializerOptions)
            //     .ConfigureAwait(false);
            // return true;
            // TODO implement

            return Task.FromResult(true);
        }

        private StringContent CreateContent(object content)
        {
            var serialized = JsonConvert.SerializeObject(content, _serializerOptions);
            return new StringContent(serialized, Encoding.UTF8, MediaTypeNames.Application.Json);
        }
    }

    public enum SmsStatus
    {
        Queued,
        Sent,
        Failed
    }

    public class PushBulletSendSmsRequest
    {

        public bool? Active { get; set; }
        public string? Iden { get; set; }
        public PushBulletSendSmsRequestData? Data { get; set; }
    }

    public class PushBulletSendSmsRequestData
    {
        public string? TargetDeviceIden { get; set; }
        public IList<string> Addresses { get; set; } = Array.Empty<string>();
        public string? Message { get; set; }
        public Guid? Guid { get; set; } = System.Guid.NewGuid();
        public SmsStatus? Status { get; set; }
    }
}