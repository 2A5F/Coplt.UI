namespace Coplt.UI;

[AttributeUsage(AttributeTargets.Class)]
public sealed class WidgetAttribute : Attribute;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ParamAttribute : Attribute;
