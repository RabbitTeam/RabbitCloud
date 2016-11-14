/*namespace RabbitCloud.Abstractions
{
    public struct ServiceId
    {
        private string _id;

        public static implicit operator string(ServiceId serviceId)
        {
            return serviceId._id;
        }

        public static implicit operator ServiceId(string id)
        {
            return new ServiceId
            {
                _id = id
            };
        }

        #region Overrides of ValueType

        /// <summary>返回该实例的完全限定类型名。</summary>
        /// <returns>包含完全限定类型名的 <see cref="T:System.String" />。</returns>
        public override string ToString()
        {
            return _id;
        }

        #endregion Overrides of ValueType
    }
}*/