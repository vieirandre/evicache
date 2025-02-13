using System.Collections.Immutable;

namespace EviCache.Abstractions;

public interface ICacheHandlerInspection<TKey> where TKey : notnull
{
    public ImmutableList<TKey> InternalCollection { get; }
}
