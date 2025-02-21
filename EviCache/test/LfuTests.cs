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
}
