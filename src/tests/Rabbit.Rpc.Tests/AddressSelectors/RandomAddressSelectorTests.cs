using Rabbit.Rpc.Address;
using Rabbit.Rpc.Runtime.Client.Address.Resolvers.Implementation.Selectors;
using Rabbit.Rpc.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Rabbit.Rpc.Tests.AddressSelectors
{
    public class RandomAddressSelectorTests
    {
        [Fact]
        public async void RandomAddressTest()
        {
            IAddressSelector selector = new RandomAddressSelector();

            var context = GetSelectContext();

            var list = new List<AddressModel>();
            for (var i = 0; i < 100; i++)
                list.Add(await selector.SelectAsync(context));

            Assert.True(list.Distinct().Count() > 1);

            selector = new RandomAddressSelector((min, max) => 0);

            for (var i = 0; i < 100; i++)
                Assert.Equal(context.Address.First(), await selector.SelectAsync(context));
        }

        private static AddressSelectContext GetSelectContext()
        {
            return new AddressSelectContext
            {
                Address = Enumerable.Range(1, 100).Select(i => new IpAddressModel("127.0.0.1", i)),
                Descriptor = new ServiceDescriptor
                {
                    Id = "service1"
                }
            };
        }
    }
}