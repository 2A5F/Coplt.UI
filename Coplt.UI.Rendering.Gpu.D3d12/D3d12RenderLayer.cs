using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Coplt.Dropping;
using Coplt.UI.Collections;
using Coplt.UI.Rendering.Gpu.D3d12.Utilities;
using Coplt.UI.Rendering.Gpu.Graphics;

namespace Coplt.UI.Rendering.Gpu.D3d12;

[Dropping(Unmanaged = true)]
public sealed unsafe partial class D3d12RenderLayerManager : GpuRenderLayerManager
{
    #region Consts

    public const uint BatchMaxSize = 64;

    #endregion

    #region Fields

    private readonly Queue<D3d12RenderLayer> m_returned_layer = new();
    private readonly Queue<(D3d12FrameUploadPool Pool, ulong Frame)> m_returned_pool = new();
    private D3d12FrameUploadPool? m_current_upload_pool;

    internal UploadRange m_cmd_buf;
    internal uint m_sum_cmd_count;
    internal uint m_cmd_offset;

    #endregion

    #region Props

    public D3d12FrameUploadPool UploadPool => m_current_upload_pool!;

    #endregion

    #region Ctor

    public D3d12RenderLayerManager(D3d12RendererBackend Backend) : base(Backend)
    {
        m_current_upload_pool = new(Backend);
    }

    #endregion

    #region Reset

    public override void Reset(uint width, uint height, float MaxZ)
    {
        D3d12FrameUploadPool new_pool;
        if (m_returned_pool.TryPeek(out var r) && Backend.CurrentFrame - r.Frame >= GpuRendererBackend.FrameCount)
            new_pool = r.Pool;
        else new_pool = new D3d12FrameUploadPool((D3d12RendererBackend)Backend);
        if (m_current_upload_pool != null) m_returned_pool.Enqueue((m_current_upload_pool, Backend.CurrentFrame));
        m_current_upload_pool = new_pool;
        m_cmd_buf = default;
        m_sum_cmd_count = 0;
        m_cmd_offset = 0;
    }

    #endregion

    #region Layer

    public override GpuRenderLayer RentLayer() => m_returned_layer.TryDequeue(out var r) ? r : new D3d12RenderLayer(this);

    public override void ReturnLayer(ref GpuRenderLayer? layer)
    {
        if (layer == null) return;
        var l = (D3d12RenderLayer)layer;
        l.Reset();
        m_returned_layer.Enqueue(l);
        layer = null;
    }

    #endregion

    #region Record

    public override void Record(ReadOnlySpan<GpuRenderLayer> Layers)
    {
        var layers = Unsafe.BitCast<ReadOnlySpan<GpuRenderLayer>, ReadOnlySpan<D3d12RenderLayer>>(Layers);
        foreach (var layer in layers)
        {
            m_sum_cmd_count += layer.SumCmdCount();
        }
        if (m_sum_cmd_count == 0) return;
        m_cmd_buf = m_current_upload_pool!.Alloc(((ulong)sizeof(D3d12DrawCommand_Box) + 4) * m_sum_cmd_count, (uint)sizeof(D3d12DrawCommand_Box));
        foreach (var layer in layers)
        {
            layer.Record();
        }
    }
    
    public override void Render(ReadOnlySpan<GpuRenderLayer> Layers)
    {
        var layers = Unsafe.BitCast<ReadOnlySpan<GpuRenderLayer>, ReadOnlySpan<D3d12RenderLayer>>(Layers);
        foreach (var layer in layers)
        {
            layer.Render(Backend);
        }
    }

    #endregion
}

public sealed unsafe partial class D3d12RenderLayer(D3d12RenderLayerManager Manager) : GpuRenderLayer
{
    internal struct BatchGroup
    {
        private struct BufferRange<T> where T : unmanaged
        {
            public UploadRange<T> Buffer;
            public uint Count;
            public uint Capacity;
        }

        private EmbedList<BufferRange<BoxDataHandleData>> m_batches;

        public uint SumCmdCount() => (uint)m_batches.Count;

        public void Reset(D3d12RenderLayerManager Manager)
        {
            m_batches.Clear();
        }

        private void AddBatchBuffer(D3d12RenderLayerManager Manager)
        {
            var size = m_batches.Count == 0 ? 4 : m_batches[^1].Capacity * 2;
            m_batches.Add(new()
            {
                Buffer = Manager.UploadPool.Alloc<BoxDataHandleData>((int)size, (uint)sizeof(BoxDataHandleData)),
                Count = 0,
                Capacity = size,
            });
        }

        public void Add(D3d12RenderLayerManager Manager, in BoxDataHandle handle)
        {
            if (m_batches.Count == 0) AddBatchBuffer(Manager);
            ref var last = ref m_batches[^1];
            if (last.Count > D3d12RenderLayerManager.BatchMaxSize)
            {
                AddBatchBuffer(Manager);
                last = ref m_batches[^1];
            }

            var index = last.Count++;
            var data = handle.Data;
            last.Buffer[index] = data;
        }

        public void Record(D3d12RenderLayerManager Manager, uint VertexCount)
        {
            var bytes_span = Manager.m_cmd_buf.GetSpan().Slice((int)Manager.m_cmd_offset, m_batches.Count * sizeof(D3d12DrawCommand_Box));
            Manager.m_cmd_offset += (uint)(m_batches.Count * sizeof(D3d12DrawCommand_Box));
            var cmd_buf = MemoryMarshal.Cast<byte, D3d12DrawCommand_Box>(bytes_span);
            var i = 0;
            foreach (var buffer in m_batches)
            {
                cmd_buf[i++] = new()
                {
                    // ViewData = new()
                    // {
                    //     BufferLocation = Manager.m_cur_view_data.GpuVPtr,
                    //     SizeInBytes = (uint)sizeof(ViewData),
                    // },
                    Batches = buffer.Buffer.GpuVPtr,
                    Draw = new()
                    {
                        VertexCountPerInstance = VertexCount,
                        InstanceCount = buffer.Count,
                        StartVertexLocation = 0,
                        StartInstanceLocation = 0
                    }
                };
            }
        }
    }

    internal EmbedMap<uint, BatchGroup> m_batches;
    internal UploadRange<D3d12DrawCommand_Box> m_draw_buffer;
    internal uint m_cmd_offset;
    internal uint m_cmd_count;

    public uint SumCmdCount()
    {
        var sum = 0u;
        foreach (var batch in m_batches)
        {
            sum += batch.Value.SumCmdCount();
        }
        m_cmd_count = sum;
        return sum;
    }

    public void Reset()
    {
        Data = default;
        foreach (var batch in m_batches)
        {
            batch.Value.Reset(Manager);
        }
        m_cmd_offset = uint.MaxValue;
        m_cmd_count = 0;
    }

    public override void AddItem(in BoxDataHandle handle)
    {
        ref var group = ref m_batches.GetValueRefOrAddDefault(handle.VertexCount, out _);
        group.Add(Manager, in handle);
    }

    public void Record()
    {
        if (m_cmd_count == 0) return;
        m_cmd_offset = Manager.m_cmd_offset = Manager.m_cmd_offset.AlignUp(4);
        foreach (var batch in m_batches)
        {
            batch.Value.Record(Manager, batch.Key);
        }
    }

    public void Render(GpuRendererBackend Backend)
    {
        var backed = (D3d12RendererBackend)Backend;
        backed.m_command_list.Handle->SetPipelineState(backed.Pipeline_Box_NoDepth.m_pipeline);
        backed.m_command_list.Handle->ExecuteIndirect(
            backed.CommandSignature_Box.m_command_signature,
            m_cmd_count,
            Manager.m_cmd_buf.Resource,
            Manager.m_cmd_buf.Offset + m_cmd_offset,
            null,
            0
        );
    }
}
