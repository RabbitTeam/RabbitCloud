using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Abstractions
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