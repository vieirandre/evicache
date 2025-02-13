namespace EviCache.Abstractions;

public interface IEvictionPolicy<TKey, TValue> : IEvictionPolicyInspection<TKey> where TKey : notnull
{
    void RecordAccess(TKey key);
    void RecordInsertion(TKey key);
    void RecordUpdate(TKey key);
    void RecordRemoval(TKey key);
    void Clear();
    bool TrySelectEvictionCandidate(out TKey candidate);
}