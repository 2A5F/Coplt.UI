#include "FontManager.h"

using namespace Coplt;

FontManager::AssocUpdate::AssocUpdate(void* Data, Func<void, void*>* OnDrop, Func<void, void*, IFontFace*, u64>* OnAdd, Func<void, void*, IFontFace*, u64>* OnExpired)
    : Data(Data), OnDrop(OnDrop), OnAdd(OnAdd), OnExpired(OnExpired)
{
}

FontManager::AssocUpdate::~AssocUpdate()
{
    if (OnDrop) OnDrop(Data);
}

FontManager::AssocUpdate::AssocUpdate(AssocUpdate&& other) noexcept
    : Data(std::exchange(other.Data, nullptr)),
      OnDrop(std::exchange(other.OnDrop, nullptr)),
      OnAdd(std::exchange(other.OnAdd, nullptr)),
      OnExpired(std::exchange(other.OnExpired, nullptr))
{
}

FontManager::AssocUpdate& FontManager::AssocUpdate::operator=(AssocUpdate&& other) noexcept
{
    if (this != &other) AssocUpdate(std::forward<AssocUpdate>(other)).swap(*this);
    return *this;
}

void FontManager::AssocUpdate::swap(AssocUpdate& other) noexcept
{
    std::swap(Data, other.Data);
    std::swap(OnDrop, other.OnDrop);
    std::swap(OnAdd, other.OnAdd);
    std::swap(OnExpired, other.OnExpired);
}

void FontManager::AssocUpdate::InvokeOnAdd(IFontFace* face, const u64 id) const
{
    if (OnAdd) OnAdd(Data, face, id);
}

void FontManager::AssocUpdate::InvokeOnExpired(IFontFace* face, const u64 id) const
{
    if (OnExpired) OnExpired(Data, face, id);
}

FontManager::Node& FontManager::AddNode(const u64 id, Rc<IFontFace> face)
{
    auto entry = m_id_to_node.GetValueRefOrUninitializedValue(id);
    COPLT_DEBUG_ASSERT(!entry);
    auto& node = entry.SetValue(
        Node{
            .m_id = id,
            .m_face = std::move(face),
            .m_last_use_frame = m_cur_frame,
            . m_last_use_time = m_cur_time,
            .m_newer = -1,
            .m_older = -1,
        }
    );
    if (m_newest >= 0)
    {
        auto& old_newest = *m_id_to_node.UnsafeAt(m_newest);
        old_newest.m_newer = entry.m_index;
        node.m_older = m_newest;
        m_newest = entry.m_index;
    }
    else
    {
        m_oldest = m_newest = entry.m_index;
    }
    return node;
}

void FontManager::MakeNewest(Node& node, const i32 index)
{
    if (node.m_last_use_frame == m_cur_frame) return;
    node.m_last_use_frame = m_cur_frame;
    node.m_last_use_time = m_cur_time;
    COPLT_DEBUG_ASSERT(m_newest >= 0);
    if (node.m_newer < 0) return;
    auto& newer = *m_id_to_node.UnsafeAt(node.m_newer);
    newer.m_older = node.m_older;
    if (node.m_older >= 0)
    {
        auto& older = *m_id_to_node.UnsafeAt(node.m_older);
        older.m_newer = node.m_newer;
    }
    auto& old_newest = *m_id_to_node.UnsafeAt(m_newest);
    old_newest.m_newer = index;
    node.m_older = m_newest;
    node.m_newer = -1;
    m_newest = index;
}

u64 FontManager::SetAssocUpdate(
    void* Data,
    Func<void, void*>* OnDrop,
    Func<void, void*, IFontFace*, u64>* OnAdd,
    Func<void, void*, IFontFace*, u64>* OnExpired
)
{
    const u64 id = m_assoc_update_id_inc++;
    m_assoc_updates.Set(id, AssocUpdate(Data, OnDrop, OnAdd, OnExpired));
    return id;
}

void FontManager::RemoveAssocUpdate(const u64 AssocUpdateId)
{
    m_assoc_updates.Remove(AssocUpdateId);
}

void FontManager::SetExpireFrame(const u64 FrameCount)
{
    m_expire_frame = std::max<u64>(FrameCount, 4);
}

void FontManager::SetExpireTime(u64 TimeTicks)
{
    m_expire_time = TimeTicks;
}

u64 FontManager::GetCurrentFrame() const
{
    return m_cur_frame;
}

void FontManager::Update(u64 CurrentTime, const std::function<void(Node* node)>& OnExpired)
{
    std::lock_guard lock(m_mutex);
    const auto last_frame = m_cur_frame;
    m_cur_frame++;
    m_cur_time = CurrentTime;
    if (last_frame == 0) return;
    while (m_oldest >= 0)
    {
        auto& node = *m_id_to_node.UnsafeAt(m_oldest);
        if (
            m_cur_frame - node.m_last_use_frame <= m_expire_frame
            || m_cur_time - node.m_last_use_time <= m_expire_time
        )
            break;
        OnExpired(&node);
        if (node.m_newer < 0) m_oldest = m_newest = -1; // all removed
        else
        {
            m_oldest = node.m_newer;
            m_id_to_node.Remove(node.m_id);
        }
    }
    if (m_oldest >= 0)
    {
        auto& node = *m_id_to_node.UnsafeAt(m_oldest);
        node.m_older = -1;
    }
}

u64 FontManager::FontFaceToId(IFontFace* Face, const std::function<void(Node* node)>& OnAdd)
{
    std::lock_guard lock(m_mutex);
    if (auto entry = m_face_to_id.GetValueRefOrUninitializedValue(reinterpret_cast<usize>(Face)))
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
        auto& node = AddNode(id, Rc<IFontFace>::UnsafeClone(Face));
        OnAdd(&node);
        return id;
    }
}

IFontFace* FontManager::IdToFontFace(const u64 Id)
{
    std::lock_guard lock(m_mutex);
    const auto entry = m_id_to_node.TryGet(Id);
    if (!entry) return nullptr;
    auto& node = entry.GetValue();
    MakeNewest(node, entry.Index());
    return node.m_face.get();
}

void FontManager::OnAdd(const Node* node) const
{
    auto enumerator = m_assoc_updates.GetEnumerator();
    while (enumerator.MoveNext())
    {
        enumerator.Current().second->InvokeOnAdd(node->m_face.get(), node->m_id);
    }
}

void FontManager::OnExpired(const Node* node) const
{
    auto enumerator = m_assoc_updates.GetEnumerator();
    while (enumerator.MoveNext())
    {
        enumerator.Current().second->InvokeOnExpired(node->m_face.get(), node->m_id);
    }
}
