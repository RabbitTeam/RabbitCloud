namespace Rabbit.Cloud.Client.Go.ApplicationModels
{
    public interface IApplicationModelConvention
    {
        void Apply(ApplicationModel application);
    }

    public interface IServiceModelConvention
    {
        void Apply(ServiceModel service);
    }

    public interface IRequestModelConvention
    {
        void Apply(RequestModel request);
    }

    public interface IParameterModelConvention
    {
        void Apply(ParameterModel parameter);
    }
}