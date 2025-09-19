## About

[![nuget](https://img.shields.io/nuget/v/evicache.svg)](https://www.nuget.org/packages/EviCache/)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/44d364a2788647de8886f9a99628496e)](https://app.codacy.com/gh/vieirandre/evicache/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)
[![Codacy Badge](https://app.codacy.com/project/badge/Coverage/44d364a2788647de8886f9a99628496e)](https://app.codacy.com/gh/vieirandre/evicache/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_coverage)

`EviCache` is a lightweight, thread-safe, in-memory caching library for .NET.

It supports multiple eviction policies and offers extended cache operations. Moreover, it provides metrics and inspection capabilities.

## Table of Contents
- [About](#about)
- [Overview](#usage)
- [Feedback & Contributing](#feedback)

## Overview

### Quick start

```csharp
using EviCache;
using EviCache.Enums;
using EviCache.Options;

// Create a cache with LRU and capacity 100
var cache = new Cache<string, string>(new CacheOptions(
    capacity: 100,
    evictionPolicy: EvictionPolicy.LRU));

// Put & Get
cache.Put("user:1", "André");
var name = cache.Get("user:1"); // "André"

// GetOrAdd (adds on miss, returns existing on hit)
var color = cache.GetOrAdd("color", "blue");

// Per-item absolute expiration (e.g., 1 minute)
cache.Put("otp", "123456",
    new CacheItemOptions {
        Expiration = new ExpirationOptions.Absolute(TimeSpan.FromMinutes(1))
    });
```

#### Async

```csharp
// Same semantics, but async + cancellation support
await cache.PutAsync("k", "v", ct);
var value = await cache.GetAsync("k", ct);
var (found, v) = await cache.TryGetAsync("missing", ct);
```

### Eviction policies

Choose via `CacheOptions.EvictionPolicy`:

* LRU: evicts least-recently-used.
* LFU: evicts least-frequently-used.
* FIFO: evicts oldest inserted.
* NoEviction: refuse to add when full; throws `CacheFullException`.

When capacity is full, the cache evicts one candidate (if the policy allows). If no candidate can be evicted or policy is `NoEviction`, a `CacheFullException` is thrown with diagnostic data (capacity, attempted key, policy).

### Expiration (TTL)

Set expiration globally (default for all items) or per item:

* `ExpirationOptions.Absolute(TimeSpan ttl)`: expires at `now + ttl`.
* `ExpirationOptions.Sliding(TimeSpan ttl)`: expires if not accessed within `ttl`.
* `ExpirationOptions.None`: no expiration.

#### Examples:

```csharp
// Global default expiration: absolute 10 minutes
var cache = new Cache<string, byte[]>(new CacheOptions(
    100, EvictionPolicy.LFU,
    new ExpirationOptions.Absolute(TimeSpan.FromMinutes(10))));

// Per-item sliding expiration: 5 minutes
cache.Put("session:12", session,
    new CacheItemOptions {
        Expiration = new ExpirationOptions.Sliding(TimeSpan.FromMinutes(5))
    });
```

> Expired items are purged lazily on access and during capacity checks; they won’t appear in `GetKeys()`/`GetSnapshot()`.

### API overview

* Retrieval
  * `Get(key)` → value (throws if missing).
  * `TryGet(key, out value)` / `TryGetAsync` → no throw.
  * `ContainsKey(key)` / `ContainsKeyAsync` → does not affect hit/miss counters.
* Mutation
  * `Put(key, value)` / `Put(key, value, options)` → insert or update.
  * `AddOrUpdate(key, value)` → inserts on miss; returns the provided value.
  * `GetOrAdd(key, value)` → returns existing value on hit; otherwise, inserts and returns the provided value.
  * `Remove(key)` → bool; `Clear()` → clears all.
* Inspection
  * `GetKeys()` → `ImmutableList<TKey>` of non-expired keys (order depends on policy).
  * `GetSnapshot()` → `ImmutableList<KeyValuePair<TKey,TValue>>` of non-expired entries.
* Metadata
  * `GetMetadata(key)` / `TryGetMetadata(key, out meta)` → last access/update, access count, expiration, etc.
* Metrics
  * `Capacity`, `Count` (purges expired first), `Hits`, `Misses`, `Evictions`.

### Hits & Misses

* Successful `Get`/`TryGet`/`GetOrAdd` (hit path) increments **Hits** and updates access metadata + policy structures.
* Miss paths increment **Misses**.
* `ContainsKey` is intentionally “cold”: it checks existence without touching metrics or access ordering.

### Thread-safety

All public operations are protected by an internal `SemaphoreSlim`, ensuring a single writer/reader critical section. Both sync and async APIs are safe to call concurrently.

### Disposal semantics

* If a cached value implements `IDisposable`/`IAsyncDisposable`, it is disposed when the entry is removed, evicted, updated (when replacing a different instance), or during `Clear()`.
* `Clear()` gathers disposables and disposes them in the background (respecting the provided cancellation token in `ClearAsync`). Errors during disposal are logged.

### Logging

Pass an `ILogger` to the constructor or rely on the default `NullLogger`. Notable events:

* Initialization (capacity, policy) at `Information`.
* Evictions at `Debug`.
* Errors during eviction selection or background disposal at `Error`.

### Exceptions

* `KeyNotFoundException`: Get on a non-existing/expired key.
* `CacheFullException`: when full and:
  * policy is `NoEviction`, or
  * an eviction candidate couldn’t be selected/removed.

`CacheFullException` includes `Capacity`, the attempted key (if available), and `EvictionPolicy` for diagnostics.

### Performance notes

* Keys are tracked per policy for fast candidate selection; `GetKeys()` returns a filtered, immutable snapshot without expired entries.
* `Count` triggers a purge of expired items before returning the size.
* Avoid calling `ContainsKey` as a pre-check before `Get` — do a single `TryGet` to minimize lock acquisitions.

<a id="feedback"></a>
## Feedback & Contributing

`EviCache` is released as open source under the [MIT license](https://licenses.nuget.org/MIT). Bug reports and contributions are welcome at [the GitHub repository](https://github.com/vieirandre/evicache).