﻿namespace Rabbit.Go.Abstractions.Filters
{
    public interface IFilterProvider
    {
        int Order { get; }

        void OnProvidersExecuting(FilterProviderContext context);

        void OnProvidersExecuted(FilterProviderContext context);
    }
}