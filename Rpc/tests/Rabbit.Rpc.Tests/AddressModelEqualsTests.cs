using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rabbit.Rpc.Address;
using System.Linq;

namespace Rabbit.Rpc.Tests
{
    [TestClass]
    public class AddressModelEqualsTests
    {
        [TestMethod]
        public void IpAddressModelEqualsTest()
        {
            AddressModel model1 = new IpAddressModel("127.0.0.1", 1234);
            AddressModel model2 = new IpAddressModel("127.0.0.1", 1234);
            AddressModel model3 = new IpAddressModel("127.0.0.1", 12345);
            AddressModel model4 = new IpAddressModel("127.0.0.2", 1234);
            AddressModel model5 = new IpAddressModel("127.0.0.2", 12345);

            Assert.IsTrue(model1.Equals(model2));
            Assert.IsTrue(model1 == model2);

            Assert.IsFalse(!model1.Equals(model2));
            Assert.IsFalse(model1 != model2);

            Assert.IsFalse(model1.Equals(model3));
            Assert.IsFalse(model1 == model3);
            Assert.IsFalse(model1.Equals(model4));
            Assert.IsFalse(model1 == model4);
            Assert.IsFalse(model1.Equals(model5));
            Assert.IsFalse(model1 == model5);

            Assert.IsTrue(!model1.Equals(model3));
            Assert.IsTrue(model1 != model3);
            Assert.IsTrue(!model1.Equals(model4));
            Assert.IsTrue(model1 != model4);
            Assert.IsTrue(!model1.Equals(model5));
            Assert.IsTrue(model1 != model5);

            var array1 = new[] { new IpAddressModel("127.0.0.1", 1234), new IpAddressModel("127.0.0.2", 1234) };
            var array2 = new[] { new IpAddressModel("127.0.0.1", 1234), new IpAddressModel("127.0.0.2", 1234) };

            Assert.IsFalse(array1.Except(array2).Any());
            Assert.AreEqual(2, array1.Intersect(array2).Count());
        }
    }
}