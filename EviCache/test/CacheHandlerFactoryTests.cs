using EviCache.Enums;
using EviCache.Options;
using System.Reflection;

namespace EviCache.Tests;

public class CacheHandlerFactoryTests
{
    [Theory]
    [InlineData(EvictionPolicy.LRU, "LruCacheHandler`1")]
    [InlineData(EvictionPolicy.LFU, "LfuCacheHandler`1")]
    [InlineData(EvictionPolicy.FIFO, "FifoCacheHandler`1")]
    [InlineData(EvictionPolicy.NoEviction, "NoEvictionCacheHandler`1")]
    public void Should_ReturnCorrectHandler_ForEachPolicy(EvictionPolicy policy, string expectedTypeName)
    {
        // act

        object result = InvokeCreateMethod(policy);

        // assert

        Assert.NotNull(result);
        string actualTypeName = result.GetType().Name;
        Assert.Equal(expectedTypeName, actualTypeName);
    }

    [Fact]
    public void Should_ThrowException_ForInvalidPolicy()
    {
        // arrange

        EvictionPolicy invalidPolicy = (EvictionPolicy)999;

        // act & assert

        TargetInvocationException ex = Assert.Throws<TargetInvocationException>(() => InvokeCreateMethod(invalidPolicy));

        Assert.IsType<NotSupportedException>(ex.InnerException);
        Assert.Equal($"'{invalidPolicy}' is not a supported policy", ex.InnerException.Message);
    }

    private static object InvokeCreateMethod(EvictionPolicy policy)
    {
        var factoryType = typeof(CacheOptions).Assembly.GetType("EviCache.Factories.CacheHandlerFactory");
        Assert.NotNull(factoryType);

        MethodInfo createMethod = factoryType!.GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(createMethod);

        MethodInfo genericMethod = createMethod!.MakeGenericMethod(typeof(int));

        return genericMethod.Invoke(null, new object[] { policy })!;
    }
}
