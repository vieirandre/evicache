namespace EviCache.Options;

public record CacheItemOptions
{
    public ExpirationOptions? Expiration { get; init; }
}
