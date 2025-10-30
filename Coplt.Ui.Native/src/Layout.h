#pragma once

#include "Com.h"

namespace Coplt::LayoutCalc
{
    COPLT_FORCE_INLINE
    ChildsData* GetChildsData(NLayoutContext* ctx, i32 index)
    {
        return &ctx->node_childs_data[index];
    }

    COPLT_FORCE_INLINE
    StyleData* GetStyleData(NLayoutContext* ctx, i32 index)
    {
        return &ctx->node_style_data[index];
    }

    COPLT_FORCE_INLINE
    CommonData* GetCommonData(NLayoutContext* ctx, i32 index)
    {
        return &ctx->node_common_data[index];
    }

    struct CtxNodeRef
    {
        NLayoutContext* ctx;
        NodeId id;

        explicit CtxNodeRef(
            NLayoutContext* ctx, const NodeId id
        ) : ctx(ctx), id(id)
        {
        }

        ChildsData& ChildsData() const
        {
            return *GetChildsData(ctx, id.Index);
        }

        StyleData& StyleData() const
        {
            return *GetStyleData(ctx, id.Index);
        }

        CommonData& CommonData() const
        {
            return *GetCommonData(ctx, id.Index);
        }
    };

    void CollectDirty(CtxNodeRef root);
}
