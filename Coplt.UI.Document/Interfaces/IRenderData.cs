namespace Coplt.UI.Document.Interfaces;

public interface IRenderData : IDisposable
{
    public void OnRemoveFromTree();
}

public struct NonRd : IRenderData
{
    public void Dispose() { }
    
    public void OnRemoveFromTree() { }
}
