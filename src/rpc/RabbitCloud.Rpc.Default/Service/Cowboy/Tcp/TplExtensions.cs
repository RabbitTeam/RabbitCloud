using System.Threading.Tasks;

namespace Cowboy.Sockets.Tcp
{
    internal static class TplExtensions
    {
        public static void Forget(this Task task) { }
    }
}