using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Rabbit.Extensions.Configuration.Test
{
    public class TemplateSupportTest
    {
        [Theory(DisplayName = "BasicTest")]
        [InlineData("port", "80")]
        [InlineData("url", "http://localhost:80/User/Get/?p1=${notReplace}&t=${t}")]
        [InlineData("ch:url", "http://localhost:80/User/Get/?p1=${notReplace}&t=${t}_test")]
        [InlineData("ch:c:url", "http://localhost:80/User/Get/?p1=${notReplace}&t=${t}_test_test")]
        public void BasicTest(string key, string value)
        {
            var configuration = GetConfiguration();

            Assert.Equal(value, configuration[key]);
        }

        [Fact(DisplayName = "ReloadOnChangeTest")]
        public void ReloadOnChangeTest()
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "temp.txt");

            try
            {
                void SetName(string name)
                {
                    File.WriteAllText(filePath, JsonConvert.SerializeObject(new
                    {
                        firstName = name,
                        lastName = "johnson",
                        fullName = "${firstName} ${lastName}"
                    }));
                }

                SetName("ben");

                var configuration = new ConfigurationBuilder()
                    .AddJsonFile(filePath, false, true)
                    .Build()
                    .EnableTemplateSupport();

                Assert.Equal("ben johnson", configuration["fullName"]);

                var autoResetEvent = new AutoResetEvent(false);
                configuration.GetReloadToken().RegisterChangeCallback(async s =>
                {
                    await Task.Delay(50);
                    autoResetEvent.Set();
                }, null);

                SetName("michael");
                autoResetEvent.WaitOne(5000);

                //new value
                Assert.Equal("michael johnson", configuration["fullName"]);
            }
            finally
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        [Fact(DisplayName = "CyclicDependencyTest")]
        public void CyclicDependencyTest()
        {
            var data = new Dictionary<string, string>
            {
                {"key1", "${key3}"},
                {"key2", "${key1}"},
                {"key3", "${key2}"}
            };
            var exception = Assert.Throws<ArgumentException>("key1", () =>
               {
                   GetConfiguration(data);
               });
            Assert.Equal("cyclic dependency 'key1 > key3 > key2 > key1'.\r\nParameter name: key1", exception.Message);
        }

        [Theory(DisplayName = "VariableMissingTest")]
        [InlineData(VariableMissingAction.ThrowException, null)]
        [InlineData(VariableMissingAction.UseKey, "http://localhost:80/User/Get/?p1=${notReplace}&t=${t}")]
        [InlineData(VariableMissingAction.UseEmpty, "http://localhost:80/User/Get/?p1=${notReplace}&t=")]
        public void VariableMissingTest(VariableMissingAction variableMissingAction, string value)
        {
            IConfiguration Init()
            {
                return GetConfiguration(configure: c => { c.VariableMissingAction = variableMissingAction; });
            }

            if (variableMissingAction == VariableMissingAction.ThrowException)
            {
                var exception = Assert.Throws<ArgumentException>((Func<IConfiguration>)Init);
                Assert.Equal("missing key 't'.", exception.Message);
            }
            else
            {
                var configuration = Init();
                Assert.Equal(value, configuration["url"]);
            }
        }

        #region Private Method

        private static IConfigurationRoot GetConfiguration(IDictionary<string, string> memoryConfiguration = null, Action<TemplateSupportOptions> configure = null)
        {
            if (memoryConfiguration == null)
                memoryConfiguration = GetMemoryConfiguration();

            return new ConfigurationBuilder()
                .AddInMemoryCollection(memoryConfiguration)
                .Build()
                .EnableTemplateSupport(configure);
        }

        private static Dictionary<string, string> GetMemoryConfiguration()
        {
            return new Dictionary<string, string>
            {
                {"url", "http://${service:host}:${port}/${controller}/${action}/?p1=\\${notReplace}&t=${t}"},
                {"service:host", "localhost"},
                {"port", "80"},
                {"controller", "User"},
                {"action", "Get"},
                {"ch:url","${url}_test" },
                {"ch:c:url","${ch:url}_test" }
            };
        }

        #endregion Private Method
    }
}