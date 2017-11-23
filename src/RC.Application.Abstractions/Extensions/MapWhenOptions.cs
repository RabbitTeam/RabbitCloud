using System;

namespace Rabbit.Cloud.Application.Abstractions.Extensions
{
    public class MapWhenOptions<TContext> where TContext : IRabbitContext
    {
        private Func<TContext, bool> _predicate;

        /// <summary>
        /// The user callback that determines if the branch should be taken.
        /// </summary>
        public Func<TContext, bool> Predicate
        {
            get => _predicate;
            set => _predicate = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// The branch taken for a positive match.
        /// </summary>
        public RabbitRequestDelegate Branch { get; set; }
    }
}