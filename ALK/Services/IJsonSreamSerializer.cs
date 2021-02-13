using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace ALK.Services
{
    public interface IJsonStreamSerializer
    {
        Task SerializeToStreamAsync<T>(T obj, Stream toStream);
        Task<T> DeserializeStreamAsync<T>(Stream stream);
    }

    public class JsonStreamSerializer : IJsonStreamSerializer
    {
        private readonly JsonSerializerOptions? _options;

        public async Task<T> DeserializeStreamAsync<T>(Stream stream)
            => await JsonSerializer.DeserializeAsync<T>(stream, _options).ConfigureAwait(false);

        public async Task SerializeToStreamAsync<T>(T obj, Stream toStream)
        {
            await JsonSerializer.SerializeAsync<T>(toStream, obj, _options);
        }
    }
}