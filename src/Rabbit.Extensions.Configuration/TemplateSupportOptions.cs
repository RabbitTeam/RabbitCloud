namespace Rabbit.Extensions.Configuration
{
    public class TemplateSupportOptions : TemplateRenderOptions
    {
        public bool EnableChildren { get; set; } = true;
        public bool ReloadOnChange { get; set; } = true;
    }
}