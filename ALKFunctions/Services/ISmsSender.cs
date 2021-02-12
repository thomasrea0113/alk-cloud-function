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
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ALKFunctions.Services
{
    public interface ISmsSender
    {
        Task SendAsync(string message, params string[] to);
    }

    public class PushBulletSmsSender : ISmsSender
    {
        private readonly HttpClient _client;
        private readonly string _device;

        private readonly JsonSerializerSettings _serializerOptions = new()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };

        public PushBulletSmsSender(HttpClient client, IOptionsMonitor<AppConfig> config)
        {
            _client = client;
            _device = config.CurrentValue.PushBullet?.DeviceIdentifier
                ?? throw new NullReferenceException(nameof(PushBulletConfig.DeviceIdentifier));
        }

        public async Task SendAsync(string message, params string[] to)
        {
            var content = CreateContent(new
            {
                data = new PushBulletSendSmsRequest
                (
                    targetDeviceIden: _device,
                    addresses: to,
                    message: message
                )
            });
            var response = await _client.PostAsync("texts", content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }

        private StringContent CreateContent(object content)
        {
            var serialized = JsonConvert.SerializeObject(content, _serializerOptions);
            return new StringContent(serialized, Encoding.UTF8, MediaTypeNames.Application.Json);
        }
    }

    public record PushBulletSendSmsRequest
    {
        public string TargetDeviceIden { get; }
        public IEnumerable<string> Addresses { get; }
        public string Message { get; }

        public PushBulletSendSmsRequest(string targetDeviceIden, IEnumerable<string> addresses, string message)
            => (TargetDeviceIden, Addresses, Message) = (targetDeviceIden, addresses, message);
    }
}