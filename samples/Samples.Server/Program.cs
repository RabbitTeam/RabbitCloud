using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Grpc;
using Rabbit.Cloud.Grpc.Abstractions.Server;
using Rabbit.Cloud.Serialization.Json;
using Rabbit.Cloud.Serialization.MessagePack;
using Rabbit.Cloud.Serialization.Protobuf;
using Rabbit.Cloud.Server.Grpc;
using Samples.Service;
using System;

namespace Samples.Server
{
    public class ApplicationInfo
    {
        public string ServiceName { get; set; }
        public string HostName { get; set; }
        public int HostPort { get; set; }
    }

    internal class Program
    {
        private static ApplicationInfo ApplicationInfo { get; set; }

        private static void Main(string[] args)
        {
            ApplicationInfo = BuildConfiguration(args).Get<ApplicationInfo>();
            {
                IServiceProvider services = new ServiceCollection()
                    .AddLogging()
                    .AddOptions()
                    .AddSingleton<ITestService, TestService>()
                    .AddGrpcServer(options =>
                    {
                        options
                            .Serializers
                            .AddProtobufSerializer()
                            .AddMessagePackSerializer()
                            .AddJsonSerializer();
                    })
                    .AddServerGrpc()
                    .BuildServiceProvider();

                var serverServiceDefinitionTable = services.GetRequiredService<IServerServiceDefinitionTableProvider>().ServerServiceDefinitionTable;

                {
                    var server = new Grpc.Core.Server
                    {
                        Ports = { new ServerPort(ApplicationInfo.HostName, ApplicationInfo.HostPort, ServerCredentials.Insecure) }
                    };

                    foreach (var definition in serverServiceDefinitionTable)
                    {
                        server.Services.Add(definition);
                    }
                    server.Start();
                }

                Console.WriteLine("press key exit...");
                Console.ReadLine();
            }
        }

        private static IConfiguration BuildConfiguration(string[] args)
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
        }
    }
}