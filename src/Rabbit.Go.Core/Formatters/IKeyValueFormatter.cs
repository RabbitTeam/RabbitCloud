using System.Threading.Tasks;

namespace Rabbit.Go.Formatters
{
    public interface IKeyValueFormatter
    {
        Task FormatAsync(KeyValueFormatterContext context);
    }
}