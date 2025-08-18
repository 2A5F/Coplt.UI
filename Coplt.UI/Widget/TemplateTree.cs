namespace Coplt.UI.Widgets;

internal abstract class TemplateTree
{
    public static TemplateTree<W> Get<W>() where W : AWidget<W>, IWidget<W>, new()
        => TemplateTree<W>.s_instance ??= Build<W>();

    private static TemplateTree<W> Build<W>() where W : AWidget<W>, IWidget<W>, new()
    {
        return null!; // todo
    }
}

internal class TemplateTree<W> : TemplateTree
    where W : AWidget<W>, IWidget<W>, new()
{
    public static TemplateTree<W>? s_instance;
}
