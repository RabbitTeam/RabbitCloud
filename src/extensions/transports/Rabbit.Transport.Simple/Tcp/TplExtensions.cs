using System.Threading.Tasks;

namespace Rabbit.Transport.Simple.Tcp
{
    internal static class TplExtensions
    {
        public static void Forget(this Task task) { }
    }
}