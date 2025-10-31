#pragma once

#include "Com.h"

namespace Coplt::FFIUtils
{
    template <class T>
    struct FFIOrderedSetEnumerator
    {
        const FFIOrderedSet<T>* set{};
        FFIOrderedSetNode<T>* cur{};

        explicit FFIOrderedSetEnumerator(const FFIOrderedSet<T>* set) : set(set)
        {
        }

        bool MoveNext()
        {
            if (set->m_nodes == nullptr) return false;
            if (cur == nullptr)
            {
                if (set->m_first == -1) return false;
                cur = &set->m_nodes[set->m_first];
            }
            else
            {
                if (cur->OrderNext == -1) return false;
                cur = &set->m_nodes[cur->OrderNext];
            }
            return true;
        }

        T* Current()
        {
            return std::addressof(cur->Value);
        }
    };

    template <class T>
    FFIOrderedSetEnumerator<T> GetEnumerator(const FFIOrderedSet<T>* set)
    {
        return FFIOrderedSetEnumerator(set);
    }

    inline NodeType GetType(const NodeId id)
    {
        return static_cast<NodeType>(id.IdAndType & 0xF);
    }
}
