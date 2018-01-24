using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Rabbit.Go.Utilities
{
    public class TemplateUtilities
    {
        private const string Left = "{";
        private const string Right = "}";

        private static readonly Regex Regex = new Regex(@"[\\]?\" + Left + "([^" + Right + "]+)" + Right, RegexOptions.Compiled, TimeSpan.FromSeconds(1));

        public static string[] GetVariables(string template)
        {
            var empty = Enumerable.Empty<string>().ToArray();

            if (string.IsNullOrEmpty(template))
                return empty;

            if (!template.Contains(Left) || !template.Contains(Right))
                return empty;

            var matches = Regex.Matches(template);

            var list = new List<string>();
            foreach (Match match in matches)
            {
                if (match.Groups[0].Value.StartsWith("\\"))
                    continue;
                list.Add(match.Groups[1].Value);
            }
            return list.Any() ? list.ToArray() : empty;
        }

        public static string Escaped(string template)
        {
            return template.Replace("\\", "");
        }

        public static string GetReplaceText(string key)
        {
            return Left + key + Right;
        }
    }
}