using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Miscellaneous;
using Coplt.UI.Native;

namespace Coplt.UI.Texts;

[Dropping]
public sealed unsafe partial class FontManager
{
    #region Fields

    [Drop]
    internal Rc<IFontManager> m_inner;
    internal readonly FrameSource m_frame_source;

    private readonly ManagedAssocUpdate m_managed_assoc;

    #endregion

    #region Properties

    public ref readonly Rc<IFontManager> Inner => ref m_inner;
    public FrameSource FrameSource => m_frame_source;

    #endregion

    #region Ctor

    public FontManager(FrameSource FrameSource)
    {
        m_frame_source = FrameSource;
        var lib = NativeLib.Instance;
        IFontManager* p_fm;
        lib.m_lib.CreateFontManager(FrameSource.m_inner.Handle, &p_fm).TryThrowWithMsg();
        m_inner = new(p_fm);
        m_managed_assoc = new();
        SetAssocUpdate(m_managed_assoc);
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

    private sealed class ManagedAssocUpdate : IAssocUpdate
    {
        public readonly ConcurrentDictionary<Ptr<IFontFace>, FontFace> m_native_to_manager = new();

        public void OnAdd(IFontFace* face, ulong id)
        {
            face->AddRef();
            m_native_to_manager.GetOrAdd(face, static face => new(new(face)));
        }

        public void OnExpired(IFontFace* face, ulong id)
        {
            m_native_to_manager.Remove(face, out _);
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

    /// <inheritdoc cref="IFontManager.Collect" />
    public void Collect() => m_inner.Collect();

    /// <inheritdoc cref="IFontManager.IdToFontFace" />
    public IFontFace* IdToNativeFontFace(ulong Id) => m_inner.IdToFontFace(Id);

    #endregion

    #region Managed

    public FontFace? NativeFontFaceToFontFace(IFontFace* Face)
    {
        if (Face == null) return null;
        return m_managed_assoc.m_native_to_manager.GetValueOrDefault(Face);
    }

    public FontFace? IdToFontFace(ulong Id)
    {
        var face = IdToNativeFontFace(Id);
        if (face == null) return null;
        return m_managed_assoc.m_native_to_manager.GetValueOrDefault(face);
    }

    #endregion
}
