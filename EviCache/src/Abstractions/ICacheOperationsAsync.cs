namespace EviCache.Abstractions;

public interface ICacheOperationsAsync<TKey, TValue> where TKey : notnull
{
    Task<TValue> GetAsync(TKey key);
    Task<(bool Found, TValue Value)> TryGetAsync(TKey key);
    Task<bool> ContainsKeyAsync(TKey key);

    Task PutAsync(TKey key, TValue value);
    Task<TValue> GetOrAddAsync(TKey key, TValue value);
    Task<TValue> AddOrUpdateAsync(TKey key, TValue value);

    Task<bool> RemoveAsync(TKey key);
    Task ClearAsync();
}
