using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using Xunit;
using static Rabbit.Cloud.Client.Abstractions.Utilities.ClientUtilities;

namespace RC.Client.Abstractions.Test
{
    public class ClientUtilitiesTest
    {
        [Fact]
        public void ParseQueryTest()
        {
            void BasicCheck(IDictionary<string, StringValues> query)
            {
                Assert.Equal("1,2", query["a"]);
                Assert.Equal("2", query["b"]);
                Assert.Equal("1,2,3", query["c"]);
            }

            BasicCheck(ParseQuery("?a=1&b=2&a=2&c=1,2,3"));
            BasicCheck(ParseQuery("a=1&b=2&a=2&c=1,2,3"));
            BasicCheck(ParseQuery("http://localhost:99/Test?a=1&b=2&a=2&c=1,2,3"));

            Assert.Equal(string.Empty, ParseQuery("a=")["a"]);
            Assert.False(ParseQuery("a").ContainsKey("a"));

            Assert.NotNull(ParseQuery(null));
            Assert.NotNull(ParseQuery(""));
        }

        [Fact]
        public void ParseNullableQueryTest()
        {
            Assert.Null(ParseNullableQuery(null));
            Assert.Null(ParseNullableQuery(""));
        }

        [Fact]
        public void SplitPathAndQueryTest()
        {
            var result = SplitPathAndQuery("/user/get?id=1&name=ben");

            Assert.Equal("/user/get", result.Path);

            Assert.NotNull(result.Query);

            Assert.Equal("1", result.Query["id"]);
            Assert.Equal("ben", result.Query["name"]);


            result = SplitPathAndQuery("/user/get");
            Assert.Equal("/user/get", result.Path);
            Assert.Null(result.Query);

            result = SplitPathAndQuery("/user/get?");
            Assert.Equal("/user/get", result.Path);
            Assert.Null(result.Query);

        }
    }
}