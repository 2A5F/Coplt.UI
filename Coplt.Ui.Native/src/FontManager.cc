#include "FontManager.h"

using namespace Coplt;

void FontManager::AddNode(const u64 id, IFontFace* face)
{
    m_id_to_node.TryAdd(
        id, Node{
            .m_id = id,
            .m_face = Rc<IFontFace>::UnsafeClone(face),
            .m_last_use_frame = m_cur_frame,
            . m_last_use_time = m_cur_time,
            .m_newer = -1, // todo
            .m_older = -1, // todo
        }
    );
}

void FontManager::MakeNewest(Node& node)
{
    if (node.m_last_use_frame == m_cur_frame) return;
    node.m_last_use_frame = m_cur_frame;
    node.m_last_use_time = m_cur_time;
    // todo
}
