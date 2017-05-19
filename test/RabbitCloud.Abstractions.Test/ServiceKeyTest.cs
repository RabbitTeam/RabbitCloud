using System.Collections.Generic;
using Xunit;

namespace RabbitCloud.Abstractions.Test
{
    public class ServiceKeyTest
    {
        [Fact]
        public void EqualTest()
        {
            var key = new ServiceKey("test");

            Assert.Equal(key, new ServiceKey("test"));
        }

        [Fact]
        public void DictionaryEqualTest()
        {
            var dictionary = new Dictionary<ServiceKey, long>
            {
                [new ServiceKey("test")] = 1,
                [new ServiceKey("test")] = 2
            };

            Assert.Equal(1, dictionary.Count);
        }

        [Fact]
        public void ToStringTest()
        {
            var key = new ServiceKey("test");
            Assert.Equal("unknown/test/latest", key.ToString());

            key = new ServiceKey("group", "test", "2.0");

            Assert.Equal("group/test/2.0", key.ToString());
        }
    }
}