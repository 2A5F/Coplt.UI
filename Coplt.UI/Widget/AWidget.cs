namespace Coplt.UI.Widgets;

public abstract class AWidget
{
    public virtual void Setup() {}
}

public interface IWidget<Self> where Self : AWidget<Self>, IWidget<Self>, new()
{
    public static abstract void ElementTemplate();

    public static abstract void StyleTemplate();
}

public abstract class AWidget<Self> : AWidget where Self : AWidget<Self>, IWidget<Self>, new()
{
    #region ElementBuilder

    // ReSharper disable once StaticMemberInGenericType
    internal static ElementBuilder? s_current_element_builder;
    internal static ElementBuilder GetElementBuilder() => s_current_element_builder ?? throw new InvalidOperationException();

    public static View<Self> View(
        string? Name = null,
        ReadOnlySpan<string> Tags = default
    ) => GetElementBuilder().CreateView<Self>();

    public static View<Self, T> Of<T>(
        string? Name = null,
        ReadOnlySpan<string> Tags = default
    ) where T : AWidget => GetElementBuilder().CreateView<Self, T>();

    public static Scope<Self> If(
        Func<Self, bool> Condition
    ) => GetElementBuilder().CreateIf<Self>();

    public static Scope<Self> ElseIf(
        Func<Self, bool> Condition
    ) => GetElementBuilder().CreateElseIf<Self>();

    public static Scope<Self> Else() => GetElementBuilder().CreateElse<Self>();

    public static Scope<Self> For<T>(
        out LoopContext<T> Context, Func<Self, IEnumerable<T>> Source
    ) => GetElementBuilder().CreateIf<Self>();

    public static Scope<Self> For<T>(
        out LoopContext<T> Context, Func<Self, T> Init, ForCond<Self, T> Cond, ForInc<Self, T> Inc
    ) => GetElementBuilder().CreateIf<Self>();

    public static Scope<Self> Empty() => GetElementBuilder().CreateEmpty<Self>();

    #endregion
}

public delegate bool ForCond<in Self, T>(Self self, ref T value);
public delegate void ForInc<in Self, T>(Self self, ref T value);
