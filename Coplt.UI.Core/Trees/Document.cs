using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Coplt.Dropping;
using Coplt.UI.Collections;
using Coplt.UI.Layouts;
using Coplt.UI.Miscellaneous;
using Coplt.UI.Native;
using Coplt.UI.Styles;
using Coplt.UI.Texts;
using Coplt.UI.Trees.Datas;
using Coplt.UI.Trees.Modules;
using Coplt.UI.Utilities;
using Coplt.UI.Utilities.TypeChains;

namespace Coplt.UI.Trees;

[Dropping(Unmanaged = true)]
public sealed partial class Document
{
    #region Fields

    internal readonly Template m_template;
    [Drop(Order = 10)]
    internal readonly Arche m_view_arche;
    [Drop(Order = 10)]
    internal readonly Arche m_text_paragraph_arche;
    [Drop(Order = 10)]
    internal readonly Arche m_text_span_arche;
    [Drop]
    internal NativeMap<NodeId, RootData> m_roots;
    // ReSharper disable once CollectionNeverQueried.Global
    internal readonly IModule[] m_modules;
    internal readonly Action<Document>[] m_modules_update;
    internal readonly FrameSource m_frame_source;
    internal readonly FontManager m_font_manager;
    internal uint m_node_id_inc;

    internal LocaleId DefaultLocale = Utils.GetUserUiDefaultLocale();

    internal bool m_extern_frame_source;

    #endregion

    #region Ctor

    internal Document(Template template, FrameSource? frame_source, FontManager? font_manager)
    {
        m_template = template;
        m_frame_source = frame_source ?? new();
        m_font_manager = font_manager ?? new(m_frame_source);
        m_extern_frame_source = font_manager is not null;
        m_view_arche = m_template.m_view_arche.Create();
        m_text_paragraph_arche = m_template.m_text_paragraph_arche.Create();
        m_text_span_arche = m_template.m_text_span_arche.Create();
        m_modules = new IModule[m_template.m_modules.Length];
        m_modules_update = new Action<Document>[m_template.m_modules.Length];
        for (var i = 0; i < m_template.m_modules.Length; i++)
        {
            var module_template = m_template.m_modules[i];
            var module = m_modules[i] = module_template.Create(this);
            m_modules_update[i] = module.Update;
        }
    }

    #endregion

    #region Builder

    public sealed class Builder
    {
        private readonly Dictionary<Type, AModuleTemplate> m_modules = new();
        private readonly Dictionary<ArcheTarget, Dictionary<Type, AStorageTemplate>> m_types = new();
        private FrameSource? m_frame_source;
        private FontManager? m_font_manager;

        public Builder()
        {
            Attach<ManagedData>(ArcheTarget.All);
            Attach<CommonData>(ArcheTarget.All, storage: StorageType.Pinned);
            Attach<LayoutData>(ArcheTarget.View, storage: StorageType.Pinned);
            Attach<ChildsData>(ArcheTarget.View | ArcheTarget.TextParagraph, storage: StorageType.Pinned);
            Attach<StyleData>(ArcheTarget.View, storage: StorageType.Pinned);
            Attach<TextStyleData>(ArcheTarget.TextParagraph | ArcheTarget.TextSpan, storage: StorageType.Pinned);
            Attach<TextParagraphData>(ArcheTarget.TextParagraph, storage: StorageType.Pinned);
            Attach<TextSpanData>(ArcheTarget.TextSpan, storage: StorageType.Pinned);
            With<LayoutModule>();
        }

        public Builder WithFrameSource(FrameSource frame_source)
        {
            m_frame_source = frame_source;
            return this;
        }

        public Builder WithFontManager(FontManager font_manager)
        {
            m_frame_source = font_manager.m_frame_source;
            m_font_manager = font_manager;
            return this;
        }

        public Builder Attach<T>(StorageType storage = StorageType.Default) where T : new()
            => Attach<T>(ArcheTarget.All, storage: storage);
        public Builder Attach<T>(ArcheTarget targets, StorageType storage = StorageType.Default)
            where T : new()
        {
            foreach (var target in targets)
            {
                ref var types = ref CollectionsMarshal.GetValueRefOrAddDefault(m_types, target, out var exists);
                if (!exists) types = new();
                ref var t = ref CollectionsMarshal.GetValueRefOrAddDefault(types!, typeof(T), out exists);
                if (exists) throw new ArgumentException($"Type {typeof(T)} has already been attached.");
                t = new StorageTemplate<T>(storage);
            }
            return this;
        }

        public Builder With<T>() where T : IModule
        {
            m_modules.TryAdd(typeof(T), new ModuleTemplate<T>());
            return this;
        }

        public Template Build() => new(
            new ArcheTemplate(m_types[ArcheTarget.View]),
            new ArcheTemplate(m_types[ArcheTarget.TextParagraph]),
            new ArcheTemplate(m_types[ArcheTarget.TextSpan]),
            m_modules.Values.ToArray()
        );

        public Document Create() => Build().Create(m_frame_source, m_font_manager);
    }

    #endregion

    #region Module

    public interface IModule
    {
        public static virtual Type[] Before => [];
        public static virtual Type[] After => [];

        public static abstract IModule Create(Document document);

        public void Update(Document document);
    }

    internal abstract class AModuleTemplate
    {
        public abstract IModule Create(Document document);
    }

    internal class ModuleTemplate<T> : AModuleTemplate
        where T : IModule
    {
        public override IModule Create(Document document) => T.Create(document);
    }

    #endregion

    #region Template

    public sealed class Template
    {
        internal readonly ArcheTemplate m_view_arche;
        internal readonly ArcheTemplate m_text_paragraph_arche;
        internal readonly ArcheTemplate m_text_span_arche;
        internal readonly AModuleTemplate[] m_modules;

        internal Template(ArcheTemplate view_arche, ArcheTemplate text_paragraph_arche, ArcheTemplate text_span_arche, AModuleTemplate[] modules)
        {
            m_view_arche = view_arche;
            m_text_paragraph_arche = text_paragraph_arche;
            m_text_span_arche = text_span_arche;
            m_modules = modules;
        }

        public Document Create(FrameSource? frame_source, FontManager? font_manager) =>
            new(this, frame_source, font_manager);
    }

    internal sealed class ArcheTemplate
    {
        internal readonly AStorageTemplate[] m_storages;
        internal readonly C m_type_chain;

        public ArcheTemplate(Dictionary<Type, AStorageTemplate> storages)
        {
            m_storages = storages.Values.OrderBy(a => a.Type.FullName, StringComparer.Ordinal).ToArray();
            Debug.Assert(m_storages.Length > 0);
            m_type_chain = m_storages[0].Chain();
            for (var i = 1; i < m_storages.Length; i++)
            {
                m_type_chain = m_storages[i].Chain(m_type_chain);
            }
        }

        internal Arche Create() => new(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int IndexOf<T>() => m_type_chain.IndexOf<T>();
    }

    internal abstract class AStorageTemplate(Type Type, StorageType type)
    {
        public Type Type { get; } = Type;
        internal readonly StorageType m_type = type;

        internal abstract AStorage Create();

        internal abstract C Chain();
        internal abstract C Chain(C b);
    }

    internal sealed class StorageTemplate<T>(StorageType type) : AStorageTemplate(typeof(T), type) where T : new()
    {
        internal override AStorage Create() => m_type switch
        {
            StorageType.Default => new Storage<T>(),
            StorageType.Pinned => RuntimeHelpers.IsReferenceOrContainsReferences<T>()
                ? throw new NotSupportedException($"Type {typeof(T)} is a managed type and cannot be pinned")
                : new PinnedStorage<T>(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), m_type, null)
        };

        internal override C Chain() => C<T>.Instance;
        internal override C Chain(C b) => b.Add<C<T>>();
    }

    public ulong CurrentFrame { get; private set; }

    #endregion

    #region Drop

    [Drop]
    private void Drop()
    {
        foreach (var module in m_modules)
        {
            if (module is IDisposable disposable) disposable.Dispose();
        }
    }

    #endregion

    #region Instance

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Arche ViewArche() => m_view_arche;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Arche TextParagraphArche() => m_text_paragraph_arche;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Arche TextSpanArche() => m_text_span_arche;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AStorage<T> ViewStorageOf<T>() => m_view_arche.StorageOf<T>();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AStorage<T> TextParagraphStorageOf<T>() => m_text_paragraph_arche.StorageOf<T>();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AStorage<T> TextSpanStorageOf<T>() => m_text_span_arche.StorageOf<T>();

    public sealed class Arche : IDisposable
    {
        internal readonly ArcheTemplate m_template;
        internal readonly C m_type_chain;
        internal readonly AStorage[] m_storages;
        internal EmbedList<Action> m_storage_fns_dispose;
        internal readonly Action<int, CtrlOp>[] m_storage_fns_add;
        internal readonly Action<int>[] m_storage_fns_remove;
        internal NSplitMapCtrl<uint> m_ctrl = new();

        internal Arche(ArcheTemplate template)
        {
            m_template = template;
            m_type_chain = template.m_type_chain;
            m_storages = new AStorage[template.m_storages.Length];
            for (var i = 0; i < template.m_storages.Length; i++)
            {
                var storage = template.m_storages[i];
                m_storages[i] = storage.Create();
            }
            foreach (var storage in m_storages)
            {
                var dispose = storage.Dispose;
                if (dispose == null) continue;
                m_storage_fns_dispose.Add(dispose);
            }
            m_storage_fns_add = new Action<int, CtrlOp>[m_storages.Length];
            m_storage_fns_remove = new Action<int>[m_storages.Length];
            for (var i = 0; i < m_storages.Length; i++)
            {
                var storage = m_storages[i];
                m_storage_fns_add[i] = storage.Add;
                m_storage_fns_remove[i] = storage.Remove;
            }
        }

        public override string ToString() => m_type_chain.ToString()!;

        public void Dispose()
        {
            foreach (var dispose in m_storage_fns_dispose)
            {
                dispose();
            }
            m_ctrl.Dispose();
        }

        public int GetRawCount() => m_ctrl.m_count;
        public unsafe int* GetBuckets() => m_ctrl.m_buckets;
        public unsafe NSplitMapCtrl<uint>.Ctrl* GetCtrls() => m_ctrl.m_ctrls.m_items;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AStorage<T> StorageOf<T>() => (AStorage<T>)UnsafeStorageAt(IndexOf<T>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AStorage UnsafeStorageAt(int index) => Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(m_storages), index);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T UnsafeGetDataRefAt<T>(int index) => ref UnsafeStorageAt(index).UnsafeGetDataRef<T>();

        public int Count => m_ctrl.Count;

        public ReadOnlySpan<AStorage> Storages => m_storages;

        public int Add(uint id)
        {
            var op = CtrlOp.None;
            var r = m_ctrl.TryInsert(id, false, ref op, out var idx);
            if (r != InsertResult.AddNew) throw new InvalidOperationException();
            foreach (var add in m_storage_fns_add)
            {
                add(idx, op);
            }
            return idx;
        }

        public void Remove(uint id)
        {
            var idx = m_ctrl.Remove(id);
            foreach (var remove in m_storage_fns_remove)
            {
                remove(idx);
            }
        }

        public int IndexOf<T>() => m_type_chain.IndexOf<T>();
    }

    public abstract unsafe class AStorage
    {
        public abstract Action? Dispose { get; }
        public abstract Action<int, CtrlOp> Add { get; }
        public abstract Action<int> Remove { get; }

        /// <returns>if not pinned, return null</returns>
        public abstract T* UnsafeGetDataPtr<T>();
        public abstract ref T UnsafeGetDataRef<T>();
    }

    public abstract class AStorage<T> : AStorage;

    public sealed unsafe class Storage<T> : AStorage<T>
        where T : new()
    {
        internal SplitMapData<T> m_data = new();

        public ref T GetDataRef() => ref MemoryMarshal.GetArrayDataReference(m_data.m_items!);

        public override Action? Dispose => null;

        public override Action<int, CtrlOp> Add => (idx, op) =>
        {
            m_data.ApplyOp(op);
            m_data.UnsafeAt(idx) = new();
        };

        public override Action<int> Remove => idx =>
        {
            ref var slot = ref m_data.UnsafeAt(idx);
            DisposeProxy.TryDispose(ref slot);
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                slot = default!;
            }
        };

        public override T1* UnsafeGetDataPtr<T1>() => null;
        public override ref T1 UnsafeGetDataRef<T1>() => ref Unsafe.As<T, T1>(ref MemoryMarshal.GetArrayDataReference(m_data.m_items!));
    }

    public sealed unsafe partial class PinnedStorage<T> : AStorage<T>
        where T : new()
    {
        internal NSplitMapData<T> m_data = new();

        public T* GetDataPtr() => m_data.m_items;
        public ref T GetDataRef() => ref *m_data.m_items;

        public override Action Dispose => () => m_data.Dispose();

        public override Action<int, CtrlOp> Add => (idx, op) =>
        {
            m_data.ApplyOp(op);
            m_data.UnsafeAt(idx) = new();
        };

        public override Action<int> Remove => idx => { DisposeProxy.TryDispose(ref m_data.UnsafeAt(idx)); };

        public override T1* UnsafeGetDataPtr<T1>() => (T1*)m_data.m_items;
        public override ref T1 UnsafeGetDataRef<T1>() => ref *(T1*)m_data.m_items;
    }

    #endregion

    #region At

    public Arche ArcheOf(NodeId id) => id.Type switch
    {
        NodeType.View => m_view_arche,
        NodeType.TextParagraph => m_text_paragraph_arche,
        NodeType.TextSpan => m_text_span_arche,
        _ => throw new ArgumentOutOfRangeException()
    };

    public ref T At<T>(NodeId id)
    {
        var arche = ArcheOf(id);
        var nth = arche.IndexOf<T>();
        if (nth < 0) throw new InvalidOperationException($"{typeof(T)} dose not exists in {{ {id.Type} }} node");
        var storage = arche.UnsafeStorageAt(nth);
        return ref Unsafe.Add(ref storage.UnsafeGetDataRef<T>(), id.Index);
    }

    public unsafe T* PtrAt<T>(NodeId id)
    {
        var arche = ArcheOf(id);
        var nth = arche.IndexOf<T>();
        if (nth < 0) throw new InvalidOperationException($"{typeof(T)} dose not exists in {{ {id.Type} }} node");
        var storage = arche.UnsafeStorageAt(nth);
        return storage.UnsafeGetDataPtr<T>() + id.Index;
    }

    public ref T UnsafeAt<T>(NodeId id)
    {
        var arche = ArcheOf(id);
        var nth = arche.IndexOf<T>();
        var storage = arche.UnsafeStorageAt(nth);
        return ref Unsafe.Add(ref storage.UnsafeGetDataRef<T>(), id.Index);
    }

    public unsafe T* UnsafePtrAt<T>(NodeId id)
    {
        var arche = ArcheOf(id);
        var nth = arche.IndexOf<T>();
        var storage = arche.UnsafeStorageAt(nth);
        return storage.UnsafeGetDataPtr<T>() + id.Index;
    }

    #endregion

    #region Node

    public ref RootData AddRoot(NodeId Id, LocaleId? DefaultLocale = null, float Dpi = 96) =>
        ref AddRoot(Id, AvailableSpace.MinContent, AvailableSpace.MinContent, DefaultLocale, Dpi);
    public ref RootData AddRoot(NodeId Id, AvailableSpace X, AvailableSpace Y, LocaleId? DefaultLocale = null, float Dpi = 96, bool UseRounding = true)
    {
        if (Id.Type != NodeType.View) throw new InvalidOperationException("Root must be view.");
        ref var root = ref m_roots.GetValueRefOrUninitialized(Id, out _);
        root.DefaultLocale = DefaultLocale ?? default;
        root.Node = Id;
        root.AvailableSpaceXValue = X.Value;
        root.AvailableSpaceYValue = Y.Value;
        root.AvailableSpaceX = X.Type;
        root.AvailableSpaceY = Y.Type;
        root.Dpi = Dpi;
        root.UseRounding = UseRounding;
        return ref root;
    }

    public ref RootData GetRootData(NodeId Id, out bool Exists)
    {
        if (Id.Type != NodeType.View) throw new InvalidOperationException("Root must be view.");
        return ref m_roots.GetValueRefOrNullRef(Id, out Exists);
    }

    public bool RemoveRoot(NodeId id) => m_roots.Remove(id);

    public NodeId CreateView()
    {
        var id = m_node_id_inc++;
        var index = m_view_arche.Add(id);
        m_view_arche.UnsafeGetDataRefAt<CommonData>(index).NodeId = id;
        return new((uint)index, id, NodeType.View);
    }

    public NodeId CreateTextParagraph()
    {
        var id = m_node_id_inc++;
        var index = m_text_paragraph_arche.Add(id);
        m_text_paragraph_arche.UnsafeGetDataRefAt<CommonData>(index).NodeId = id;
        return new((uint)index, id, NodeType.TextParagraph);
    }

    public NodeId CreateTextSpan()
    {
        var id = m_node_id_inc++;
        var index = m_text_span_arche.Add(id);
        m_text_span_arche.UnsafeGetDataRefAt<CommonData>(index).NodeId = id;
        return new((uint)index, id, NodeType.TextSpan);
    }

    public void RemoveNode(NodeId id)
    {
        if (id.Type == NodeType.Null) return;
        ref var common = ref UnsafeAt<CommonData>(id);
        {
            if (common.Parent is { } parent)
            {
                ref var childs = ref UnsafeAt<ChildsData>(parent);
                childs.m_childs.Remove(id);
                DirtyLayout(parent);
            }
        }
        switch (id.Type)
        {
            case NodeType.View:
            {
                ref var childs = ref UnsafeAt<ChildsData>(id);
                foreach (var child in childs)
                {
                    ref var parent = ref UnsafeAt<CommonData>(child);
                    parent.Parent = null;
                }
                m_view_arche.Remove(id.Id);
                m_roots.Remove(id);
                break;
            }
            case NodeType.TextParagraph:
            {
                ref var childs = ref UnsafeAt<ChildsData>(id);
                foreach (var child in childs)
                {
                    ref var parent = ref UnsafeAt<CommonData>(child);
                    parent.Parent = null;
                }
                m_text_paragraph_arche.Remove(id.Id);
                break;
            }
            case NodeType.TextSpan:
            {
                m_text_span_arche.Remove(id.Id);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void AddChild(NodeId id, NodeId child)
    {
        ref var child_common = ref UnsafeAt<CommonData>(child);
        if (child_common.Parent is not null) throw new ArgumentException("Target child node already has a parent.");
        switch (id.Type)
        {
            case NodeType.View:
            {
                if (child.Type == NodeType.TextSpan) throw new InvalidOperationException("Cannot add a text span to a view.");
                ref var childs = ref m_view_arche.UnsafeGetDataRefAt<ChildsData>((int)id.Index);
                childs.m_childs.Add(child);
                break;
            }
            case NodeType.TextParagraph:
            {
                if (child.Type != NodeType.TextSpan) throw new InvalidOperationException("Can only add a text span to a text paragraph.");
                ref var childs = ref m_text_paragraph_arche.UnsafeGetDataRefAt<ChildsData>((int)id.Index);
                childs.m_childs.Add(child);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(id), $"{id.Type} can not have childs.");
        }

        child_common.Parent = id;
        DirtyLayout(child);
    }

    public void RemoveChild(NodeId id, NodeId child)
    {
        ref var childs = ref UnsafeAt<ChildsData>(id);
        ref var child_common = ref UnsafeAt<CommonData>(child);
        if (child_common.Parent != id) throw new ArgumentException("Target node is not a child of this node.");
        childs.m_childs.Remove(child);
        child_common.Parent = null;
        DirtyLayout(id);
    }

    #endregion

    #region Dirty

    public void DirtyLayout(NodeId node)
    {
        while (true)
        {
            ref var data = ref UnsafeAt<CommonData>(node);
            if (node.Type is NodeType.View)
            {
                ref var layout = ref UnsafeAt<LayoutData>(node);
                if (layout.IsLayoutDirty(this)) return;
                layout.MarkLayoutDirty(this);
                layout.LayoutCache.Flags = LayoutCacheFlags.Empty;
            }
            if (data.Parent is { } parent)
            {
                node = parent;
                continue;
            }
            break;
        }
    }

    #endregion

    #region Update

    public void Update()
    {
        foreach (var update in m_modules_update)
        {
            update(this);
        }

        if (!m_extern_frame_source)
        {
            var data = m_frame_source.Data;
            m_frame_source.Data = new()
            {
                NthFrame = data.NthFrame + 1,
                TimeTicks = (ulong)Stopwatch.GetTimestamp(),
            };
            CurrentFrame = data.NthFrame + 1;
        }
        else
        {
            CurrentFrame = m_frame_source.Data.NthFrame;
        }
    }

    #endregion
}

[Flags]
public enum ArcheTarget : byte
{
    None = 0,
    View = 1 << 0,
    TextParagraph = 1 << 1,
    TextSpan = 1 << 2,
    All = View | TextParagraph | TextSpan,
}

public struct ArcheTargetEnumerator(ArcheTarget last)
{
    private ArcheTarget last = last;

    public ArcheTarget Current { get; set; }

    public bool MoveNext()
    {
        if (last == ArcheTarget.None) return false;
        var offset = BitOperations.TrailingZeroCount((int)last);
        Current = (ArcheTarget)(1 << offset);
        last &= ~Current;
        return true;
    }
}

public static class DocumentEx
{
    extension(ArcheTarget self)
    {
        public ArcheTargetEnumerator GetEnumerator() => new(self);
    }

    extension(Document.AStorage self)
    {
        public Document.Storage<T> AsCommon<T>() where T : new() => (Document.Storage<T>)self;
        public Document.PinnedStorage<T> AsPinned<T>() where T : new() => (Document.PinnedStorage<T>)self;
    }

    extension<T>(Document.AStorage<T> self) where T : new()
    {
        public Document.Storage<T> AsCommon() => (Document.Storage<T>)self;
        public Document.PinnedStorage<T> AsPinned() => (Document.PinnedStorage<T>)self;
    }
}
