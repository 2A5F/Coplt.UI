#pragma once

#include <utility>
#include <mutex>

#include "Com.h"
#include "Map.h"

namespace Coplt
{
    struct FontManager
    {
        struct Node
        {
            u64 m_id{};
            Rc<IFontFace> m_face{};
            u64 m_last_use_frame{};
            u64 m_last_use_time{};
            i32 m_newer{};
            i32 m_older{};
        };

        u64 m_expire_frame{180};
        u64 m_expire_time{30000000};
        u64 m_id_inc{1};
        Map<u64, Node> m_id_to_node{};
        Map<usize, u64> m_face_to_id{};
        std::mutex m_mutex{};
        u64 m_cur_frame{};
        u64 m_cur_time{};
        i32 m_newest{};
        i32 m_oldest{};

        void AddNode(u64 id, IFontFace* face);
        void MakeNewest(Node& node);
    };

    template <class Self>
    struct FontManagerBase : ComImpl<Self, IFontManager>, FontManager
    {
        COPLT_IMPL_START

        COPLT_FORCE_INLINE
        u64 Impl_SetAssocUpdate(void* Data, Func<void, void*, IFontFace*, u64>* OnAdd, Func<void, void*, IFontFace*, u64>* OnExpired);

        COPLT_FORCE_INLINE
        void* Impl_RemoveAssocUpdate(u64 AssocUpdateId);

        COPLT_FORCE_INLINE
        void Impl_SetExpireFrame(u64 FrameCount);

        COPLT_FORCE_INLINE
        void Impl_SetExpireTime(u64 TimeTicks);

        COPLT_FORCE_INLINE
        u64 Impl_GetCurrentFrame() const;

        COPLT_FORCE_INLINE
        void Impl_Update(u64 CurrentTime);

        COPLT_FORCE_INLINE
        u64 Impl_FontFaceToId(IFontFace* Face);

        COPLT_FORCE_INLINE
        IFontFace* Impl_IdToFontFace(u64 Id);

        COPLT_IMPL_END
    };

    template <class Self>
    u64 FontManagerBase<Self>::Impl_SetAssocUpdate(
        void* Data,
        Func<void, void*, IFontFace*, u64>* OnAdd,
        Func<void, void*, IFontFace*, u64>* OnExpired
    )
    {
        // todo;
        return 0;
    }

    template <class Self>
    void* FontManagerBase<Self>::Impl_RemoveAssocUpdate(u64 AssocUpdateId)
    {
        // todo
        return nullptr;
    }

    template <class Self>
    void FontManagerBase<Self>::Impl_SetExpireFrame(const u64 FrameCount)
    {
        m_expire_frame = std::max<u64>(FrameCount, 4);
    }

    template <class Self>
    u64 FontManagerBase<Self>::Impl_GetCurrentFrame() const
    {
        return m_cur_frame;
    }

    template <class Self>
    void FontManagerBase<Self>::Impl_Update(const u64 CurrentTime)
    {
        std::lock_guard lock(m_mutex);
        const auto last_frame = m_cur_frame;
        m_cur_frame++;
        m_cur_time = CurrentTime;
        if (last_frame == 0) return;
        // todo
    }

    template <class Self>
    u64 FontManagerBase<Self>::Impl_FontFaceToId(IFontFace* Face)
    {
        std::lock_guard lock(m_mutex);
        // todo get or add
        const auto pair = m_face_to_id.TryGet(reinterpret_cast<usize>(Face));
        if (pair.has_value())
        {
            const auto id = *pair.value().second;
            const auto node = m_id_to_node.TryGet(id).value().second;
            MakeNewest(*node);
            return node->m_id;
        }
        else
        {
            const auto id = m_id_inc++;
            AddNode(id, Face);
            return id;
        }
    }

    template <class Self>
    IFontFace* FontManagerBase<Self>::Impl_IdToFontFace(u64 Id)
    {
        std::lock_guard lock(m_mutex);
        const auto pair = m_id_to_node.TryGet(Id);
        if (!pair.has_value()) return nullptr;
        const auto node = pair.value().second;
        MakeNewest(*node);
        return node->m_face.get();
    }
}
