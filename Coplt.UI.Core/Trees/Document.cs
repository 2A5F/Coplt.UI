using System.Collections.Frozen;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Coplt.Dropping;
using Coplt.UI.Collections;
using Coplt.UI.Layouts;
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
    internal readonly Arche m_arche;
    [Drop]
    internal NativeMap<NodeId, RootData> m_roots;
    // ReSharper disable once CollectionNeverQueried.Global
    internal readonly IModule[] m_modules;
    internal readonly Action<Document>[] m_modules_update;
    internal uint m_node_id_inc;

    #endregion

    #region Ctor

    internal Document(Template template)
    {
        m_template = template;
        m_arche = m_template.m_arche.Create();
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
        private readonly Dictionary<Type, AStorageTemplate> m_types = new();

        public Builder()
        {
            Attach<ParentData>();
            Attach<ChildsData>(storage: StorageType.Pinned);
            Attach<CommonData>(storage: StorageType.Pinned);
            Attach<StyleData>(storage: StorageType.Pinned);
            With<LayoutModule>();
        }

        public Builder Attach<T>(StorageType storage = StorageType.Default)
            where T : new()
        {
            ref var t = ref CollectionsMarshal.GetValueRefOrAddDefault(m_types, typeof(T), out var exists);
            if (exists) throw new ArgumentException($"Type {typeof(T)} has already been attached.");
            t = new StorageTemplate<T>(storage);
            return this;
        }

        public Builder With<T>() where T : IModule
        {
            m_modules.TryAdd(typeof(T), new ModuleTemplate<T>());
            return this;
        }

        public Template Build() => new(new ArcheTemplate(m_types), m_modules.Values.ToArray());

        public Document Create() => Build().Create();
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
        internal readonly ArcheTemplate m_arche;
        internal readonly AModuleTemplate[] m_modules;

        internal Template(ArcheTemplate arche, AModuleTemplate[] modules)
        {
            m_arche = arche;
            m_modules = modules;
        }

        public Document Create() => new(this);
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

    public ref T At<T>(NodeId id)
    {
        var nth = m_arche.IndexOf<T>();
        if (nth < 0) throw new InvalidOperationException();
        var storage = m_arche.UnsafeStorageAt(nth);
        return ref Unsafe.Add(ref storage.UnsafeGetDataRef<T>(), id.Index);
    }

    public unsafe T* UnsafePtrAt<T>(NodeId id)
    {
        var nth = m_arche.IndexOf<T>();
        if (nth < 0) throw new InvalidOperationException();
        var storage = m_arche.UnsafeStorageAt(nth);
        return storage.UnsafeGetDataPtr<T>() + id.Index;
    }

    #endregion

    #region Node

    public ref RootData AddRoot(NodeId id) => ref AddRoot(id, AvailableSpace.MinContent, AvailableSpace.MinContent);
    public ref RootData AddRoot(NodeId id, AvailableSpace X, AvailableSpace Y, bool UseRounding = true)
    {
        ref var root = ref m_roots.GetValueRefOrUninitialized(id, out _);
        root.Node = id;
        root.AvailableSpaceXValue = X.Value;
        root.AvailableSpaceYValue = Y.Value;
        root.AvailableSpaceX = X.Type;
        root.AvailableSpaceY = Y.Type;
        root.UseRounding = UseRounding;
        return ref root;
    }

    public bool RemoveRoot(NodeId id) => m_roots.Remove(id);

    public NodeId CreateView()
    {
        var id = m_node_id_inc++;
        var index = m_arche.Add(id);
        return new((uint)index, id, NodeType.View);
    }

    public void RemoveView(NodeId id)
    {
        if (id.Type != NodeType.View) throw new InvalidOperationException();
        {
            ref var parent = ref At<ParentData>(id);
            if (parent.Parent is { } par)
            {
                ref var childs = ref At<ChildsData>(par);
                childs.UnsafeRemove(id);
            }
        }
        {
            ref var childs = ref At<ChildsData>(id);
            foreach (var child in childs)
            {
                ref var parent = ref At<ParentData>(child);
                parent.UnsafeRemoveParent();
            }
        }
        m_arche.Remove(id.Id);
    }

    #endregion

    #region Dirty

    public void DirtyLayout(NodeId id)
    {
        while (true)
        {
            ref var data = ref At<CommonData>(id);
            if ((data.DirtyFlags & DirtyFlags.Layout) != 0) return;
            data.DirtyFlags |= DirtyFlags.Layout;
            if (At<ParentData>(id).Parent is { } parent)
            {
                id = parent;
                continue;
            }
            break;
        }
    }

    public void DirtyTextLayout(NodeId id)
    {
        At<CommonData>(id).DirtyFlags |= DirtyFlags.TextLayout;
        DirtyLayout(id);
    }

    #endregion

    #region Update

    public void Update()
    {
        foreach (var update in m_modules_update)
        {
            update(this);
        }
    }

    #endregion
}

public static class DocumentEx
{
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
