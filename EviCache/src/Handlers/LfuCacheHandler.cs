using EviCache.Abstractions;
using System.Collections.Immutable;

namespace EviCache.Handlers;

public class LfuCacheHandler<TKey, TValue> : ICacheHandler<TKey, TValue> where TKey : notnull
{
    private readonly Dictionary<TKey, int> _keyFrequencies = new();
    private readonly SortedDictionary<int, LinkedList<TKey>> _frequencyBuckets = new();

    public ImmutableList<TKey> InternalCollection => throw new NotImplementedException();

    public void RegisterAccess(TKey key)
    {
        if (!_keyFrequencies.TryGetValue(key, out int oldFreq))
        {
            RegisterInsertion(key);
            return;
        }

        if (_frequencyBuckets.TryGetValue(oldFreq, out var bucket))
        {
            bucket.Remove(key);

            if (bucket.Count == 0)
                _frequencyBuckets.Remove(oldFreq);
        }

        int newFreq = oldFreq + 1;
        _keyFrequencies[key] = newFreq;

        if (!_frequencyBuckets.TryGetValue(newFreq, out var newBucket))
        {
            newBucket = new LinkedList<TKey>();
            _frequencyBuckets[newFreq] = newBucket;
        }

        newBucket.AddLast(key);
    }

    public void RegisterInsertion(TKey key)
    {
        _keyFrequencies[key] = 1;

        if (!_frequencyBuckets.TryGetValue(1, out var bucket))
        {
            bucket = new LinkedList<TKey>();
            _frequencyBuckets[1] = bucket;
        }

        bucket.AddLast(key);
    }

    public void RegisterUpdate(TKey key)
    {
        throw new NotImplementedException();
    }

    public void RegisterRemoval(TKey key)
    {
        if (!_keyFrequencies.TryGetValue(key, out int freq))
            return;

        _keyFrequencies.Remove(key);

        if (!_frequencyBuckets.TryGetValue(freq, out var bucket))
            return;

        bucket.Remove(key);

        if (bucket.Count == 0)
            _frequencyBuckets.Remove(freq);
    }

    public void Clear()
    {
        _keyFrequencies.Clear();
        _frequencyBuckets.Clear();
    }

    public bool TrySelectEvictionCandidate(out TKey candidate)
    {
        if (_frequencyBuckets.Count == 0)
        {
            candidate = default!;
            return false;
        }

        var firstBucket = _frequencyBuckets.First();
        if (firstBucket.Value.Count == 0)
        {
            candidate = default!;
            return false;
        }

        candidate = firstBucket.Value.First.Value;
        return true;
    }
}
