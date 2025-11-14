#pragma once

#include <functional>
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
            i32 m_newer{-1};
            i32 m_older{-1};
        };

        u64 m_expire_frame{180};
        u64 m_expire_time{30000000};
        u64 m_id_inc{1};
        Map<u64, Node> m_id_to_node{};
        Map<usize, u64> m_face_to_id{};
        std::mutex m_mutex{};
        u64 m_cur_frame{};
        u64 m_cur_time{};
        i32 m_newest{-1};
        i32 m_oldest{-1};

        struct AssocUpdate final
        {
            void* Data{};
            Func<void, void*>* OnDrop{};
            Func<void, void*, IFontFace*, u64>* OnAdd{};
            Func<void, void*, IFontFace*, u64>* OnExpired{};

            AssocUpdate() = default;
            explicit AssocUpdate(void* Data, Func<void, void*>* OnDrop, Func<void, void*, IFontFace*, u64>* OnAdd, Func<void, void*, IFontFace*, u64>* OnExpired);

            ~AssocUpdate();

            AssocUpdate(const AssocUpdate&) = delete;
            AssocUpdate& operator=(const AssocUpdate&) = delete;

            AssocUpdate(AssocUpdate&& other) noexcept;
            AssocUpdate& operator=(AssocUpdate&& other) noexcept;

            void swap(AssocUpdate& other) noexcept;

            void InvokeOnAdd(IFontFace* face, u64 id) const;
            void InvokeOnExpired(IFontFace* face, u64 id) const;
        };

        Map<u64, AssocUpdate> m_assoc_updates{};
        u64 m_assoc_update_id_inc{1};

        Node& AddNode(u64 id, Rc<IFontFace> face);
        void MakeNewest(Node& node, i32 index);

        u64 SetAssocUpdate(
            void* Data,
            Func<void, void*>* OnDrop,
            Func<void, void*, IFontFace*, u64>* OnAdd,
            Func<void, void*, IFontFace*, u64>* OnExpired
        );
        void RemoveAssocUpdate(u64 AssocUpdateId);
        void SetExpireFrame(u64 FrameCount);
        void SetExpireTime(u64 TimeTicks);
        u64 GetCurrentFrame() const;
        void Update(u64 CurrentTime, const std::function<void(Node* node)>& OnExpired);
        u64 FontFaceToId(IFontFace* Face, const std::function<void(Node* node)>& OnAdd);
        IFontFace* IdToFontFace(u64 Id);

        void OnAdd(const Node* node) const;
        void OnExpired(const Node* node) const;
    };

    template <class Self>
    struct FontManagerBase : ComImpl<Self, IFontManager>, FontManager
    {
        COPLT_IMPL_START

        COPLT_FORCE_INLINE
        u64 Impl_SetAssocUpdate(void* Data, Func<void, void*>* OnDrop, Func<void, void*, IFontFace*, u64>* OnAdd, Func<void, void*, IFontFace*, u64>* OnExpired);

        COPLT_FORCE_INLINE
        void Impl_RemoveAssocUpdate(u64 AssocUpdateId);

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
        Func<void, void*>* OnDrop,
        Func<void, void*, IFontFace*, u64>* OnAdd,
        Func<void, void*, IFontFace*, u64>* OnExpired
    )
    {
        return SetAssocUpdate(Data, OnDrop, OnAdd, OnExpired);
    }

    template <class Self>
    void FontManagerBase<Self>::Impl_RemoveAssocUpdate(const u64 AssocUpdateId)
    {
        return RemoveAssocUpdate(AssocUpdateId);
    }

    template <class Self>
    void FontManagerBase<Self>::Impl_SetExpireFrame(const u64 FrameCount)
    {
        SetExpireFrame(FrameCount);
    }

    template <class Self>
    void FontManagerBase<Self>::Impl_SetExpireTime(const u64 TimeTicks)
    {
        SetExpireTime(TimeTicks);
    }

    template <class Self>
    u64 FontManagerBase<Self>::Impl_GetCurrentFrame() const
    {
        return GetCurrentFrame();
    }

    template <class Self>
    void FontManagerBase<Self>::Impl_Update(const u64 CurrentTime)
    {
        Update(CurrentTime, [self = static_cast<Self*>(this)](auto node) { self->OnExpired(node); });
    }

    template <class Self>
    u64 FontManagerBase<Self>::Impl_FontFaceToId(IFontFace* Face)
    {
        return FontFaceToId(Face, [self = static_cast<Self*>(this)](auto node) { self->OnAdd(node); });
    }

    template <class Self>
    IFontFace* FontManagerBase<Self>::Impl_IdToFontFace(const u64 Id)
    {
        return IdToFontFace(Id);
    }
}
