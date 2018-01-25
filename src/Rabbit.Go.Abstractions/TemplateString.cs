using System.Linq;

namespace Rabbit.Go
{
    public struct TemplateString
    {
        public TemplateString(string template, string[] variables)
        {
            Template = template;
            Variables = variables;
            NeedParse = variables != null && Variables.Any();
        }

        public string Template { get; }
        public string[] Variables { get; }

        public bool NeedParse { get; }
    }
}