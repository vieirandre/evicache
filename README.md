## About

`EviCache` is a lightweight, thread-safe, in-memory caching library for .NET.

It supports multiple eviction policies and offers extended cache operations. Moreover, it provides metrics and inspection capabilites.

## Key Features

- **Thread-safe operations**: All cache operations are synchronized, ensuring thread safety for concurrent access.
- **Multiple eviction policies** (work in progress for others):
    - Least Recently Used (LRU): Evicts the item that has not been accessed for the longest period.
    - Least Frequently Used (LFU): Evicts the item with the lowest access frequency.
    - No Eviction: New items are not accepted when the cache is full.
- **Built-in metrics**: Tracks cache count, hits, misses, and evictions.
- **Cache inspection**: Retrieves snapshots and list of keys currently in the cache.

## How to Use

**Initializing the cache**

```csharp
using EviCache;
using EviCache.Options;
using EviCache.Enums;

// Create cache options with a capacity of 5 and LRU eviction policy
var cacheOptions = new CacheOptions(5, EvictionPolicy.LRU);

// Instantiate the cache
// 'int' is the type for keys (TKey) and must be non-nullable
// 'string' is the type for values (TValue) stored in the cache
var cache = new Cache<int, string>(cacheOptions);
```

**Inserting and retrieving values**

```csharp
// Insert a new value into the cache
cache.Put(1, "one");

// Retrieve the value (throws KeyNotFoundException if key does not exist)
string value = cache.Get(1);

// Retrieve the value without throwing an exception if the key is missing
bool retrieved = cache.TryGet(1, out string value);
```

**Conditional retrieval and updates**

```csharp
// Return the existing value for a key or add a new key/value pair if not found
string value = cache.GetOrAdd(2, "two");

// Update the value if the key exists; otherwise, add it
string updatedValue = cache.AddOrUpdate(1, "newOne");
```

**Checking, removing, and clearing entries**

```csharp
// Check if a key exists in the cache
bool exists = cache.ContainsKey(1);

// Remove a key from the cache
bool removed = cache.Remove(1);

// Clear the entire cache
cache.Clear();
```

**Inspecting cache contents**

```csharp
// Retrieve a snapshot of the cache
ImmutableList<KeyValuePair<TKey, TValue>> snapshot = cache.GetSnapshot();

// Retrieve all keys
ImmutableList<TKey> keys = cache.GetKeys();
```

**Accessing cache metrics**

```csharp
// Access cache metrics
Console.WriteLine($"Cache Count: {cache.Count}");
Console.WriteLine($"Cache Hits: {cache.Hits}");
Console.WriteLine($"Cache Misses: {cache.Misses}");
Console.WriteLine($"Cache Evictions: {cache.Evictions}");
```