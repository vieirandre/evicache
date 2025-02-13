namespace EviCache.Abstractions;

public interface ICacheHandler<TKey, TValue> : ICacheHandlerInspection<TKey> where TKey : notnull
{
    void RecordAccess(TKey key);
    void RecordInsertion(TKey key);
    void RecordUpdate(TKey key);
    void RecordRemoval(TKey key);
    void Clear();
    bool TrySelectEvictionCandidate(out TKey candidate);
}