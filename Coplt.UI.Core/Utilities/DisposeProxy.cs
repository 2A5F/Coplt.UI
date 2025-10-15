using System.Runtime.CompilerServices;
using InlineIL;
using static InlineIL.IL.Emit;

namespace Coplt.UI.Utilities;

public static class DisposeProxy
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TryDispose<T>(ref T obj)
    {
        if (!DisposeProxy<T>.IsDisposable) return;
        DisposeProxy<T>.Dispose(ref obj);
    }
}

// ReSharper disable once ConvertToStaticClass
// ReSharper disable once ClassNeverInstantiated.Global
public abstract class DisposeProxy<T>
{
    private DisposeProxy() { }

    public static bool IsDisposable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    } = typeof(IDisposable).IsAssignableFrom(typeof(T));

    // ReSharper disable once EntityNameCapturedOnly.Global
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Dispose(ref T disposable)
    {
        Ldarg(nameof(disposable));
        Constrained<T>();
        Callvirt(MethodRef.Method(TypeRef.Type<IDisposable>(), nameof(IDisposable.Dispose)));
        Ret();
    }
}
