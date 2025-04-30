using EviCache.Abstractions;
using System.Collections.Immutable;

namespace EviCache;

public sealed partial class Cache<TKey, TValue> : ICacheInspection<TKey, TValue> where TKey : notnull
{
    public ImmutableList<TKey> GetKeys() =>
        WithLock(_cacheHandler.GetKeys);

    public ImmutableList<KeyValuePair<TKey, TValue>> GetSnapshot()
    {
        return WithLock(() =>
        {
            var builder = ImmutableList.CreateBuilder<KeyValuePair<TKey, TValue>>();

            foreach (var (key, cacheItem) in _cacheMap)
            {
                if (IsExpired(key, cacheItem)) continue;
                builder.Add(new(key, cacheItem.Value));
            }

            return builder.ToImmutable();
        });
    }
}
