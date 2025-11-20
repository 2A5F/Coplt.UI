#pragma once

#include <utility>

#include "hb.h"
#include "hb-ot.h"
#ifdef _WINDOWS
#include "hb-directwrite.h"
#endif

#include "Com.h"

namespace Coplt::Harf
{
    template <class T, Func<T*, T*> hb_reference, Func<void, T*> hb_destroy>
    struct HObject
    {
        using HbType = T;
        using Object = HObject;

        T* m_ptr{};

        ~HObject() noexcept
        {
            hb_destroy(m_ptr);
        }

        HObject() noexcept = default;

        explicit HObject(T* ptr) noexcept
            : m_ptr(ptr)
        {
        }

        void swap(HObject& other) noexcept
        {
            std::swap(m_ptr, other.m_ptr);
        }

        HObject(const HObject& other) noexcept
            : m_ptr(hb_reference(other))
        {
        }

        HObject(HObject&& other) noexcept
            : m_ptr(std::exchange(other.m_ptr, nullptr))
        {
        }

        HObject& operator=(const HObject& other) noexcept
        {
            if (this != &other) HObject(other).swap(*this);
            return *this;
        }

        HObject& operator=(HObject&& other) noexcept
        {
            if (this != &other) HObject(std::forward<HObject>(other)).swap(*this);
            return *this;
        }

        operator T*() const noexcept
        {
            return m_ptr;
        }

        explicit operator bool() const noexcept
        {
            return m_ptr != nullptr;
        }
    };

    struct HFont;

    struct HFace : HObject<hb_face_t, hb_face_reference, hb_face_destroy>
    {
        using Object::Object;

        #ifdef _WINDOWS
        explicit HFace(IDWriteFontFace* face)
            : Object(hb_directwrite_face_create(face))
        {
        }

        IDWriteFontFace* GetDWriteFontFace() const
        {
            return hb_directwrite_face_get_dw_font_face(m_ptr);
        }

        #endif
    };

    struct HFont : HObject<hb_font_t, hb_font_reference, hb_font_destroy>
    {
        using Object::Object;

        explicit HFont(const HFace& face)
            : Object(hb_font_create(face))
        {
        }

        #ifdef _WINDOWS
        explicit HFont(IDWriteFontFace* face)
            : Object(hb_directwrite_font_create(face))
        {
        }

        explicit HFont(int hb_font);

        IDWriteFontFace* GetDWriteFontFace() const
        {
            return hb_directwrite_font_get_dw_font_face(m_ptr);
        }

        #endif

        HFont CreateSubFont() const
        {
            return HFont(hb_font_create_sub_font(m_ptr));
        }

        void SetPixelsPerEm(const u32 size) const
        {
            hb_font_set_ppem(m_ptr, size, size);
        }

        void SetPixelsPerEm(const u32 x, const u32 y) const
        {
            hb_font_set_ppem(m_ptr, x, y);
        }

        void SetVariations(const std::initializer_list<const hb_variation_t> variations) const
        {
            hb_font_set_variations(m_ptr, variations.begin(), variations.size());
        }

        void SetVariations(const std::span<const hb_variation_t> variations) const
        {
            hb_font_set_variations(m_ptr, variations.data(), variations.size());
        }

        u32 GetLigatureCarets(
            const hb_direction_t direction,
            const hb_codepoint_t glyph,
            const u32 start_offset,
            u32* caret_count,
            hb_position_t* caret_array
        ) const
        {
            return hb_ot_layout_get_ligature_carets(
                m_ptr, direction, glyph, start_offset, caret_count, caret_array
            );
        }
    };
}
