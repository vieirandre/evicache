namespace LruCache.Tests;

public class LruCacheTests
{
    [Fact]
    public void Test1()
    {
        // arrange

        var cache = new LruCache<int, string>(3);

        // act

        cache.Put(1, "value");
        cache.TryGet(1, out var result);

        // assert

        Assert.Equal("value", result);
    }
}