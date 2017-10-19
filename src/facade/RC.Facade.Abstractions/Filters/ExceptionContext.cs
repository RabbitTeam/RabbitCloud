using Rabbit.Cloud.Client.Abstractions;
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace Rabbit.Cloud.Facade.Abstractions.Filters
{
    public class ExceptionContext : FilterContext
    {
        private Exception _exception;
        private ExceptionDispatchInfo _exceptionDispatchInfo;

        public ExceptionContext(IRabbitContext rabbitContext, IList<IFilterMetadata> filters) : base(rabbitContext, filters)
        {
        }

        public virtual Exception Exception
        {
            get => _exception == null && _exceptionDispatchInfo != null
                ? _exceptionDispatchInfo.SourceException
                : _exception;

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

        public virtual bool ExceptionHandled { get; set; } = false;
    }
}