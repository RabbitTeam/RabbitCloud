using System;

namespace Rabbit.Cloud.Client.Abstractions.Extensions
{
    public class MapWhenOptions<TContext>
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
        public RabbitRequestDelegate<TContext> Branch { get; set; }
    }
}