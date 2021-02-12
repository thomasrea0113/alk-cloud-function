using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ALKFunctions.Functions
{
    public class ALKFunction : HttpFunctionBase
    {
#pragma warning disable CS8618
        [Inject]
        public IOptions<AppConfig> Config { get; set; }

        [Inject]
        public ILogger<ALKFunction> Logger { get; set; }
#pragma warning restore CS8618

        public override Task HandleAsync(HttpContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}