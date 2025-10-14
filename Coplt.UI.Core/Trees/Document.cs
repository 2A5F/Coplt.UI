using System.Collections.Frozen;
using System.Runtime.InteropServices;
using Coplt.Dropping;
using Coplt.UI.Collections;
using Coplt.UI.Trees.Datas;

namespace Coplt.UI.Trees;

[Dropping]
public sealed partial class Document
{
    #region Fields

    internal readonly Template m_template;
    internal readonly Arche[] m_arches;
    internal DenseSet m_dense_set;

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
            StorageType.Pinned => new PinnedStorage<T>(this),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    #endregion

    #region Instance

    public sealed class Arche
    {
        internal readonly ArcheTemplate m_template;
        internal readonly AStorage[] m_storages;

        internal Arche(ArcheTemplate template)
        {
            m_template = template;
            m_storages = new AStorage[template.m_storages.Count];
            foreach (var storage in template.m_storages.Values)
            {
                m_storages[storage.m_index] = storage.Create();
            }
        }
    }

    public abstract class AStorage { }

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
        internal readonly StorageTemplate<T> m_template;
        internal PinChunkList<T> m_list = new();

        internal Storage(StorageTemplate<T> template) : base(template) { }
    }

    public sealed class PinnedStorage<T> : AStorage<T>
        where T : new()
    {
        internal readonly StorageTemplate<T> m_template;
        internal PinChunkList<T> m_list = new();

        internal PinnedStorage(StorageTemplate<T> template) : base(template) { }
    }

    #endregion
}
