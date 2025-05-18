using EviCache.Abstractions;
using EviCache.Extensions;
using System.Collections.Immutable;

namespace EviCache;

public sealed partial class Cache<TKey, TValue> : ICacheInspection<TKey, TValue> where TKey : notnull
{
    public ImmutableList<TKey> GetKeys() =>
        _gate.Execute(_cacheHandler.GetKeys);

    public ImmutableList<KeyValuePair<TKey, TValue>> GetSnapshot()
    {
        return _gate.Execute(() =>
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
