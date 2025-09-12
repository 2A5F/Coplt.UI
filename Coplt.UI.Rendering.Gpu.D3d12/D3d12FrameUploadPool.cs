using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Coplt.Dropping;
using Coplt.Mathematics;
using Coplt.UI.Rendering.Gpu.D3d12.Utilities;
using Silk.NET.Direct3D12;

namespace Coplt.UI.Rendering.Gpu.D3d12;

[Dropping(Unmanaged = true)]
public sealed unsafe partial class D3d12FrameUploadPool(D3d12RendererBackend Backend)
{
    #region Consts

    public const ulong InitialSize = 128 * 1024; // 64k

    #endregion

    #region Static

    private static int m_debug_id_inc;

    #endregion

    #region Types

    [Dropping]
    internal partial struct BufferItem(D3d12GpuUploadBuffer Buffer)
    {
        [Drop]
        public readonly D3d12GpuUploadBuffer Buffer = Buffer;
        public ulong Offset = 0;

        public void Reset()
        {
            Offset = 0;
        }
    }

    #endregion

    #region Fields

    public D3d12RendererBackend Backend { get; } = Backend;

    internal readonly List<BufferItem> m_buffers = new();

    public ulong CurrentUsedMemory { get; private set; }

    #endregion

    #region Clear

    [Drop]
    private void Clear()
    {
        ClearBuffers();
        CurrentUsedMemory = 0;
    }

    private void ClearBuffers()
    {
        foreach (var buffer in m_buffers)
        {
            buffer.Dispose();
        }
        m_buffers.Clear();
    }

    #endregion

    #region Reset

    public void Reset()
    {
        ResetBuffers();
        CurrentUsedMemory = 0;
    }

    private void ResetBuffers()
    {
        // 只保留最后一个缓冲区
        if (m_buffers.Count == 0) return;
        else if (m_buffers.Count == 1)
        {
            ref var last = ref CollectionsMarshal.AsSpan(m_buffers)[0];
            last.Reset();
        }
        else
        {
            var last = m_buffers[^1];
            m_buffers.RemoveAt(m_buffers.Count - 1);
            ClearBuffers();
            last.Reset();
            m_buffers.Add(last);
        }
    }

    #endregion

    #region Alloc

    private BufferItem DoAlloc(ulong Size)
    {
        return new(new(Backend, Size));
    }

    public UploadRange Alloc(ulong Size, ulong Align = 256)
    {
        ref var item = ref Unsafe.NullRef<BufferItem>();
        if (m_buffers.Count == 0)
        {
            var size = Math.Max(Size.up2pow2(), InitialSize);
            m_buffers.Add(DoAlloc(size));
            item = ref CollectionsMarshal.AsSpan(m_buffers)[0];
            item.Offset = Size;
            CurrentUsedMemory += Size;
            if (Backend.DebugEnabled) item.Buffer.Resource.Handle->SetName($"FrameUploadBuffer {m_debug_id_inc++}");
            return UploadRange.Create(in item, 0, Size);
        }
        else
        {
            {
                var buffers = CollectionsMarshal.AsSpan(m_buffers);
                for (var i = 0; i < buffers.Length; i++)
                {
                    item = ref buffers[i];
                    var aligned = item.Offset.AlignUp(Align);
                    var end = aligned + Size;
                    if (end <= item.Buffer.Size)
                    {
                        var used = end - item.Offset;
                        item.Offset = end;
                        CurrentUsedMemory += used;
                        return UploadRange.Create(in item, aligned, Size);
                    }
                }
            }
            {
                var i = m_buffers.Count;
                var next_size = Math.Max(Size.up2pow2(), item.Buffer.Size * 2);
                m_buffers.Add(DoAlloc(next_size));
                item = ref CollectionsMarshal.AsSpan(m_buffers)[0];
                if (Backend.DebugEnabled) item.Buffer.Resource.Handle->SetName($"FrameUploadBuffer {m_debug_id_inc++}");
                item.Offset = Size;
                CurrentUsedMemory += Size;
                return UploadRange.Create(in item, 0, Size);
            }
        }
    }

    public UploadRange<T> Alloc<T>(int Count = 1, ulong Align = 256) where T : unmanaged
    {
        var range = Alloc((ulong)(sizeof(T) * Count), Align);
        return new(Count, range.Pointer, range.GpuVPtr, range.Resource, range.Offset);
    }

    public UploadRange<T> Alloc<T>(uint Count = 1, ulong Align = 256) where T : unmanaged
    {
        var range = Alloc((ulong)(sizeof(T) * Count), Align);
        return new((int)Count, range.Pointer, range.GpuVPtr, range.Resource, range.Offset);
    }

    public UploadRange<T> Alloc<T>(T Value, ulong Align = 256) where T : unmanaged
    {
        var range = Alloc<T>(1, Align);
        range.View() = Value;
        return range;
    }

    public UploadRange<T> Alloc<T>(ReadOnlySpan<T> Values, ulong Align = 256) where T : unmanaged
    {
        var range = Alloc<T>(Values.Length, Align);
        Values.CopyTo(range.GetSpan());
        return range;
    }

    #endregion
}

public readonly unsafe struct UploadRange(ulong Size, void* Pointer, ulong GpuVPtr, ID3D12Resource* Resource, ulong Offset)
{
    #region Fields

    public readonly ulong Size = Size;
    public readonly void* Pointer = Pointer;
    public readonly ulong GpuVPtr = GpuVPtr;
    public readonly ID3D12Resource* Resource = Resource;
    public readonly ulong Offset = Offset;

    #endregion

    #region Create

    internal static UploadRange Create(in D3d12FrameUploadPool.BufferItem buffer, ulong Offset, ulong Size)
    {
        var ptr = (byte*)buffer.Buffer.MappedPtr + Offset;
        var gpu_ptr = buffer.Buffer.GpuVPtr + Offset;
        return new(Size, ptr, gpu_ptr, buffer.Buffer.Resource.Handle, Offset);
    }

    #endregion

    #region View

    public Span<byte> GetSpan() => new(Pointer, (int)Size);
    public Span<T> GetSpan<T>() where T : unmanaged => MemoryMarshal.Cast<byte, T>(GetSpan());
    public ref T View<T>() where T : unmanaged
    {
        if (Size < (ulong)sizeof(T)) throw new InvalidOperationException($"UploadRange size {Size} is smaller than {sizeof(T)}");
        return ref Unsafe.AsRef<T>(Pointer);
    }

    #endregion
}

public readonly unsafe struct UploadRange<T>(int Count, void* Pointer, ulong GpuVPtr, ID3D12Resource* Resource, ulong Offset) where T : unmanaged
{
    #region Fields

    public readonly int Count = Count;
    public readonly void* Pointer = Pointer;
    public readonly ulong GpuVPtr = GpuVPtr;
    public readonly ID3D12Resource* Resource = Resource;
    public readonly ulong Offset = Offset;

    #endregion

    #region Convert

    public static implicit operator UploadRange(UploadRange<T> range) =>
        new((ulong)(sizeof(T) * range.Count), range.Pointer, range.GpuVPtr, range.Resource, range.Offset);

    #endregion

    #region View

    public Span<T> GetSpan() => new(Pointer, Count);
    public ref T View() => ref *(T*)Pointer;

    public ref T this[int index] => ref ((T*)Pointer)[index];
    public ref T this[uint index] => ref ((T*)Pointer)[index];

    #endregion
}
