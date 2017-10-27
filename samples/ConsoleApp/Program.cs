using Google.Protobuf;
using Grpc.Core;
using Helloworld;
using Rabbit.Cloud.Grpc.Abstractions.Utilities;
using Rabbit.Cloud.Grpc.Server;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public class Program
    {
        public class TestService
        {
            /*            public Task<HelloReply> Hello(HelloRequest request, ServerCallContext context)
                        {
                            return Task.FromResult(new HelloReply
                            {
                                Message = "hello " + request.Name
                            });
                        }*/

            public Task<HelloReply> Hello(HelloRequest request)
            {
                return Task.FromResult(new HelloReply
                {
                    Message = "hello " + request.Name
                });
            }
        }

        private static async Task Main(string[] args)
        {
            var methodInfo = typeof(TestService).GetMethod("Hello");
            var requestType = typeof(HelloRequest);
            var responseType = typeof(HelloReply);
            object requestMarshaller =
                Marshallers.Create(t => t.ToByteArray(), data => HelloRequest.Parser.ParseFrom(data));
            object responseMarshaller =
                Marshallers.Create(t => t.ToByteArray(), data => HelloReply.Parser.ParseFrom(data));

            requestMarshaller = MarshallerUtilities.CreateMarshaller(requestType, model => ((IMessage)model).ToByteArray(), data => HelloRequest.Parser.ParseFrom(data));
            responseMarshaller = MarshallerUtilities.CreateMarshaller(responseType, model => ((IMessage)model).ToByteArray(), data => HelloReply.Parser.ParseFrom(data));

            var method = MethodUtilities.CreateMethod(methodInfo, requestType, responseType, requestMarshaller, responseMarshaller);
            return;
            var provider = new DefaultServerServiceDefinitionProvider(new DefaultServerServiceDefinitionProviderOptions
            {
                Factory = Activator.CreateInstance,
                Types = new[] { typeof(TestService) }
            });

            var def = provider.GetDefinitions().First();

            var server = new Server
            {
                Services = { def },
                Ports = { new ServerPort("localhost", 9909, ServerCredentials.Insecure) }
            };

            server.Start();

            var channel = new Channel("localhost", 9909, ChannelCredentials.Insecure);
            var invoker = new DefaultCallInvoker(channel);

            var r = invoker.BlockingUnaryCall(
                (Method<HelloRequest, HelloReply>)new MethodFactory(new MarshallerFactory()).GetMethod(typeof(TestService).GetMethod("Hello"),
                    typeof(HelloRequest), typeof(HelloReply)), null,
                new CallOptions(), new HelloRequest { Name = "majian" });

            Console.WriteLine(r.Message);
        }
    }
}