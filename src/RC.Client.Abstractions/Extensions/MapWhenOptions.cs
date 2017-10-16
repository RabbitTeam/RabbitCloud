using System;

namespace Rabbit.Cloud.Client.Abstractions.Extensions
{
    public class MapWhenOptions
    {
        private Func<RabbitContext, bool> _predicate;

        /// <summary>
        /// The user callback that determines if the branch should be taken.
        /// </summary>
        public Func<RabbitContext, bool> Predicate
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