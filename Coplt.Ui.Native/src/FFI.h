#pragma once

#include "Com.h"

namespace Coplt::FFIUtils
{
    struct FFIOrderedSetNodeBase
    {
        i32 HashCode;
        i32 Next;
        i32 OrderNext;
        i32 OrderPrev;
    };

    template <class T>
    struct FFIOrderedSetEnumerator
    {
        struct FFIOrderedSetNode : FFIOrderedSetNodeBase
        {
            T Value;
        };

        const FFIOrderedSet* set{};
        FFIOrderedSetNode* cur{};

        explicit FFIOrderedSetEnumerator(const FFIOrderedSet* set) : set(set)
        {
        }

        bool MoveNext()
        {
            if (set->m_nodes == nullptr) return false;
            if (cur == nullptr)
            {
                if (set->m_first == -1) return false;
                cur = &static_cast<FFIOrderedSetNode*>(set->m_nodes)[set->m_first];
            }
            else
            {
                if (cur->OrderNext == -1) return false;
                cur = &static_cast<FFIOrderedSetNode*>(set->m_nodes)[cur->OrderNext];
            }
            return true;
        }

        T* Current()
        {
            return std::addressof(cur->Value);
        }
    };

    template <class T>
    FFIOrderedSetEnumerator<T> GetEnumerator(const FFIOrderedSet* set)
    {
        return FFIOrderedSetEnumerator<T>(set);
    }

    inline NodeType GetType(const NodeId id)
    {
        return static_cast<NodeType>(id.IdAndType & 0xF);
    }
}
