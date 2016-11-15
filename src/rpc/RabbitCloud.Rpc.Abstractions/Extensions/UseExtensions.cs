using System;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Abstractions.Extensions
{
    /// <summary>
    /// Rpc应用程序使用中间件扩展方法。
    /// </summary>
    public static class UseExtensions
    {
        /// <summary>
        /// 使用一个中间件。
        /// </summary>
        /// <param name="app">Rpc应用程序构建器。</param>
        /// <param name="middleware">中间件委托。</param>
        /// <returns>Rpc应用程序构建器。</returns>
        public static IRpcApplicationBuilder Use(this IRpcApplicationBuilder app, Func<RpcContext, Func<Task>, Task> middleware)
        {
            return app.Use(next =>
            {
                return context =>
                {
                    Func<Task> simpleNext = () => next(context);
                    return middleware(context, simpleNext);
                };
            });
        }
    }
}