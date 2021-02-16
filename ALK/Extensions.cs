using System.IO;
using System.Net.Http;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ALK
{
    public static class Extensions
    {
        private static readonly Random _random;

        static Extensions()
        {
            _random = new Random(Guid.NewGuid().GetHashCode());
        }

        public static T RandomItem<T>(this IList<T> items)
            => items[_random.Next(items.Count)];

        public static async Task<T> DeserializeBodyAsync<T>(this Task<HttpResponseMessage> responseTask, bool ensureSuccess = true,
            JsonSerializerSettings? settings = null)
        {
            var response = await responseTask.ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(content, settings) ?? throw new InvalidCastException();
        }
    }
}