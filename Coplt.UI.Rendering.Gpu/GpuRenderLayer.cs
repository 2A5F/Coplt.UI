using System.Diagnostics.CodeAnalysis;
using Coplt.Dropping;
using Coplt.Mathematics;
using Coplt.UI.Collections;
using Coplt.UI.Rendering.Gpu.Graphics;

namespace Coplt.UI.Rendering.Gpu;

[Dropping(Unmanaged = true)]
internal sealed unsafe partial class GpuRenderLayerPool(GpuRendererBackend Backend)
{
    #region Consts

    public const uint BatchMaxSize = 64;

    #endregion

    #region Fields

    public GpuRendererBackend Backend { get; } = Backend;

    private readonly Queue<GpuRenderLayer> m_returned_layer = new();
    private readonly Queue<(GpuUploadList Buffer, ulong Frame)> m_returned_buffer = new();

    #endregion

    #region Layer

    public GpuRenderLayer RentLayer() => m_returned_layer.TryDequeue(out var r) ? r : new(this);

    public void ReturnLayer(ref GpuRenderLayer? layer)
    {
        if (layer == null) return;
        layer.Reset();
        m_returned_layer.Enqueue(layer);
        layer = null;
    }

    #endregion

    #region Buffer

    public GpuUploadList RentBuffer()
    {
        if (m_returned_buffer.TryDequeue(out var r) && Backend.CurrentFrame - r.Frame >= GpuRendererBackend.FrameCount) return r.Buffer;
        return Backend.AllocUploadList((uint)sizeof(BoxDataHandleData), BatchMaxSize);
    }

    public void ReturnBuffer(ref GpuUploadList? buffer)
    {
        if (buffer == null) return;
        m_returned_buffer.Enqueue((buffer, Backend.CurrentFrame));
        buffer = null;
    }

    #endregion
}

internal unsafe class GpuRenderLayer(GpuRenderLayerPool Pool)
{
    public GpuRenderLayerType Type;
    // l t w h; only use when Clip = true
    public uint4 ScissorRect;
    public bool Clip;

    internal struct BatchGroup
    {
        private struct BufferItem
        {
            // todo 从每个单独 GpuUploadList 改层共享大 List，Buffer 移到 Pool 中
            public GpuUploadList Buffer;
            public uint Count;
        }

        private EmbedList<BufferItem> m_buffers;

        public void Reset(GpuRenderLayerPool Pool)
        {
            foreach (ref var buffer in m_buffers)
            {
                buffer.Count = 0;
                Pool.ReturnBuffer(ref buffer.Buffer!);
            }
            m_buffers.UnsafeClear();
        }

        private void AddBuffer(GpuRenderLayerPool Pool)
        {
            m_buffers.Add(new() { Buffer = Pool.RentBuffer(), Count = 0 });
        }

        public void Add(GpuRenderLayerPool Pool, in BoxDataHandle handle)
        {
            if (m_buffers.Count == 0) AddBuffer(Pool);
            ref var last = ref m_buffers[^1];
            if (last.Count > GpuRenderLayerPool.BatchMaxSize)
            {
                AddBuffer(Pool);
                last = ref m_buffers[^1];
            }

            var index = last.Count++;
            var data = handle.Data;
            ((BoxDataHandleData*)last.Buffer.MappedPtr)[index] = data;
            last.Buffer.MarkItemChanged(index);
        }
    }

    internal EmbedMap<uint, BatchGroup> m_batches;

    public void Reset()
    {
        Type = 0;
        ScissorRect = default;
        Clip = false;
        foreach (var batch in m_batches)
        {
            batch.Value.Reset(Pool);
        }
    }

    public void AddItem(in BoxDataHandle handle)
    {
        ref var group = ref m_batches.GetValueRefOrAddDefault(handle.VertexCount, out _);
        group.Add(Pool, in handle);
    }

    public void Record()
    {
        
    }
}

internal enum GpuRenderLayerType : uint
{
    Opaque,
    Alpha,
}
