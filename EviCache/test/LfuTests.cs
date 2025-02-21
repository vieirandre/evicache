using EviCache.Enums;
using EviCache.Options;

namespace EviCache.Tests;

public class LfuTests
{
    private const EvictionPolicy _evictionPolicy = EvictionPolicy.LFU;

    [Fact]
    public void Should_ReturnValue_WhenKeyIsInsertedAndRetrieved()
    {
        // arrange

        var options = new CacheOptions(3, _evictionPolicy);
        var cache = new Cache<int, string>(options);

        // act

        cache.Put(1, "value");
        bool found = cache.TryGet(1, out var result);

        // assert

        Assert.True(found);
        Assert.Equal("value", result);
    }

    [Fact]
    public void Should_EvictLeastFrequentlyUsed_WhenCapacityIsExceeded()
    {
        // arrange

        var options = new CacheOptions(2, _evictionPolicy);
        var cache = new Cache<int, string>(options);

        cache.Put(1, "value1");
        cache.Put(2, "value2");

        cache.Get(1);
        cache.Get(1);

        // act

        cache.Put(3, "value3");

        // assert

        Assert.False(cache.TryGet(2, out _));
        Assert.True(cache.TryGet(1, out var value1));
        Assert.Equal("value1", value1);
        Assert.True(cache.TryGet(3, out var value3));
        Assert.Equal("value3", value3);
    }

    [Fact]
    public void Should_StoreAndRetrieve_WhenKeyIsStringAndValueIsInt()
    {
        // arrange

        var options = new CacheOptions(2, _evictionPolicy);
        var cache = new Cache<string, int>(options);

        // act

        cache.Put("this", 10);
        cache.Put("that", 20);
        bool found = cache.TryGet("that", out var thatValue);

        // assert

        Assert.True(found);
        Assert.Equal(20, thatValue);
    }

    [Fact]
    public void Should_UpdateExistingKeyAndEvictLeastFrequentlyUsed_WhenReinsertingKeyOverCapacity()
    {
        // arrange

        var options = new CacheOptions(2, _evictionPolicy);
        var cache = new Cache<int, string>(options);

        cache.Put(1, "oldValue");
        cache.Put(2, "value2");

        // act

        cache.Put(1, "newValue");
        cache.Put(3, "value3");

        // assert

        Assert.False(cache.TryGet(2, out _));
        Assert.Equal("newValue", cache.Get(1));
        Assert.Equal("value3", cache.Get(3));
    }
}
