namespace EviCache;

public partial class Cache<TKey, TValue> : IDisposable where TKey : notnull
{
    public void Dispose() => Clear();

    private static void DisposeItem(TValue? value)
    {
        if (value is IDisposable disposable)
            disposable.Dispose();
    }
}
