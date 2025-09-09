using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

namespace Coplt.UI.Rendering.Gpu.D3d12.Utilities;

internal static class Utils
{
    #region HResult

    public static HResult AsHResult(this int result) => result;

    [StackTraceHidden]
    public static void TryThrow(this HResult result)
    {
        if (!result.IsSuccess) result.Throw();
    }

    [StackTraceHidden]
    public static void TryThrowHResult(this int result) => result.AsHResult().TryThrow();

    #endregion

    #region ID3D12Fence Wait

    public static unsafe void Wait(this ComPtr<ID3D12Fence> fence, ulong value, EventWaitHandle handle)
    {
        if (fence.GetCompletedValue() >= value) return;
        fence.SetEventOnCompletion(value, (void*)handle.SafeWaitHandle.DangerousGetHandle()).TryThrowHResult();
        handle.WaitOne();
    }

    public static unsafe ValueTask WaitAsync(this ComPtr<ID3D12Fence> fence, ulong value, EventWaitHandle handle)
    {
        if (fence.GetCompletedValue() >= value) return ValueTask.CompletedTask;
        fence.SetEventOnCompletion(value, (void*)handle.SafeWaitHandle.DangerousGetHandle()).TryThrowHResult();
        var tcs = new TaskCompletionSource();
        ThreadPool.RegisterWaitForSingleObject(handle, static (tcs, _) => { ((TaskCompletionSource)tcs!).SetResult(); }, tcs, TimeSpan.MaxValue, true);
        return new(tcs.Task);
    }

    #endregion

    #region GetManifestResourceSpan

    public static unsafe ReadOnlySpan<byte> GetManifestResourceSpan(this Assembly asm, string name)
    {
        var stream = (UnmanagedMemoryStream)asm.GetManifestResourceStream(name)!;
        return new(stream.PositionPointer, (int)stream.Length);
    }

    #endregion

    #region AlignUp

    public static uint AlignUp(this uint value, uint alignment) => (value + alignment - 1) & ~(alignment - 1);

    public static ulong AlignUp(this ulong value, ulong alignment) => (value + alignment - 1) & ~(alignment - 1);

    #endregion

    #region Format Utils

    extension(Format Format)
    {
        public Format ToSrgb => Format switch
        {
            Format.FormatR8G8B8A8Unorm => Format.FormatR8G8B8A8UnormSrgb,
            Format.FormatBC1Unorm => Format.FormatBC1UnormSrgb,
            Format.FormatBC2Unorm => Format.FormatBC2UnormSrgb,
            Format.FormatBC3Unorm => Format.FormatBC3UnormSrgb,
            Format.FormatB8G8R8A8Unorm => Format.FormatB8G8R8A8UnormSrgb,
            Format.FormatB8G8R8X8Unorm => Format.FormatB8G8R8X8UnormSrgb,
            Format.FormatBC7Unorm => Format.FormatBC7UnormSrgb,
            _ => Format
        };
    }

    #endregion
}
