using Microsoft.Extensions.Configuration;

namespace Rabbit.Extensions.Configuration
{
    /// <summary>
    /// 变量缺失动作。
    /// </summary>
    public enum VariableMissingAction
    {
        /// <summary>
        /// 使用Key当做值
        /// </summary>
        UseKey,

        /// <summary>
        /// 使用空值。
        /// </summary>
        UseEmpty,

        /// <summary>
        /// 抛出异常。
        /// </summary>
        ThrowException
    }

    public class TemplateRenderOptions
    {
        public VariableMissingAction VariableMissingAction { get; set; } = VariableMissingAction.UseKey;

        public IConfiguration Target { get; set; }
        public IConfiguration Source { get; set; }
    }
}