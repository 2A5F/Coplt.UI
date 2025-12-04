using Coplt.Com;
using Coplt.Dropping;
using Coplt.Mathematics;
using Coplt.UI.Core.Geometry.Native;
using Coplt.UI.Native;

namespace Coplt.UI.Core.Geometry;

public enum AtlasAllocatorType : byte
{
    Common,
    Bucketed,
}

[Dropping(Unmanaged = true)]
public sealed unsafe partial class AtlasAllocator
{
    #region Fields

    [Drop]
    internal Rc<IAtlasAllocator> m_inner;
    internal readonly AtlasAllocatorType m_type;
    internal readonly int2 m_size;

    #endregion

    #region Props

    public ref readonly Rc<IAtlasAllocator> Inner => ref m_inner;
    public ref readonly AtlasAllocatorType Type => ref m_type;
    public ref readonly int2 Size => ref m_size;

    #endregion

    #region Ctor

    public AtlasAllocator(int2 Size, AtlasAllocatorType Type = AtlasAllocatorType.Common) : this(Size.x, Size.y, Type) { }

    public AtlasAllocator(int Width, int Height, AtlasAllocatorType Type = AtlasAllocatorType.Common)
    {
        IAtlasAllocator* ptr;
        NativeLib.Instance.m_lib.CreateAtlasAllocator(Type, Width, Height, &ptr).TryThrowWithMsg();
        m_inner = new(ptr);
        m_type = Type;
        m_size = Size;
    }

    #endregion

    #region Id

    public record struct AllocId(uint Id);

    #endregion

    #region Methods

    public void Clear() => m_inner.Clear();
    public bool IsEmpty => m_inner.IsEmpty;

    public bool Allocate(int2 Size, out AllocId Id, out AABB2DI Rect) =>
        Allocate(Size.x, Size.y, out Id, out Rect);
    public bool Allocate(int Width, int Height, out AllocId Id, out AABB2DI Rect)
    {
        uint id = 0;
        fixed (AABB2DI* p_rect = &Rect)
        {
            var r = m_inner.Allocate(Width, Height, &id, p_rect);
            Id = new(id);
            return r;
        }
    }

    public void Deallocate(AllocId id) => m_inner.Deallocate(id.Id);

    #endregion
}
