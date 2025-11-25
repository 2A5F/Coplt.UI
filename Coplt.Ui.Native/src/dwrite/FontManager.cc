#include "FontManager.h"

#include "FontFace.h"

using namespace Coplt;

u64 DWriteFontManager::DwriteFontFaceToId(IDWriteFontFace5* Face)
{
    if (Face == nullptr) throw NullPointerError();
    std::lock_guard lock(m_mutex);
    if (auto entry = m_dwrite_face_to_id.GetValueRefOrUninitializedValue(reinterpret_cast<usize>(Face)))
    {
        const u64 id = entry.GetValue();
        auto& node = m_id_to_node.TryGet(id).GetValue();
        MakeNewest(node, entry.m_index);
        return node.m_id;
    }
    else
    {
        const auto id = m_id_inc++;
        entry.SetValue(id);
        Rc face(new DWriteFontFace(Rc<IDWriteFontFace5>::UnsafeClone(Face), id));
        m_face_to_id.Set(reinterpret_cast<usize>(face.get()), id);
        auto& node = AddNode(id, std::move(face));
        OnAdd(&node);
        return id;
    }
}

Rc<DWriteFontFace> DWriteFontManager::DwriteFontFaceToFontFace(IDWriteFontFace5* Face)
{
    if (Face == nullptr) throw NullPointerError();
    std::lock_guard lock(m_mutex);
    if (auto entry = m_dwrite_face_to_id.GetValueRefOrUninitializedValue(reinterpret_cast<usize>(Face)))
    {
        const u64 id = entry.GetValue();
        auto& node = m_id_to_node.TryGet(id).GetValue();
        MakeNewest(node, entry.m_index);
        return node.m_face.StaticCast<DWriteFontFace>();
    }
    else
    {
        const auto id = m_id_inc++;
        entry.SetValue(id);
        Rc face(new DWriteFontFace(Rc<IDWriteFontFace5>::UnsafeClone(Face), id));
        m_face_to_id.Set(reinterpret_cast<usize>(face.get()), id);
        auto& node = AddNode(id, face);
        OnAdd(&node);
        return face;
    }
}

void DWriteFontManager::OnExpired(Node* node)
{
    if (node == nullptr) throw NullPointerError();
    const auto face = static_cast<DWriteFontFace*>(node->m_face.get());
    const auto dwrite_face = reinterpret_cast<usize>(static_cast<IDWriteFontFace*>(face->m_face.get()));
    m_dwrite_face_to_id.Remove(dwrite_face);
    this->FontManager::OnExpired(node);
}

void DWriteFontManager::OnAdd(Node* node)
{
    if (node == nullptr) throw NullPointerError();
    const auto face = static_cast<DWriteFontFace*>(node->m_face.get());
    const auto dwrite_face = reinterpret_cast<usize>(static_cast<IDWriteFontFace*>(face->m_face.get()));
    m_dwrite_face_to_id.TryAdd(dwrite_face, node->m_id);
    this->FontManager::OnAdd(node);
}
