using Rabbit.Rpc.Address;
using System.Linq;
using Xunit;

namespace Rabbit.Rpc.Tests
{
    public class AddressModelEqualsTests
    {
        [Fact]
        public void IpAddressModelEqualsTest()
        {
            AddressModel model1 = new IpAddressModel("127.0.0.1", 1234);
            AddressModel model2 = new IpAddressModel("127.0.0.1", 1234);
            AddressModel model3 = new IpAddressModel("127.0.0.1", 12345);
            AddressModel model4 = new IpAddressModel("127.0.0.2", 1234);
            AddressModel model5 = new IpAddressModel("127.0.0.2", 12345);

            Assert.True(model1.Equals(model2));
            Assert.True(model1 == model2);

            Assert.False(!model1.Equals(model2));
            Assert.False(model1 != model2);

            Assert.False(model1.Equals(model3));
            Assert.False(model1 == model3);
            Assert.False(model1.Equals(model4));
            Assert.False(model1 == model4);
            Assert.False(model1.Equals(model5));
            Assert.False(model1 == model5);

            Assert.True(!model1.Equals(model3));
            Assert.True(model1 != model3);
            Assert.True(!model1.Equals(model4));
            Assert.True(model1 != model4);
            Assert.True(!model1.Equals(model5));
            Assert.True(model1 != model5);

            var array1 = new[] { new IpAddressModel("127.0.0.1", 1234), new IpAddressModel("127.0.0.2", 1234) };
            var array2 = new[] { new IpAddressModel("127.0.0.1", 1234), new IpAddressModel("127.0.0.2", 1234) };

            Assert.False(array1.Except(array2).Any());
            Assert.Equal(2, array1.Intersect(array2).Count());
        }
    }
}