namespace LruCache.Tests;

public class LruCacheTests
{
    [Fact]
    public void Should_ReturnValue_WhenKeyIsInsertedAndRetrieved()
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
    public void Should_EvictLeastRecentlyUsed_WhenCapacityIsExceeded()
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

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Should_ThrowArgumentOutOfRangeException_WhenCapacityIsZeroOrNegative(int invalidCapacity)
    {
        // act & assert

        Assert.Throws<ArgumentOutOfRangeException>(() => new LruCache<int, string>(invalidCapacity));
    }

    [Fact]
    public void Should_StoreAndRetrieve_WhenKeyIsStringAndValueIsInt()
    {
        // arrange

        var cache = new LruCache<string, int>(2);

        // act

        cache.Put("this", 10);
        cache.Put("that", 20);
        cache.TryGet("that", out var thatValue);

        // assert

        Assert.Equal(20, thatValue);
    }
}