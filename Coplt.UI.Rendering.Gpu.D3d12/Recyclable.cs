using Coplt.Dropping;

namespace Coplt.UI.Rendering.Gpu.D3d12;

public interface ID3d12Recyclable
{
    public int CurrentFrame { get; }
    public void Recycle();
}

[Dropping]
public abstract partial class AD3d12Recyclable<T>(D3d12RecyclablePool<T> Pool) : ID3d12Recyclable
    where T : AD3d12Recyclable<T>
{
    public D3d12RecyclablePool<T> Pool { get; } = Pool;

    public int CurrentFrame { get; internal set; }
    protected abstract T Recycle();

    void ID3d12Recyclable.Recycle() => Pool.Recycle(Recycle());
}

[Dropping(Unmanaged = true)]
public sealed partial class D3d12RecyclablePool<T>(ID3d12RecyclablePoolSource Source) 
    where T : AD3d12Recyclable<T>
{
    public ID3d12RecyclablePoolSource Source { get; } = Source;
    internal readonly Queue<T> m_pool_queue = new();
    internal readonly Lock m_lock = new();

    internal void Recycle(T pack)
    {
        using var _ = m_lock.EnterScope();
        m_pool_queue.Enqueue(pack);
    }

    public void Return(T pack)
    {
        pack.CurrentFrame = Source.CurrentFrame;
        Source.RegRecycle(pack);
    }

    public T? Rent() => m_pool_queue.TryDequeue(out var r) ? r : null;

    [Drop]
    private void Drop()
    {
        foreach (var queue in m_pool_queue)
        {
            queue.Dispose();
        }
        m_pool_queue.Clear();
    }
}

public interface ID3d12RecyclablePoolSource
{
    public int CurrentFrame { get; }
    public void RegRecycle(ID3d12Recyclable item);
}
