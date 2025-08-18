namespace Coplt.UI.Widgets;

internal class ElementBuilder
{
    public View<Self> CreateView<Self>() => throw new NotImplementedException();

    public View<Self, T> CreateView<Self, T>() where T : AWidget => throw new NotImplementedException();

    public Scope<Self> CreateIf<Self>() => throw new NotImplementedException();

    public Scope<Self> CreateElseIf<Self>() => throw new NotImplementedException();

    public Scope<Self> CreateElse<Self>() => throw new NotImplementedException();

    public Scope<Self> CreateEmpty<Self>() => throw new NotImplementedException();
}
