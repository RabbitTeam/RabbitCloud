using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Rabbit.Cloud.Client.Go
{
    public class TemplateRenderContext
    {
        public TemplateRenderContext(string template, IDictionary<string, object> variables)
        {
            Template = template;
            Variables = variables;
        }

        public string Template { get; set; }
        public IDictionary<string, object> Variables { get; set; }
        public IReadOnlyList<string> VariableNames { get; set; }
        public bool IsRender { get; set; }

        public string Result { get; set; }
    }

    public interface ITemplateEngine
    {
        void Render(TemplateRenderContext context);
    }

    public static class TemplateEngineExtensions
    {
        public static TemplateRenderContext Render(this ITemplateEngine templateEngine, string template, IDictionary<string, object> variables)
        {
            if (template == null || variables == null || !variables.Any())
                return null;
            var context = new TemplateRenderContext(template, variables);
            templateEngine.Render(context);
            return context;
        }
    }

    public class TemplateEngine : ITemplateEngine
    {
        #region Implementation of ITemplateEngine

        public void Render(TemplateRenderContext context)
        {
            var template = context.Template;
            var variableNames = GetVariables(template);
            var builder = new StringBuilder(template);

            context.VariableNames = variableNames;

            foreach (var variable in variableNames)
            {
                var variableValue = GetVariableValue(context, variable);
                if (variableValue == null)
                    continue;
                var replaceValue = GetReplaceText(variable);
                if (template.IndexOf(replaceValue, StringComparison.OrdinalIgnoreCase) == -1)
                    continue;

                builder.Replace(replaceValue, variableValue);
                context.IsRender = true;
            }
            context.Result = context.IsRender ? Escaped(builder.ToString()) : context.Template;
        }

        #endregion Implementation of ITemplateEngine

        private static string GetVariableValue(TemplateRenderContext context, string variable)
        {
            if (context.Variables == null || !context.Variables.TryGetValue(variable, out var value) || value == null)
                return string.Empty;
            return value.ToString();
        }

        public static IReadOnlyList<string> GetVariables(string template)
        {
            var matches = Regex.Matches(template, @"[\\]?\{([^}]+)}");
            var list = new List<string>();
            foreach (Match match in matches)
            {
                if (match.Groups[0].Value.StartsWith("\\"))
                    continue;
                list.Add(match.Groups[1].Value);
            }
            return list;
        }

        public static string Escaped(string template)
        {
            return template.Replace("\\", "");
        }

        public static string GetReplaceText(string key)
        {
            return "{" + key + "}";
        }
    }
}