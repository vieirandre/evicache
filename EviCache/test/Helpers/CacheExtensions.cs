using System.Reflection;

namespace EviCache.Tests.Helpers;

public static class CacheExtensions
{
    public static Task OverrideEvictionCandidateCollection<TKey, TValue>(this Cache<TKey, TValue> cache, string evictionCandidateCollectionFieldName, object fakeEvictionCandidateCollection) where TKey : notnull
    {
        var selectorField = typeof(Cache<TKey, TValue>).GetField("_evictionCandidateSelector", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(selectorField);

        var evictionHandler = selectorField.GetValue(cache);
        Assert.NotNull(evictionHandler);

        var handlerCollectionField = evictionHandler.GetType().GetField(evictionCandidateCollectionFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(handlerCollectionField);

        handlerCollectionField.SetValue(evictionHandler, fakeEvictionCandidateCollection);

        return Task.CompletedTask;
    }
}
