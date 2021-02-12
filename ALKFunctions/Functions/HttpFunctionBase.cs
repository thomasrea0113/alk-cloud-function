using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ALKFunctions.Services;
using Google.Cloud.Functions.Framework;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ALKFunctions.Functions
{
    public abstract class HttpFunctionBase : IHttpFunction
    {
        protected HttpFunctionBase()
        {
            var services = ServiceExtensions.BuildServiceProvider();

            var needsInjection = GetType().GetProperties()
                .Where(p => p.GetCustomAttribute<InjectAttribute>() != null);

            foreach (var prop in needsInjection)
                prop.SetValue(this, services.GetRequiredService(prop.PropertyType));
        }

        public abstract Task HandleAsync(HttpContext context);
    }
}