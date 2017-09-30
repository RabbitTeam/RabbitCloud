using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Client.Abstractions
{
    public interface IRabbitApplicationBuilder<TContext>
    {
        IServiceProvider ApplicationServices { get; set; }
        IDictionary<string, object> Properties { get; }

        RabbitRequestDelegate<TContext> Build();

        IRabbitApplicationBuilder<TContext> New();

        IRabbitApplicationBuilder<TContext> Use(Func<RabbitRequestDelegate<TContext>, RabbitRequestDelegate<TContext>> middleware);
    }
}