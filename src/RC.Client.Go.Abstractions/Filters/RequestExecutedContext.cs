using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace Rabbit.Cloud.Client.Go.Abstractions.Filters
{
    public class RequestExecutedContext : FilterContext
    {
        private Exception _exception;
        private ExceptionDispatchInfo _exceptionDispatchInfo;

        public RequestExecutedContext(GoRequestContext goRequestContext, IList<IFilterMetadata> filters, IDictionary<string, object> arguments)
            : base(goRequestContext, filters)
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