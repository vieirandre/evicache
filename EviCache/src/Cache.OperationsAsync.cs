using EviCache.Abstractions;

namespace EviCache;

public partial class Cache<TKey, TValue> : ICacheOperationsAsync<TKey, TValue> where TKey : notnull
{
    public Task<TValue> GetAsync(TKey key)
    {
        return Task.FromResult(Get(key));
    }

    public Task<(bool Found, TValue Value)> TryGetAsync(TKey key)
    {
        bool found = TryGet(key, out TValue value);
        return Task.FromResult((found, value));
    }

    public Task<bool> ContainsKeyAsync(TKey key)
    {
        return Task.FromResult(ContainsKey(key));
    }

    public Task PutAsync(TKey key, TValue value)
    {
        Put(key, value);
        return Task.CompletedTask;
    }

    public Task<TValue> GetOrAddAsync(TKey key, TValue value)
    {
        return Task.FromResult(GetOrAdd(key, value));
    }

    public Task<TValue> AddOrUpdateAsync(TKey key, TValue value)
    {
        return Task.FromResult(AddOrUpdate(key, value));
    }

    public Task<bool> RemoveAsync(TKey key)
    {
        return Task.FromResult(Remove(key));
    }

    public Task ClearAsync()
    {
        Clear();
        return Task.CompletedTask;
    }
}