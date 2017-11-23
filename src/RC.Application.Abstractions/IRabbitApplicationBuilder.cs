using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Application.Abstractions
{
    public interface IRabbitApplicationBuilder
    {
        IServiceProvider ApplicationServices { get; set; }
        IDictionary<string, object> Properties { get; }

        RabbitRequestDelegate Build();

        IRabbitApplicationBuilder New();

        IRabbitApplicationBuilder Use(Func<RabbitRequestDelegate, RabbitRequestDelegate> middleware);
    }
}