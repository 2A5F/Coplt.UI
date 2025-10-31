#pragma once

#include "Com.h"

namespace Coplt
{
    template <class Self>
    struct BaseTextLayout : ComImpl<Self, ITextLayout>
    {
        void ClearCache()
        {
            // todo
        }

        void AddText(NodeId parent, u32 length)
        {
            // todo
        }

        void AddInlineBlock(NodeId parent)
        {
            // todo
        }

        void AddBlock(NodeId parent)
        {
            // todo
        }

        void StartScope(NodeId node)
        {
            // todo
        }

        void EndScope(NodeId node)
        {
            // todo
        }
    };
}
