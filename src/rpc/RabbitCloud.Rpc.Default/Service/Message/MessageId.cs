using System.Linq;
using System.Text;

namespace RabbitCloud.Rpc.Default.Service.Message
{
    public struct Id
    {
        private readonly long _numberValue;
        private readonly byte[] _blobValue;

        public bool IsInteger { get; }

        private Id(byte[] value) : this(0, value)
        {
            IsInteger = false;
        }

        private Id(long numberValue, byte[] blobValue = null)
        {
            IsInteger = true;
            _numberValue = numberValue;
            _blobValue = blobValue;
        }

        public static implicit operator Id(int value)
        {
            return new Id(value);
        }

        public static implicit operator Id(long value)
        {
            return new Id(value);
        }

        public static implicit operator Id(string value)
        {
            return new Id(Encoding.UTF8.GetBytes(value));
        }

        public static implicit operator string(Id value)
        {
            return value.ToString();
        }

        public static implicit operator long(Id value)
        {
            return value._numberValue;
        }

        public static implicit operator int(Id value)
        {
            return (int)value._numberValue;
        }

        #region Overrides of ValueType

        /// <summary>返回该实例的完全限定类型名。</summary>
        /// <returns>包含完全限定类型名的 <see cref="T:System.String" />。</returns>
        public override string ToString()
        {
            if (IsInteger)
                return _numberValue.ToString();
            var blob = _blobValue;
            if (blob == null || !blob.Any())
                return string.Empty;
            return Encoding.UTF8.GetString(blob);
        }

        #endregion Overrides of ValueType

        #region Equality members

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            if (!(obj is Id))
                return false;
            var id = (Id)obj;

            if (id.IsInteger != IsInteger)
                return false;

            if (id.IsInteger)
                return _numberValue == id._numberValue;
            return ToString() == id.ToString();
        }

        /// <summary>Serves as the default hash function. </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public static bool operator ==(Id model1, Id model2)
        {
            return Equals(model1, model2);
        }

        public static bool operator !=(Id model1, Id model2)
        {
            return !Equals(model1, model2);
        }

        #endregion Equality members
    }
}