using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace Rabbit.Cloud.Client.Go.Abstractions.Filters
{
    public class ExceptionContext : FilterContext
    {
        private Exception _exception;
        private ExceptionDispatchInfo _exceptionDispatchInfo;

        public ExceptionContext(GoRequestContext goRequestContext, IList<IFilterMetadata> filters) : base(goRequestContext, filters)
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

    public interface IExceptionFilter : IFilterMetadata
    {
        void OnException(ExceptionContext context);
    }
}