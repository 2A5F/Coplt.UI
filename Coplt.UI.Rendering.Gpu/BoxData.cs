using Coplt.Dropping;
using Coplt.UI.Collections;
using Coplt.UI.Rendering.Gpu.Graphics;

namespace Coplt.UI.Rendering.Gpu;

[Dropping]
internal sealed partial class BoxDataSource(GpuRendererBackend Backend)
{
    internal const uint BoxDataBufferSize = 1024;
    internal EmbedList<GpuUploadList> m_buffers;
    internal uint m_current_buffer_offset = 0;
    internal readonly Queue<int> m_slot_freed_queue = new();
    internal EmbedList<BoxDataHandleSlot> m_slots;

    [Drop]
    private void DropBuffers()
    {
        foreach (ref var item in m_buffers)
        {
            item.Dispose();
        }
    }

    internal unsafe int RentSlot()
    {
        if (m_slot_freed_queue.TryPeek(out var r) && Backend.CurrentFrame - m_slots[r].RemovedFrame >= GpuRendererBackend.FrameCount)
        {
            m_slot_freed_queue.Dequeue();
            return r;
        }
        if (m_buffers.Count == 0 || m_current_buffer_offset >= BoxDataBufferSize)
        {
            m_buffers.Add(Backend.AllocUploadList((uint)sizeof(BoxData), BoxDataBufferSize));
            m_current_buffer_offset = 0;
        }
        var buffer = m_buffers.Count - 1;
        var index = m_current_buffer_offset++;
        var handle = m_slots.Count;
        m_slots.Add(new((uint)buffer, index, 0));
        return handle;
    }

    internal void ReturnSlot(int Index)
    {
        ref var slot = ref m_slots[Index];
        slot.RemovedFrame = Backend.CurrentFrame;
        m_slot_freed_queue.Enqueue(Index);
    }
}

internal record struct BoxDataHandleSlot(uint Buffer, uint Index, ulong RemovedFrame)
{
    public readonly uint Buffer = Buffer;
    public readonly uint Index = Index;
    public ulong RemovedFrame = RemovedFrame;
}

public record struct BoxDataHandleData(uint Buffer, uint Index)
{
    public uint Buffer = Buffer;
    public uint Index = Index;
}

internal unsafe struct BoxDataHandle(BoxDataSource? Source)
{
    public readonly BoxDataSource? Source = Source;
    public int Handle { get; private set; } = -1;

    internal ref readonly BoxDataHandleSlot Slot => ref Source!.m_slots[Handle];

    public BoxDataHandleData Data
    {
        get
        {
            var slot = Slot;
            var buffer = Source!.m_buffers[(int)slot.Buffer];
            return new(buffer.GpuDescId, slot.Index);
        }
    }

    public bool IsNull => Source == null;

    public void Update(BoxData data)
    {
        var old_handle = Handle;
        Handle = Source!.RentSlot();
        if (old_handle >= 0) Source!.ReturnSlot(old_handle);
        var slot = Slot;
        var buffer = Source!.m_buffers[(int)slot.Buffer];
        var ptr = &((BoxData*)buffer.MappedPtr)[slot.Index];
        *ptr = data;
        buffer.MarkItemChanged(slot.Index);
    }

    public void Dispose()
    {
        if (Source == null) return;
        if (Handle >= 0) Source.ReturnSlot(Handle);
        this = new(null);
    }
}
       