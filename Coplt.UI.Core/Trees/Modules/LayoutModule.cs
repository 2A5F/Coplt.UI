namespace Coplt.UI.Trees.Modules;

public sealed class LayoutModule(Document document) : Document.IModule
{
    public static Document.IModule Create(Document document) => new LayoutModule(document);

    public void Update() { }
}
