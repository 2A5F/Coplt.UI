#pragma once

#include "Com.h"
#include "Assert.h"
#include "FFI.h"
#include "Map.h"
#include "TextLayout.h"
#include "Utils.h"

namespace Coplt
{
    struct LibUi;
}

namespace Coplt::LayoutCalc
{
    struct Layout;

    Rc<Layout> CreateLayout(Rc<LibUi> lib);

    COPLT_FORCE_INLINE
    ChildsData* GetChildsData(NLayoutContext* ctx, u32 index)
    {
        return nullptr;
        // return &ctx->node_childs_data[index];
    }

    COPLT_FORCE_INLINE
    StyleData* GetStyleData(NLayoutContext* ctx, u32 index)
    {
        return nullptr;
        // return &ctx->node_style_data[index];
    }

    COPLT_FORCE_INLINE
    CommonData* GetCommonData(NLayoutContext* ctx, u32 index)
    {
        return nullptr;
        // return &ctx->node_common_data[index];
    }

    struct CtxNodeRef
    {
        NLayoutContext* ctx;
        NodeId id;

        CtxNodeRef()
            : ctx(nullptr), id()
        {
        }

        explicit CtxNodeRef(
            NLayoutContext* ctx, const NodeId id
        )
            : ctx(ctx), id(id)
        {
        }

        NodeType Type() const
        {
            return FFIUtils::GetType(id);
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

        // const NString& GetText(const NodeId text) const
        // {
        //     const auto& childs = ChildsData();
        //     auto& texts = *ffi_map<u32, NString>(&childs.m_texts);
        //     return *texts.UnsafeAt(text.Index);
        // }
        //
        // const NString& GetText(const u32 index) const
        // {
        //     const auto& childs = ChildsData();
        //     auto& texts = *ffi_map<u32, NString>(&childs.m_texts);
        //     return *texts.UnsafeAt(index);
        // }
    };

    namespace Texts
    {
        extern "C" HResultE coplt_ui_layout_touch_text(
            void* self, NLayoutContext* ctx, const NodeId& node
        );
    }
}
