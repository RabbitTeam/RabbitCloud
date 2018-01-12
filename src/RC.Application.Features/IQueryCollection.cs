using Microsoft.Extensions.Primitives;
using System.Collections.Generic;

namespace Rabbit.Cloud.Application.Features
{
    public interface IQueryCollection : IEnumerable<KeyValuePair<string, StringValues>>
    {
        int Count { get; }
        ICollection<string> Keys { get; }

        bool ContainsKey(string key);

        bool TryGetValue(string key, out StringValues value);

        StringValues this[string key] { get; }
    }
}