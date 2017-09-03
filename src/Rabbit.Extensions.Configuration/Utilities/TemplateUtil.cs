using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Rabbit.Extensions.Configuration.Utilities
{
    public static class TemplateUtil
    {
        public static bool NeedRender(string template)
        {
            return !string.IsNullOrEmpty(template) && template.Contains("${");
        }

        public static string[] GetVariables(string template)
        {
            var matches = Regex.Matches(template, @"[\\]?\${([^}]+)}");
            var list = new List<string>();
            foreach (Match match in matches)
            {
                if (match.Groups[0].Value.StartsWith("\\"))
                    continue;
                list.Add(match.Groups[1].Value);
            }
            return list.ToArray();
        }

        public static string Escaped(string template)
        {
            return template.Replace("\\", "");
        }
    }
}