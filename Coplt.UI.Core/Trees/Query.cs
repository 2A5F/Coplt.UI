using System.Runtime.CompilerServices;
using Coplt.UI.Collections;

namespace Coplt.UI.Trees;

public static partial class Query
{
    public struct Q<T0>
    {
        public ref struct Enumerator(Document document)
        {
            private Span<Document.Arche>.Enumerator m_arches = document.m_arches.AsSpan().GetEnumerator();
            private NSplitMapCtrl<NodeId>.Enumerator m_arch;
            private ref T0 t0_data = ref Unsafe.NullRef<T0>();
            private int index = -1;

            public bool MoveNext()
            {
                if (index < 0) goto Next;
                Body:
                {
                    if (!m_arch.MoveNext())
                    {
                        index = -1;
                        goto Next;
                    }
                    index = m_arch.Current;
                    return true;
                }
                Next:
                {
                    if (!m_arches.MoveNext()) return false;
                    ref var arch = ref m_arches.Current;
                    var t0_index = arch.IndexOf<T0>();
                    if (t0_index < 0) goto Next;
                    m_arch = arch.m_ctrl.GetEnumerator();
                    t0_data = ref arch.UnsafeGetDataRefAt<T0>(t0_index);
                    goto Body;
                }
            }

            public ref T0 Current => ref Unsafe.Add(ref t0_data, index);
        }
    }

    public struct Q<T0, T1> { }
    public struct Q<T0, T1, T2> { }
    public struct Q<T0, T1, T2, T3> { }
}

public struct Query<Q>(Document Document)
{
    public Document Document { get; } = Document;
}

public static partial class Query
{
    extension<T0>(Query<Q<T0>> query)
    {
        public Q<T0>.Enumerator GetEnumerator() => new(query.Document);
    }
}
