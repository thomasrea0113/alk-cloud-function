using System;
using System.Threading.Tasks;
using ALKFunctions.Functions;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;

namespace ALKFunctionsTests
{
    public class ALKFunctionsTests
    {
        [SetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Test");
        }

        [Test]
        public async Task TestALKFunction()
        {
            var alk = new ALKFunction();
            await alk.HandleAsync(new DefaultHttpContext()).ConfigureAwait(false);
            Assert.Pass();
        }
    }
}