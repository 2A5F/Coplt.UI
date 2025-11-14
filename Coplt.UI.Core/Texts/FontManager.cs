using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Native;

namespace Coplt.UI.Texts;

[Dropping]
public sealed unsafe partial class FontManager
{
    #region Fields

    [Drop]
    internal Rc<IFontManager> m_inner;

    internal readonly Lock m_lock = new();
    internal readonly Dictionary<Ptr<IFontFace>, FontFace> m_native_to_manager = new();

    #endregion

    #region Properties

    public ref readonly Rc<IFontManager> Inner => ref m_inner;

    #endregion

    #region Ctor

    internal FontManager(Rc<IFontManager> inner)
    {
        m_inner = inner;
        Init();
    }

    public FontManager()
    {
        var lib = NativeLib.Instance;
        IFontManager* p_fm;
        lib.m_lib.CreateFontManager(&p_fm).TryThrowWithMsg();
        m_inner = new(p_fm);
        Init();
    }

    private void Init()
    {
        SetAssocUpdate(new ManagedAssocUpdate(this));
    }

    #endregion

    #region AssocUpdate

    public interface IAssocUpdate
    {
        void OnAdd(IFontFace* face, ulong id);
        void OnExpired(IFontFace* face, ulong id);
    }

    /// <inheritdoc cref="IFontManager.SetAssocUpdate" />
    public ulong SetAssocUpdate(
        void* Data,
        delegate* unmanaged[Cdecl]<void*, void> OnDrop,
        delegate* unmanaged[Cdecl]<void*, IFontFace*, ulong, void> OnAdd,
        delegate* unmanaged[Cdecl]<void*, IFontFace*, ulong, void> OnExpired
    ) => m_inner.SetAssocUpdate(Data, OnDrop, OnAdd, OnExpired);

    public ulong SetAssocUpdate(IAssocUpdate assoc_update)
    {
        var gc_handle = GCHandle.Alloc(assoc_update);
        return SetAssocUpdate(
            (void*)GCHandle.ToIntPtr(gc_handle),
            &OnDrop, &OnAdd, &OnExpired
        );

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void OnDrop(void* data)
        {
            try
            {
                GCHandle.FromIntPtr((nint)data).Free();
            }
            catch (Exception e)
            {
                try
                {
                    var lib = NativeLib.Instance;
                    lib.EmitUnhandledExceptionEvent(e);
                }
                catch
                {
                    // ignored
                }
            }
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void OnAdd(void* data, IFontFace* face, ulong id)
        {
            try
            {
                var gc_handle = GCHandle.FromIntPtr((nint)data);
                Unsafe.As<IAssocUpdate>(gc_handle.Target!).OnAdd(face, id);
            }
            catch (Exception e)
            {
                try
                {
                    var lib = NativeLib.Instance;
                    lib.EmitUnhandledExceptionEvent(e);
                }
                catch
                {
                    // ignored
                }
            }
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void OnExpired(void* data, IFontFace* face, ulong id)
        {
            try
            {
                var gc_handle = GCHandle.FromIntPtr((nint)data);
                Unsafe.As<IAssocUpdate>(gc_handle.Target!).OnExpired(face, id);
            }
            catch (Exception e)
            {
                try
                {
                    var lib = NativeLib.Instance;
                    lib.EmitUnhandledExceptionEvent(e);
                }
                catch
                {
                    // ignored
                }
            }
        }
    }

    private sealed class ManagedAssocUpdate(FontManager manager) : IAssocUpdate
    {
        public void OnAdd(IFontFace* face, ulong id)
        {
            lock (manager.m_lock)
            {
                face->AddRef();
                ref var slot = ref CollectionsMarshal.GetValueRefOrAddDefault(manager.m_native_to_manager, face, out var exists);
                if (!exists) slot = new FontFace(new(face));
            }
        }

        public void OnExpired(IFontFace* face, ulong id)
        {
            lock (manager.m_lock)
            {
                manager.m_native_to_manager.Remove(face);
            }
        }
    }

    #endregion

    #region Members

    /// <inheritdoc cref="IFontManager.RemoveAssocUpdate" />
    public void RemoveAssocUpdate(ulong AssocUpdateId) => m_inner.RemoveAssocUpdate(AssocUpdateId);

    /// <inheritdoc cref="IFontManager.SetExpireFrame" />
    public void SetExpireFrame(ulong FrameCount) => m_inner.SetExpireFrame(FrameCount);

    /// <inheritdoc cref="IFontManager.SetExpireTime" />
    public void SetExpireTime(ulong TimeTicks) => m_inner.SetExpireTime(TimeTicks);

    /// <inheritdoc cref="IFontManager.GetCurrentFrame" />
    public ulong CurrentFrame => m_inner.GetCurrentFrame();

    /// <inheritdoc cref="IFontManager.Update" />
    public void Update(ulong CurrentTime) => m_inner.Update(CurrentTime);

    /// <inheritdoc cref="IFontManager.FontFaceToId" />
    public ulong NativeFontFaceToId(IFontFace* Face) => m_inner.FontFaceToId(Face);

    /// <inheritdoc cref="IFontManager.IdToFontFace" />
    public IFontFace* IdToNativeFontFace(ulong Id) => m_inner.IdToFontFace(Id);

    #endregion

    #region Managed

    public FontFace? IdToFontFace(ulong Id)
    {
        var face = IdToNativeFontFace(Id);
        if (face == null) return null;
        lock (m_lock)
        {
            return m_native_to_manager.GetValueOrDefault(face);
        }
    }

    /// <inheritdoc cref="IFontManager.FontFaceToId" />
    public ulong FontFaceToId(FontFace Face)
    {
        lock (m_lock)
        {
            m_native_to_manager.Add(Face.m_inner.Handle, Face);
        }
        return m_inner.FontFaceToId(Face.m_inner);
    }

    #endregion
}
