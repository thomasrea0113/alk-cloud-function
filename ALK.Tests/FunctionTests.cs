using System.IO;
using System;
using System.Threading.Tasks;
using ALK;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;
using ALK.Services;

namespace ALKTests
{
    public class FunctionTests
    {
        [SetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Test");
        }

        [Test]
        public async Task TestFunction()
        {
            var alk = new Function();
            var context = new DefaultHttpContext();
            context.Request.Body = new MemoryStream();

            var serializer = alk.Services.GetRequiredService<IJsonStreamSerializer>();
            await serializer.SerializeToStreamAsync(new AppConfig
            {
                PushBullet = new() { SkipSend = false }
            }, context.Request.Body).ConfigureAwait(false);
            context.Request.Body.Seek(0, 0);
            context.Request.ContentLength = context.Request.Body.Length;

            await alk.HandleAsync(context).ConfigureAwait(false);
            Assert.Pass();
        }
    }
}