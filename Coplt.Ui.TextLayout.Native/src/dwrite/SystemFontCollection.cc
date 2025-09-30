#include "SystemFontCollection.h"

#include "Backend.h"
#include "CoCom.Types.h"
#include "Error.h"

using namespace Coplt;

SystemFontCollection::SystemFontCollection(
    Rc<IDWriteFactory> dw_factory,
    Rc<IDWriteFontCollection>& collection,
    std::vector<Rc<FontFamily>>& families,
    std::vector<IFontFamily*>& p_families
) :
    m_dw_factory(std::move(dw_factory)),
    m_collection(std::move(collection)),
    m_families(std::move(families)),
    m_p_families(std::move(p_families))
{
}

Rc<SystemFontCollection> SystemFontCollection::Create(const Backend* backend)
{
    Rc<IDWriteFontCollection> collection{};
    if (const auto hr = backend->m_dw_factory->GetSystemFontCollection(collection.put()); FAILED(hr))
        throw ComException(hr, "Failed to get system font collection");
    const auto len = collection->GetFontFamilyCount();
    std::vector<Rc<FontFamily>> families;
    std::vector<IFontFamily*> p_families;
    families.reserve(len);
    p_families.reserve(len);
    for (u32 i = 0; i < len; ++i)
    {
        Rc<IDWriteFontFamily> family;
        if (const auto hr = collection->GetFontFamily(i, family.put()); FAILED(hr))
            throw ComException(hr, "Failed to get font family");
        Rc<FontFamily> obj = FontFamily::Create(family);
        p_families.push_back(obj.get());
        families.push_back(std::move(obj));
    }
    return Rc(new SystemFontCollection(
        Rc(backend->m_dw_factory), collection, families, p_families
    ));
}

IFontFamily* const* SystemFontCollection::Impl_GetFamilies(u32* count) const
{
    *count = m_p_families.size();
    return m_p_families.data();
}

void SystemFontCollection::Impl_ClearNativeFamiliesCache()
{
    m_p_families = {};
}

u32 SystemFontCollection::Impl_FindDefaultFamily()
{
    NONCLIENTMETRICS ncm = {};
    ncm.cbSize = sizeof(NONCLIENTMETRICS);

    if (!SystemParametersInfoW(
        SPI_GETNONCLIENTMETRICS,
        sizeof(NONCLIENTMETRICS),
        &ncm,
        0))
        throw std::runtime_error("Failed to get non-client metrics");

    const auto face_name = ncm.lfMessageFont.lfFaceName;
    u32 index;
    BOOL found;
    if (const auto hr = m_collection->FindFamilyName(face_name, &index, &found); FAILED(hr))
        throw ComException(hr, "Failed to find family name");

    return found ? index : 0;
}
