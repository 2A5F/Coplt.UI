#pragma once

#include "../FontManager.h"

namespace Coplt
{
    struct DWriteFontFace;

    struct DWriteFontManager final : FontManagerBase<DWriteFontManager>
    {
        Map<usize, u64> m_dwrite_face_to_id{};

        u64 DwriteFontFaceToId(IDWriteFontFace5* Face);
        Rc<DWriteFontFace> DwriteFontFaceToFontFace(IDWriteFontFace5* Face);

        void OnExpired(Node* node);
        void OnAdd(Node* node);
    };
}
