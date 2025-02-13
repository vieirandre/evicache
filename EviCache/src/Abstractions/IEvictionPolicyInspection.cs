using System.Collections.Immutable;

namespace EviCache.Abstractions;

public interface IEvictionPolicyInspection<TKey> where TKey : notnull
{
    public ImmutableList<TKey> InternalCollection { get; }
}
