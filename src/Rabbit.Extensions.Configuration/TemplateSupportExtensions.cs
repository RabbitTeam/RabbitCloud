using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Rabbit.Extensions.Configuration
{
    public static class TemplateSupportExtensions
    {
        /// <summary>
        /// enable configuration template support.
        /// </summary>
        /// <param name="targetConfiguration">need enable configuration.</param>
        /// <param name="sourceConfiguration">template value source configuration.</param>
        /// <param name="enableChildrens">is enable childrens template support.</param>
        /// <param name="reloadOnChange">configuration change is re enable template support.</param>
        /// <param name="forcedReplace">value is null replace empty, else cancel.</param>
        /// <returns></returns>
        public static T EnableTemplateSupport<T>(this T targetConfiguration, IConfiguration sourceConfiguration = null, bool enableChildrens = true, bool reloadOnChange = true, bool forcedReplace = false) where T : IConfiguration
        {
            if (sourceConfiguration == null)
                sourceConfiguration = targetConfiguration;

            //reload
            if (reloadOnChange)
                targetConfiguration.GetReloadToken().RegisterChangeCallback(s =>
                {
                    targetConfiguration.EnableTemplateSupport(sourceConfiguration, enableChildrens);
                }, null);

            void TemplateRender(IEnumerable<IConfigurationSection> sections)
            {
                while (true)
                {
                    var list = new List<IConfigurationSection>();
                    foreach (var section in sections)
                    {
                        var key = section.Path;
                        var value = section.Value;

                        var isReplace = !string.IsNullOrEmpty(value) && value.Contains("${");
                        var isChildrens = value == null;

                        if (isReplace)
                        {
                            value = Regex.Replace(value, @"\\?\${([^}]+)}", match =>
                            {
                                var raw = match.Groups[0].Value;
                                var configKey = match.Groups[1].Value;

                                //disabled
                                if (raw.StartsWith("\\"))
                                    return raw.Substring(1);

                                var result = sourceConfiguration[configKey];

                                //value
                                return result ?? (forcedReplace ? null : raw);
                            });
                            targetConfiguration[key] = value;
                        }
                        else if (isChildrens)
                        {
                            list.AddRange(section.GetChildren());
                        }
                    }
                    if (list.Any())
                    {
                        sections = list;
                        continue;
                    }
                    break;
                }
            }

            TemplateRender(targetConfiguration.GetChildren());

            return targetConfiguration;
        }
    }
}