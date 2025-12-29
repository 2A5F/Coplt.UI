using Coplt.UI.Collections;
using Coplt.UI.Native;
using Coplt.UI.Trees.Datas;

namespace Coplt.UI.Trees.Modules;

public sealed unsafe class LayoutModule : Document.IModule
{
    public static Document.IModule Create(Document document) => new LayoutModule();

    public void Update(Document document)
    {
        ref var layout = ref NativeLib.Instance.m_layout;
        fixed (NativeMap<NodeId, RootData>* p_roots = &document.m_roots)
        {
            var ctx = new NLayoutContext
            {
                CurrentFrame = document.CurrentFrame,
                font_manager = document.m_font_manager.m_inner,
                default_locale = document.DefaultLocale,

                roots = p_roots,

                view_count = document.ViewArche().GetRawCount(),
                view_buckets = document.ViewArche().GetBuckets(),
                view_ctrl = document.ViewArche().GetCtrls(),
                view_common_data = document.ViewStorageOf<CommonData>().AsPinned().GetDataPtr(),
                view_layout_data = document.ViewStorageOf<LayoutData>().AsPinned().GetDataPtr(),
                view_childs_data = document.ViewStorageOf<ChildsData>().AsPinned().GetDataPtr(),
                view_style_data = document.ViewStorageOf<StyleData>().AsPinned().GetDataPtr(),

                text_paragraph_count = document.TextParagraphArche().GetRawCount(),
                text_paragraph_buckets = document.TextParagraphArche().GetBuckets(),
                text_paragraph_ctrl = document.TextParagraphArche().GetCtrls(),
                text_paragraph_common_data = document.TextParagraphStorageOf<CommonData>().AsPinned().GetDataPtr(),
                text_paragraph_childs_data = document.TextParagraphStorageOf<ChildsData>().AsPinned().GetDataPtr(),
                text_paragraph_data = document.TextParagraphStorageOf<TextParagraphData>().AsPinned().GetDataPtr(),
                text_paragraph_style_data = document.TextParagraphStorageOf<TextStyleData>().AsPinned().GetDataPtr(),

                text_span_count = document.TextSpanArche().GetRawCount(),
                text_span_buckets = document.TextSpanArche().GetBuckets(),
                text_span_ctrl = document.TextSpanArche().GetCtrls(),
                text_span_common_data = document.TextSpanStorageOf<CommonData>().AsPinned().GetDataPtr(),
                text_span_data = document.TextSpanStorageOf<TextSpanData>().AsPinned().GetDataPtr(),
                text_span_style_data = document.TextSpanStorageOf<TextStyleData>().AsPinned().GetDataPtr(),

                rounding = true,
            };
            layout.Calc(&ctx).TryThrowWithMsg();
        }
    }
}
