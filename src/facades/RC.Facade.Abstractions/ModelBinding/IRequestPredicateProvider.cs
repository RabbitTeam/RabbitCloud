using System;

namespace Rabbit.Cloud.Facade.Abstractions.ModelBinding
{
    public interface IRequestPredicateProvider
    {
        Func<ServiceRequestContext, bool> RequestPredicate { get; }
    }
}