namespace LruCache.Tests.Utils;

public class DisposableDummy : IDisposable
{
    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        IsDisposed = true;
    }
}