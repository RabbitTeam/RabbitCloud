using System;
using System.Runtime.ExceptionServices;

namespace Rabbit.Go.Interceptors
{
    public class RequestExecutedContext : InterceptorContext
    {
        private Exception _exception;
        private ExceptionDispatchInfo _exceptionDispatchInfo;

        public RequestExecutedContext(RequestMessageBuilder requestBuilder) : base(requestBuilder)
        {
        }

        public virtual Exception Exception
        {
            get
            {
                if (_exception == null && _exceptionDispatchInfo != null)
                {
                    return _exceptionDispatchInfo.SourceException;
                }

                return _exception;
            }

            set
            {
                _exceptionDispatchInfo = null;
                _exception = value;
            }
        }

        public virtual ExceptionDispatchInfo ExceptionDispatchInfo
        {
            get => _exceptionDispatchInfo;

            set
            {
                _exception = null;
                _exceptionDispatchInfo = value;
            }
        }

        public virtual bool ExceptionHandled { get; set; }
        public object Result { get; set; }
    }
}