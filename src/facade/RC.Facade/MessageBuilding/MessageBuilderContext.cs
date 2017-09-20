using Rabbit.Cloud.Abstractions;
using Rabbit.Cloud.Facade.Abstractions;
using Rabbit.Cloud.Facade.Abstractions.MessageBuilding;
using System.Net.Http;

namespace Rabbit.Cloud.Facade.MessageBuilding
{
    public abstract class MessageBuilderContext
    {
        public abstract ServiceRequestContext ServiceRequestContext { get; set; }
        public abstract string BinderModelName { get; set; }
        public abstract BuildingTarget BuildingTarget { get; set; }
        public abstract string FieldName { get; set; }
        public virtual RabbitContext RabbitContext => ServiceRequestContext?.RabbitContext;
        public abstract bool IsTopLevelObject { get; set; }
        public abstract HttpRequestMessage RequestMessage { get; set; }
    }
}
