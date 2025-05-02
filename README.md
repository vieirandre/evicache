## About

[![nuget](https://img.shields.io/nuget/v/evicache.svg)](https://www.nuget.org/packages/EviCache/)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/44d364a2788647de8886f9a99628496e)](https://app.codacy.com/gh/vieirandre/evicache/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)

`EviCache` is a lightweight, thread-safe, in-memory caching library for .NET.

It supports multiple eviction policies and offers extended cache operations. Moreover, it provides metrics and inspection capabilities.

## Table of Contents
- [About](#about)
- [Key Features](#key-features)
- [How&nbsp;to&nbsp;Use](#how-to-use)
- [Feedback & Contributing](#feedback)

## Key Features

- **Thread-safe operations**: All cache operations are synchronized, ensuring thread safety for concurrent access.
- **Multiple eviction policies**:
    - Least Recently Used (LRU): Evicts the item that has not been accessed for the longest period.
    - Least Frequently Used (LFU): Evicts the item with the lowest access frequency.
    - First-In, First-Out (FIFO): Evicts the item that was inserted first.
    - No Eviction: New items are not accepted when the cache is full.
- **Built-in metrics**: Tracks cache count, hits, misses, and evictions.
- **Cache inspection**: Retrieves snapshots and list of keys currently in the cache.

## How to Use

**Interfaces**

* `ICache<TKey, TValue>`: Combines all functionality exposed by the interfaces below.
* `ICacheOperations<TKey, TValue>`: Provides cache operations (`Get`, `Put`, etc.).
* `ICacheOperationsAsync<TKey, TValue>`: Provides asynchronous versions of cache operations (`GetAsync`, `PutAsync`, etc.).
* `ICacheMetrics`: Access performance and usage metrics.
* `ICacheInspection<TKey, TValue>`: Inspect cache contents (keys and snapshots).
* `ICacheItemMetadata<TKey>`: Provides access to cache item metadata.

**Exceptions**

* `KeyNotFoundException`: Thrown when attempting to retrieve a key that does not exist (only via `Get`).
* `CacheFullException`: Thrown when the cache is full and (a) eviction fails, or (b) eviction is disabled (`NoEviction` policy).
  * The exception’s **`Data`** dictionary is populated for easy diagnostics: `Capacity` (int), `AttemptedKey` (string), and `EvictionPolicy` (string).

**Initializing the cache**

```csharp
using EviCache;
using EviCache.Options;
using EviCache.Enums;

// Create cache options with a capacity of 5 and LRU eviction policy
var cacheOptions = new CacheOptions(5, EvictionPolicy.LRU);

// Instantiate the cache
var cache = new Cache<int, string>(cacheOptions);

// Optionally, provide an ILogger instance
var cache = new Cache<int, string>(cacheOptions, logger);
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
string newValue = cache.AddOrUpdate(1, "newOne");
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
Console.WriteLine($"Cache Capacity: {cache.Capacity}");
Console.WriteLine($"Cache Count: {cache.Count}");
Console.WriteLine($"Cache Hits: {cache.Hits}");
Console.WriteLine($"Cache Misses: {cache.Misses}");
Console.WriteLine($"Cache Evictions: {cache.Evictions}");
```

**Accessing cache item metadata**

```csharp
var meta = cache.GetMetadata("key1"); // throws KeyNotFoundException if key does not exist

// A "try" option is available
// bool found = cache.TryGetMetadata("key1", out var meta);

Console.WriteLine($"Created: {meta.CreatedAt}");
Console.WriteLine($"Last accessed: {meta.LastAccessedAt}");
Console.WriteLine($"Last updated: {meta.LastUpdatedAt}");
Console.WriteLine($"Access count: {meta.AccessCount}");
```

<a id="feedback"></a>
## Feedback & Contributing

`EviCache` is released as open source under the [MIT license](https://licenses.nuget.org/MIT). Bug reports and contributions are welcome at [the GitHub repository](https://github.com/vieirandre/evicache).