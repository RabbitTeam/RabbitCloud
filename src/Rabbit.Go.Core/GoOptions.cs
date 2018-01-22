using Microsoft.Extensions.Primitives;
using Rabbit.Go.Abstractions;
using Rabbit.Go.Core.Codec;
using System;
using System.Collections.Generic;

namespace Rabbit.Go.Core
{
    public class GoOptions
    {
        public GoOptions()
        {
            GlobalInterceptors = new List<IGoInterceptor>();
            FormatterMappings = new FormatterCodecMappings();
            Types = new List<Type>();

            var jsonEncoder = new JsonEncoder();
            var jsonDecoder = new JsonDecoder();
            FormatterMappings
                .Set(new StringValues("application/json"), jsonEncoder, jsonDecoder);

            ForamtterEncoder = new ForamtterEncoder(this);
            ForamtterDecoder = new ForamtterDecoder(this);
        }

        public IList<IGoInterceptor> GlobalInterceptors { get; }
        public IList<Type> Types { get; set; }
        public FormatterCodecMappings FormatterMappings { get; }

        internal ForamtterEncoder ForamtterEncoder { get; }
        internal ForamtterDecoder ForamtterDecoder { get; }
    }
}