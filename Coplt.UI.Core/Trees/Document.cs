using System.Collections.Frozen;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Coplt.Dropping;
using Coplt.UI.Collections;
using Coplt.UI.Trees.Datas;
using Coplt.UI.Utilities;

namespace Coplt.UI.Trees;

[Dropping(Unmanaged = true)]
public sealed partial class Document
{
    #region Fields

    internal readonly Template m_template;
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
    }

    #endregion

    #region Builder

    public sealed class Builder
    {
        private readonly Dictionary<NodeType, Dictionary<Type, AStorageTemplate>> m_types = new();

        public Builder()
        {
            Attach<ParentData>();
            Attach<CommonStyleData>(storage: StorageType.Pinned);
            Attach<CommonEventData>();
            Attach<ChildsData>(types: NodeTypes.View, storage: StorageType.Pinned);
            Attach<ViewStyleData>(types: NodeTypes.View, storage: StorageType.Pinned);
            Attach<ViewLayoutData>(types: NodeTypes.View, storage: StorageType.Pinned);
            Attach<TextStyleData>(types: NodeTypes.Text, storage: StorageType.Pinned);
        }

        public Builder Attach<T>(NodeTypes types = NodeTypes.All, StorageType storage = StorageType.Default)
            where T : new()
        {
            var i = 0;
            foreach (var type in types)
            {
                ref var ts = ref CollectionsMarshal.GetValueRefOrAddDefault(m_types, type, out var exists);
                if (!exists) ts = new();
                ref var t = ref CollectionsMarshal.GetValueRefOrAddDefault(ts!, typeof(T), out exists);
                if (exists) throw new ArgumentException($"Type {typeof(T)} has already been attached to {type}.");
                t = new StorageTemplate<T>(i++, storage);
            }
            return this;
        }

        public Template Build()
        {
            var arches = new ArcheTemplate?[NodeType.Length];
            foreach (var (type, arch) in m_types)
            {
                arches[(int)type] = new ArcheTemplate(arch.ToFrozenDictionary());
            }
            return new(arches);
        }

        public Document Create() => Build().Create();
    }

    #endregion

    #region Template

    public sealed class Template
    {
        internal readonly ArcheTemplate?[] m_arches;

        internal Template(ArcheTemplate?[] arches)
        {
            m_arches = arches;
        }

        public Document Create() => new(this);
    }

    internal sealed class ArcheTemplate(FrozenDictionary<Type, AStorageTemplate> storages)
    {
        internal readonly FrozenDictionary<Type, AStorageTemplate> m_storages = storages;

        internal Arche Create() => new(this);
    }

    internal abstract class AStorageTemplate(int index, StorageType type)
    {
        internal readonly int m_index = index;
        internal readonly StorageType m_type = type;

        internal abstract AStorage Create();
    }

    internal sealed class StorageTemplate<T>(int index, StorageType type) : AStorageTemplate(index, type) where T : new()
    {
        internal override AStorage Create() => type switch
        {
            StorageType.Default => new Storage<T>(this),
            StorageType.Pinned => RuntimeHelpers.IsReferenceOrContainsReferences<T>()
                ? new Storage<T>(this)
                : new PinnedStorage<T>(this),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    #endregion

    #region Drop

    [Drop]
    private void Drop()
    {
        foreach (var arch in m_arches)
        {
            arch.Dispose();
        }
    }

    #endregion

    #region Instance

    public sealed class Arche : IDisposable
    {
        internal readonly ArcheTemplate m_template;
        internal readonly AStorage[] m_storages;
        internal EmbedList<Action> m_storage_fns_dispose;
        internal readonly Action<int, CtrlOp>[] m_storage_fns_add;
        internal readonly Action<int>[] m_storage_fns_remove;
        internal NSplitMapCtrl<NodeId> m_ctrl = new();

        internal Arche(ArcheTemplate template)
        {
            m_template = template;
            m_storages = new AStorage[template.m_storages.Count];
            foreach (var storage in template.m_storages.Values)
            {
                m_storages[storage.m_index] = storage.Create();
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

        public void Dispose()
        {
            foreach (var dispose in m_storage_fns_dispose)
            {
                dispose();
            }
        }

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
    }

    public abstract class AStorage
    {
        public abstract Action? Dispose { get; }
        public abstract Action<int, CtrlOp> Add { get; }
        public abstract Action<int> Remove { get; }
    }

    public abstract class AStorage<T> : AStorage
        where T : new()
    {
        internal readonly StorageTemplate<T> m_template;

        internal AStorage(StorageTemplate<T> mTemplate)
        {
            m_template = mTemplate;
        }
    }

    public sealed class Storage<T> : AStorage<T>
        where T : new()
    {
        internal SplitMapData<T> m_data = new();

        internal Storage(StorageTemplate<T> template) : base(template) { }

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
    }

    public sealed partial class PinnedStorage<T> : AStorage<T>
        where T : new()
    {
        internal NSplitMapData<T> m_data = new();

        internal PinnedStorage(StorageTemplate<T> template) : base(template) { }

        public override Action Dispose => () => m_data.Dispose();

        public override Action<int, CtrlOp> Add => (idx, op) =>
        {
            m_data.ApplyOp(op);
            m_data.UnsafeAt(idx) = new();
        };

        public override Action<int> Remove => idx => { DisposeProxy.TryDispose(ref m_data.UnsafeAt(idx)); };
    }

    #endregion

    #region Create

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
}
