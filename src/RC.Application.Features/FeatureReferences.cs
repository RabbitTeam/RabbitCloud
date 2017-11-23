using System;
using System.Runtime.CompilerServices;

namespace Rabbit.Cloud.Application.Features
{
    public struct FeatureReferences<TCache>
    {
        public FeatureReferences(IFeatureCollection collection)
        {
            Collection = collection;
            Cache = default(TCache);
            Revision = collection.Revision;
        }

        public IFeatureCollection Collection { get; }
        public int Revision { get; private set; }
        public TCache Cache;

        public TFeature Fetch<TFeature>(ref TFeature cached, Func<IFeatureCollection, TFeature> factory)
            where TFeature : class => Fetch(ref cached, Collection, factory);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TFeature Fetch<TFeature, TState>(
            ref TFeature cached,
            TState state,
            Func<TState, TFeature> factory) where TFeature : class
        {
            var flush = false;
            var revision = Collection.Revision;
            if (Revision != revision)
            {
                // Clear cached value to force call to UpdateCached
                cached = null;
                // Collection changed, clear whole feature cache
                flush = true;
            }

            return cached ?? UpdateCached(ref cached, state, factory, revision, flush);
        }

        private TFeature UpdateCached<TFeature, TState>(ref TFeature cached, TState state, Func<TState, TFeature> factory, int revision, bool flush) where TFeature : class
        {
            if (flush)
            {
                // Collection detected as changed, clear cache
                Cache = default(TCache);
            }

            cached = Collection.Get<TFeature>();
            if (cached == null)
            {
                // Item not in collection, create it with factory
                cached = factory(state);
                // Add item to IFeatureCollection
                Collection.Set(cached);
                // Revision changed by .Set, update revision to new value
                Revision = Collection.Revision;
            }
            else if (flush)
            {
                // Cache was cleared, but item retrived from current Collection for version
                // so use passed in revision rather than making another virtual call
                Revision = revision;
            }

            return cached;
        }
    }
}