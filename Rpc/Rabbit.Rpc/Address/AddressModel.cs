using System.Net;

namespace Rabbit.Rpc.Address
{
    /// <summary>
    /// 一个抽象的地址模型。
    /// </summary>
    public abstract class AddressModel
    {
        /// <summary>
        /// 创建终结点。
        /// </summary>
        /// <returns></returns>
        public abstract EndPoint CreateEndPoint();

        /// <summary>
        /// 重写后的标识。
        /// </summary>
        /// <returns>一个字符串。</returns>
        public abstract override string ToString();
    }
}