namespace EviCache.Abstractions;

public interface IEvictionPolicy<TKey, TValue> where TKey : notnull
{
    void RecordAccess(TKey key);
    void RecordInsertion(TKey key);
    void RecordUpdate(TKey key);
    void RecordRemoval(TKey key);
    void Clear();
    TKey? SelectEvictionCandidate();
}