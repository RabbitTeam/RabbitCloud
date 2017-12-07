namespace Rabbit.Cloud.ApplicationModels
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

    public interface ICodecModelConvention
    {
        void Apply(CodecModel codec);
    }
}