using Rabbit.Cloud.Client.Abstractions;
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace Rabbit.Cloud.Facade.Abstractions.Filters
{
    public class ResultExecutedContext : FilterContext
    {
        private Exception _exception;
        private ExceptionDispatchInfo _exceptionDispatchInfo;

        public ResultExecutedContext(RabbitContext rabbitContext, IList<IFilterMetadata> filters, Type returnType) : base(rabbitContext, filters)
        {
            ReturnType = returnType;
        }

        public Type ReturnType { get; }
        public object Result { get; set; }

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

        public virtual bool Canceled { get; set; }
    }
}