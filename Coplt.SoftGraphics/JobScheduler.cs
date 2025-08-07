namespace Coplt.SoftGraphics;

public abstract class AJobScheduler
{
    public abstract void Dispatch<T>(uint x, uint y, T ctx, Action<T, uint, uint> action);
}

public sealed class SyncJobScheduler : AJobScheduler
{
    private SyncJobScheduler() { }

    public static SyncJobScheduler Instance { get; } = new();

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
}

public sealed class ParallelJobScheduler : AJobScheduler
{
    private ParallelJobScheduler() { }

    public static ParallelJobScheduler Instance { get; } = new();

    public override void Dispatch<T>(uint x, uint y, T ctx, Action<T, uint, uint> action)
    {
        Parallel.For(0, x * y, i =>
        {
            var (b, a) = Math.DivRem((uint)i, x);
            action(ctx, a, b);
        });
    }
}
