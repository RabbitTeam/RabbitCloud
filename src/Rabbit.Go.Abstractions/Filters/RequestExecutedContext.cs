using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace Rabbit.Go.Abstractions.Filters
{
    public class RequestExecutedContext : FilterContext
    {
        private Exception _exception;
        private ExceptionDispatchInfo _exceptionDispatchInfo;

        public RequestExecutedContext(RequestContext requestContext, IList<IFilterMetadata> filters, IDictionary<string, object> arguments)
            : base(requestContext, filters)
        {
            Arguments = arguments;
        }

        public virtual bool Canceled { get; set; }

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
        public virtual object Result { get; set; }
    }
}