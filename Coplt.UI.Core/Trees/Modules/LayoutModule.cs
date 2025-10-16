using System.Diagnostics;
using System.Runtime.CompilerServices;
using Coplt.UI.Trees.Datas;

namespace Coplt.UI.Trees.Modules;

public sealed class LayoutModule : Document.IModule
{
    public static Document.IModule Create(Document document) => new LayoutModule(document);

    private readonly Document m_document;
    private readonly InlineArray2<Document.PinnedStorage<CommonStyleData>> m_st_CommonStyleData;
    private readonly Document.PinnedStorage<ChildsData> m_st_ChildsData;
    private readonly Document.PinnedStorage<ViewStyleData> m_st_ViewStyleData;
    private readonly Document.PinnedStorage<ViewLayoutData> m_st_ViewLayoutData;
    private readonly Document.PinnedStorage<TextStyleData> m_st_TextStyleData;

    public LayoutModule(Document document)
    {
        m_document = document;
        Debug.Assert(document.Arches.Length == 2);
        for (var i = 0; i < document.Arches.Length; i++)
        {
            var arch = document.Arches[i];
            m_st_CommonStyleData[i] = arch.UnsafeStorageAt<CommonStyleData>().AsPinned();
        }
        m_st_ChildsData = document.ArcheAt(NodeType.View).UnsafeStorageAt<ChildsData>().AsPinned();
        m_st_ViewStyleData = document.ArcheAt(NodeType.View).UnsafeStorageAt<ViewStyleData>().AsPinned();
        m_st_ViewLayoutData = document.ArcheAt(NodeType.View).UnsafeStorageAt<ViewLayoutData>().AsPinned();
        m_st_TextStyleData = document.ArcheAt(NodeType.Text).UnsafeStorageAt<TextStyleData>().AsPinned();
    }

    public void Update()
    {
        // todo
    }
}
