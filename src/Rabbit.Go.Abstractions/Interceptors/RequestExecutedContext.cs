using Rabbit.Go.Abstractions;
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace Rabbit.Go.Interceptors
{
    public class RequestExecutedContext : InterceptorContext
    {
        private Exception _exception;
        private ExceptionDispatchInfo _exceptionDispatchInfo;

        public RequestExecutedContext(RequestContext requestContext, IList<IInterceptorMetadata> interceptors)
            : base(requestContext, interceptors)
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