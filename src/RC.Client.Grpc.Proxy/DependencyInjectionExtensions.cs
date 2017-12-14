using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.Proxy;
using Rabbit.Cloud.Grpc.ApplicationModels;
using Rabbit.Cloud.Grpc.ApplicationModels.Internal;
using System;

namespace Rabbit.Cloud.Client.Grpc.Proxy
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection InjectionServiceProxy(this IServiceCollection services, IRabbitApplicationBuilder applicationBuilder)
        {
            return services
                .InjectionServiceProxy(applicationBuilder.Build(), applicationBuilder.ApplicationServices);
        }

        public static IServiceCollection InjectionServiceProxy(this IServiceCollection services, RabbitRequestDelegate application, IServiceProvider applicationServices)
        {
            var proxyFactory = new ProxyFactory(new[] { new GrpcProxyInterceptor(application, applicationServices.GetRequiredService<SerializerCacheTable>()) });

            foreach (var serviceModel in applicationServices.GetRequiredService<ApplicationModelHolder>().GetApplicationModel().Services)
            {
                services.AddSingleton(serviceModel.Type, proxyFactory.CreateInterfaceProxy(serviceModel.Type));
            }

            return services;
        }
    }
}