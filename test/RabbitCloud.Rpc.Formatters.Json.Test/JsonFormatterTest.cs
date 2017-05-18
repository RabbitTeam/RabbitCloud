using RabbitCloud.Rpc.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RabbitCloud.Rpc.Formatters.Json.Test
{
    public class JsonFormatterTest
    {
        public class UserInfo
        {
            public string Name { get; set; }
        }

        [Fact]
        public void FormatterTest()
        {
            var requestFormatter = new JsonRequestFormatter();

            var request = new Request(new Dictionary<string, string>
            {
                {"a", "1"},
                {"b", "2"}
            })
            {
                Arguments = new object[] { 1, 2f, DateTime.Now, new UserInfo { Name = "name" } },
                MethodKey = new MethodKey
                {
                    Name = "method",
                    ParamtersDesc = "Int64[]"
                },
                RequestId = 300
            };
            var data = requestFormatter.OutputFormatter.Format(request);

            var formatterRequest = requestFormatter.InputFormatter.Format(data);

            Assert.Equal(request.MethodKey, formatterRequest.MethodKey);
            Assert.Equal(request.RequestId, formatterRequest.RequestId);
            Assert.Equal(string.Join(",", request.Attachments.Keys), string.Join(",", formatterRequest.Attachments.Keys));
            Assert.Equal(string.Join(",", request.Attachments.Values), string.Join(",", formatterRequest.Attachments.Values));
            Assert.Equal(string.Join(",", request.Arguments.SelectMany(i => new[] { i.GetType().Name, i.ToString() })), string.Join(",", formatterRequest.Arguments.SelectMany(i => new[] { i.GetType().Name, i.ToString() })));
        }
    }
}