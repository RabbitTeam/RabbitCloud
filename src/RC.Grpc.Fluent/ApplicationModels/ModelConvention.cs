namespace Rabbit.Cloud.Grpc.Fluent.ApplicationModels
{
    public interface IApplicationModelConvention
    {
        void Apply(ApplicationModel application);
    }

    public interface IServiceModelConvention
    {
        void Apply(ServiceModel service);
    }

    public interface IMethodModelConvention
    {
        void Apply(MethodModel method);
    }

    public interface IMarshallerModelConvention
    {
        void Apply(MarshallerModel marshaller);
    }

    public interface IServerServiceModelConvention
    {
        void Apply(ServerServiceModel serverService);
    }

    public interface IServerMethodModelConvention
    {
        void Apply(ServerMethodModel serverMethod);
    }
}