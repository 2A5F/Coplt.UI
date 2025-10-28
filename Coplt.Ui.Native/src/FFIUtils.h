#pragma once

#include "Com.h"

namespace Coplt
{
    inline NodeType typ(const NodeId id)
    {
        return static_cast<NodeType>(id.VersionAndType & 0b1111);
    }

    template <class T>
    struct FFIOrderedSetIter
    {
        FFIOrderedSet<T>* m_self;
        FFIOrderedSetNode<T>* m_cur;

        FFIOrderedSetIter(FFIOrderedSet<T>* self) : m_self(self), m_cur(nullptr)
        {
        }

        bool MoveNext()
        {
            if (m_self->m_nodes == nullptr) return false;
            if (m_cur == nullptr)
            {
                if (m_self->m_first == -1) return false;
                m_cur = &m_self->m_nodes[m_self->m_first];
            }
            else
            {
                if (m_cur->OrderNext == -1) return false;
                m_cur = &m_self->m_nodes[m_cur->OrderNext];
            }
            return true;
        }

        T& Current()
        {
            return m_cur->Value;
        }

        T& operator*()
        {
            return m_cur->Value;
        }

        T* operator->()
        {
            return std::addressof(m_cur->Value);
        }
    };

    template <class T>
    FFIOrderedSetIter<T> iter(FFIOrderedSet<T>& set)
    {
        return FFIOrderedSetIter(&set);
    }
}
