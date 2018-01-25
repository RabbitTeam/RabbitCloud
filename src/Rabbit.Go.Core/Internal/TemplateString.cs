using Rabbit.Go.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rabbit.Go.Internal
{
    public class ParseContext
    {
        public TemplateString TemplateString { get; }

        public ParseContext(TemplateString templateString, IDictionary<string, string> arguments)
        {
            TemplateString = templateString;
            Arguments = arguments;
        }

        public IDictionary<string, string> Arguments { get; set; }
        public string Result { get; set; }
    }

    public interface ITemplateParser
    {
        void Parse(ParseContext context);
    }

    public static class TemplateParserExtensions
    {
        public static string Parse(this ITemplateParser parser, string contengt, IDictionary<string, string> arguments)
        {
            var context = new ParseContext(new TemplateString(contengt, TemplateUtilities.GetVariables(contengt)), arguments);
            parser.Parse(context);

            return context.Result;
        }
    }

    public class TemplateParser : ITemplateParser
    {
        #region Implementation of ITemplateParser

        public void Parse(ParseContext context)
        {
            var templateString = context.TemplateString;
            var template = templateString.Template;
            var builder = new StringBuilder(template);
            var variables = templateString.Variables;

            var parsed = false;
            foreach (var variable in variables)
            {
                var variableValue = GetVariableValue(context, variable);

                var replaceValue = TemplateUtilities.GetReplaceText(variable);
                if (template.IndexOf(replaceValue, StringComparison.OrdinalIgnoreCase) == -1)
                    continue;

                parsed = true;

                builder.Replace(replaceValue, variableValue);
            }
            context.Result = parsed ? TemplateUtilities.Escaped(builder.ToString()) : template;
        }

        #endregion Implementation of ITemplateParser

        private static string GetVariableValue(ParseContext context, string variable)
        {
            var arguments = context.Arguments;
            if (arguments == null || !arguments.TryGetValue(variable, out var value) || value == null)
                return string.Empty;

            return value;
        }
    }
}