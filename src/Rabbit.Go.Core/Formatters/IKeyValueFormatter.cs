using System.Threading.Tasks;

namespace Rabbit.Go.Core.Formatters
{
    public interface IKeyValueFormatter
    {
        Task FormatAsync(KeyValueFormatterContext context);
    }
}