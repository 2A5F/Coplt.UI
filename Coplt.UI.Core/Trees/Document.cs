using System.Collections.Frozen;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Coplt.Dropping;
using Coplt.UI.Collections;
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
    // ReSharper disable once CollectionNeverQueried.Global
    internal readonly IModule[] m_modules;
    internal readonly Action[] m_modules_update;
    internal readonly Arche[] m_arches;
    internal EmbedQueue<NodeId> m_node_id_recycle = new();
    internal uint m_node_id_inc;

    #endregion

    #region Ctor

    internal Document(Template template)
    {
        m_template = template;
        m_arches = new Arche[m_template.m_arches.Length];
        for (var i = 0; i < m_arches.Length; i++)
        {
            var tem = m_template.m_arches[i];
            if (tem == null) continue;
            m_arches[i] = tem.Create();
        }
        m_modules = new IModule[m_template.m_modules.Length];
        m_modules_update = new Action[m_template.m_modules.Length];
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
        private readonly Dictionary<NodeType, Dictionary<Type, AStorageTemplate>> m_types = new();

        public Builder()
        {
            Attach<NodeId>();
            Attach<ParentData>();
            Attach<CommonStyleData>(storage: StorageType.Pinned);
            Attach<CommonEventData>();
            Attach<ChildsData>(types: NodeTypes.View, storage: StorageType.Pinned);
            Attach<ViewStyleData>(types: NodeTypes.View, storage: StorageType.Pinned);
            Attach<ViewLayoutData>(types: NodeTypes.View, storage: StorageType.Pinned);
            Attach<TextStyleData>(types: NodeTypes.Text, storage: StorageType.Pinned);
            With<LayoutModule>();
        }

        public Builder Attach<T>(NodeTypes types = NodeTypes.All, StorageType storage = StorageType.Default)
            where T : new()
        {
            foreach (var type in types)
            {
                ref var ts = ref CollectionsMarshal.GetValueRefOrAddDefault(m_types, type, out var exists);
                if (!exists) ts = new();
                ref var t = ref CollectionsMarshal.GetValueRefOrAddDefault(ts, typeof(T), out exists);
                if (exists) throw new ArgumentException($"Type {typeof(T)} has already been attached to {type}.");
                t = new StorageTemplate<T>(storage);
            }
            return this;
        }

        public Builder With<T>() where T : IModule
        {
            m_modules.TryAdd(typeof(T), new ModuleTemplate<T>());
            return this;
        }

        public Template Build()
        {
            var arches = new ArcheTemplate?[NodeType.Length];
            foreach (var (type, arch) in m_types)
            {
                arches[(int)type] = new ArcheTemplate(arch);
            }
            return new(arches, m_modules.Values.ToArray());
        }

        public Document Create() => Build().Create();
    }

    #endregion

    #region Module

    public interface IModule
    {
        public static virtual Type[] Before => [];
        public static virtual Type[] After => [];

        public static abstract IModule Create(Document document);

        public void Update();
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
        internal readonly ArcheTemplate?[] m_arches;
        internal readonly AModuleTemplate[] m_modules;

        internal Template(ArcheTemplate?[] arches, AModuleTemplate[] modules)
        {
            m_arches = arches;
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
        foreach (var arch in m_arches)
        {
            arch.Dispose();
        }
    }

    #endregion

    #region Instance

    public Arche ArcheOf(NodeType type) => m_arches[(int)type];
    public ReadOnlySpan<Arche> Arches => m_arches;

    public sealed class Arche : IDisposable
    {
        internal readonly ArcheTemplate m_template;
        internal readonly C m_type_chain;
        internal readonly AStorage[] m_storages;
        internal EmbedList<Action> m_storage_fns_dispose;
        internal readonly Action<int, CtrlOp>[] m_storage_fns_add;
        internal readonly Action<int>[] m_storage_fns_remove;
        internal readonly AStorage m_node_id_storage;
        internal NSplitMapCtrl<NodeId> m_ctrl = new();

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
            m_node_id_storage = UnsafeStorageAt(m_type_chain.IndexOf<NodeId>());
        }

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

        public int Add(NodeId id)
        {
            var op = CtrlOp.None;
            var r = m_ctrl.TryInsert(id, false, ref op, out var idx);
            if (r != InsertResult.AddNew) throw new InvalidOperationException();
            if (!op.IsNone)
            {
                foreach (var add in m_storage_fns_add)
                {
                    add(idx, op);
                }
            }
            Unsafe.Add(ref m_node_id_storage.UnsafeGetDataRef<NodeId>(), idx) = id;
            return idx;
        }

        public void Remove(NodeId id)
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

    #region Query

    public Query<Query.Q<T0>> Query<T0>() => new(this);
    public Query<Query.Q<T0, T1>> Query<T0, T1>() => new(this);
    public Query<Query.Q<T0, T1, T2>> Query<T0, T1, T2>() => new(this);
    public Query<Query.Q<T0, T1, T2, T3>> Query<T0, T1, T2, T3>() => new(this);

    #endregion

    #region At

    public ref T At<T>(NodeId id)
    {
        var arche = ArcheOf(id.Type);
        var nth = arche.IndexOf<T>();
        if (nth < 0) throw new InvalidOperationException();
        var i = arche.m_ctrl.FindValue(id);
        if (i < 0) throw new IndexOutOfRangeException();
        var storage = arche.UnsafeStorageAt(nth);
        return ref Unsafe.Add(ref storage.UnsafeGetDataRef<T>(), i);
    }

    #endregion

    #region Create

    public NodeId CreateNode(NodeType type) => CreateNode(type, out _);

    /// <param name="type"></param>
    /// <param name="index">temporarily available index, if added again or the parameter will become invalid</param>
    public NodeId CreateNode(NodeType type, out int index)
    {
        if (m_node_id_recycle.TryDequeue(out var id))
            id = new(id.Id, id.Version + 1, type);
        else id = new(m_node_id_inc++, 1, type);
        index = m_arches[(int)type].Add(id);
        return id;
    }

    public void Remove(NodeId id)
    {
        var type = id.Type;
        m_arches[(int)type].Remove(id);
        m_node_id_recycle.Enqueue(id);
    }

    #endregion

    #region Update

    public void Update()
    {
        foreach (var update in m_modules_update)
        {
            update();
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
