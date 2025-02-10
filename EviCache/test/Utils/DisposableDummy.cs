namespace EviCache.Tests.Utils;

public class DisposableDummy : IDisposable
{
    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        IsDisposed = true;
    }
}