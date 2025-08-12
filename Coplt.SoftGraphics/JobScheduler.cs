namespace Coplt.SoftGraphics;

public abstract class AJobScheduler
{
    public virtual void Dispatch<T>(uint x, T ctx, Action<T, uint, uint> action) => Dispatch(x, 1, ctx, action);
    public virtual void Dispatch<T, L>(uint x, T ctx, Func<T, L> init, Action<T, L, uint, uint> action)
        => Dispatch(x, 1, ctx, init, action);
    public abstract void Dispatch<T>(uint x, uint y, T ctx, Action<T, uint, uint> action);
    public abstract void Dispatch<T, L>(uint x, uint y, T ctx, Func<T, L> init, Action<T, L, uint, uint> action);
}

public sealed class SyncJobScheduler : AJobScheduler
{
    private SyncJobScheduler() { }

    public static SyncJobScheduler Instance { get; } = new();

    public override void Dispatch<T>(uint x, T ctx, Action<T, uint, uint> action)
    {
        for (var a = 0u; a < x; a++)
        {
            action(ctx, a, 0);
        }
    }

    public override void Dispatch<T>(uint x, uint y, T ctx, Action<T, uint, uint> action)
    {
        for (var b = 0u; b < y; b++)
        {
            for (var a = 0u; a < x; a++)
            {
                action(ctx, a, b);
            }
        }
    }

    public override void Dispatch<T, L>(uint x, T ctx, Func<T, L> init, Action<T, L, uint, uint> action)
    {
        var local = init(ctx);
        for (var a = 0u; a < x; a++)
        {
            action(ctx, local, a, 0);
        }
    }

    public override void Dispatch<T, L>(uint x, uint y, T ctx, Func<T, L> init, Action<T, L, uint, uint> action)
    {
        var local = init(ctx);
        for (var b = 0u; b < y; b++)
        {
            for (var a = 0u; a < x; a++)
            {
                action(ctx, local, a, b);
            }
        }
    }
}

public sealed class ParallelJobScheduler : AJobScheduler
{
    public static int ProcessorCount { get; } = Environment.ProcessorCount;
    public static int MinLoad { get; } = Math.Max(32, Environment.ProcessorCount * 2);

    private ParallelJobScheduler() { }

    public static ParallelJobScheduler Instance { get; } = new();

    public override void Dispatch<T>(uint x, T ctx, Action<T, uint, uint> action)
    {
        if (x < MinLoad)
        {
            SyncJobScheduler.Instance.Dispatch(x, ctx, action);
            return;
        }

        Parallel.For(0, x, i => { action(ctx, (uint)i, 0); });
    }

    public override void Dispatch<T>(uint x, uint y, T ctx, Action<T, uint, uint> action)
    {
        var size = x * y;
        if (size < MinLoad)
        {
            SyncJobScheduler.Instance.Dispatch(x, y, ctx, action);
            return;
        }

        Parallel.For(0, size, i =>
        {
            var (b, a) = Math.DivRem((uint)i, x);
            action(ctx, a, b);
        });
    }
    public override void Dispatch<T, L>(uint x, T ctx, Func<T, L> init, Action<T, L, uint, uint> action)
    {
        if (x < MinLoad)
        {
            SyncJobScheduler.Instance.Dispatch(x, ctx, init, action);
            return;
        }

        Parallel.For(0, x, () => init(ctx), (i, _, l) =>
        {
            action(ctx, l, (uint)i, 0);
            return l;
        }, _ => { });
    }

    public override void Dispatch<T, L>(uint x, uint y, T ctx, Func<T, L> init, Action<T, L, uint, uint> action)
    {
        var size = x * y;
        if (size < MinLoad)
        {
            SyncJobScheduler.Instance.Dispatch(x, y, ctx, init, action);
            return;
        }

        Parallel.For(0, size, () => init(ctx), (i, _, l) =>
        {
            var (b, a) = Math.DivRem((uint)i, x);
            action(ctx, l, a, b);
            return l;
        }, _ => { });
    }
}
