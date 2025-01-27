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

    [Fact]
    public void Test2()
    {
        // arrange

        var cache = new LruCache<int, string>(2);

        cache.Put(1, "valueOne");
        cache.Put(2, "valueTwo");

        // act

        cache.TryGet(1, out var valueOne);
        cache.Put(3, "valueThree");
        cache.TryGet(2, out var valueTwo);
        cache.TryGet(3, out var valueThree);

        // assert

        Assert.Null(valueTwo);
        Assert.Equal("valueOne", valueOne);
        Assert.Equal("valueThree", valueThree);
    }
}