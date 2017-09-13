using System;

namespace Rabbit.Cloud.Facade.Abstractions.Filters
{
    public interface IFilterFactory : IFilterMetadata
    {
        bool IsReusable { get; }

        IFilterMetadata CreateInstance(IServiceProvider serviceProvider);
    }
}