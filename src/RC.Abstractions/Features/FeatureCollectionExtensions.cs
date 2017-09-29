using System;

namespace Rabbit.Cloud.Abstractions.Features
{
    public static class FeatureCollectionExtensions
    {
        public static T GetOrAdd<T>(this IFeatureCollection features, Func<T> factory)
        {
            var feature = features.Get<T>();
            if (feature != null)
                return feature;
            feature = factory();
            features.Set(feature);

            return feature;
        }
    }
}