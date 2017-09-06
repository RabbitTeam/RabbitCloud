using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Facade.Abstractions
{
    public interface IFacadeApplicationBuilder
    {
        IServiceProvider ApplicationServices { get; set; }
        IDictionary<string, object> Properties { get; }

        FacadeRequestDelegate Build();

        IFacadeApplicationBuilder New();

        IFacadeApplicationBuilder Use(Func<FacadeRequestDelegate, FacadeRequestDelegate> middleware);
    }
}