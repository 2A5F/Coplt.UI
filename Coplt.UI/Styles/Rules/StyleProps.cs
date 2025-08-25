// namespace Coplt.UI.Styles.Rules;
//
// public enum StylePropertyId
// {
//     Custom = -1,
//     Unknown = 0,
//
//     Display,
//     BoxSizing,
//     Position,
//
//     OverflowX,
//     OverflowY,
//
//     Top,
//     Right,
//     Bottom,
//     Left,
//
//     Width,
//     Height,
//     MinWidth,
//     MinHeight,
//     MaxWidth,
//     MaxHeight,
//
//     AspectRatio,
//
//     MarginTop,
//     MarginRight,
//     MarginBottom,
//     MarginLeft,
//
//     PaddingTop,
//     PaddingRight,
//     PaddingBottom,
//     PaddingLeft,
//
//     BorderTop,
//     BorderRight,
//     BorderBottom,
//     BorderLeft,
//
//     AlignItems,
//     AlignSelf,
//     JustifyItems,
//     JustifySelf,
//     AlignContent,
//     JustifyContent,
//
//     FlexDirection,
//     FlexWrap,
//     FlexBias,
//     FlexGrow,
//     FlexShrink,
//
//     GapX,
//     GapY,
//
//     TextAlign,
//
//     ZIndex,
//     Opaque,
//
//     BoxShadow,
//
//     BackgroundColor,
//     BackgroundImage,
//     BackgroundImageTint,
//
//     BorderColorTop,
//     BorderColorRight,
//     BorderColorBottom,
//     BorderColorLeft,
//
//     BorderRadiusTop,
//     BorderRadiusRight,
//     BorderRadiusBottom,
//     BorderRadiusLeft,
//
//     BorderRadiusMode,
//
//     TextColor,
//     TextSize,
//
//     BackDrop,
//     Filter,
//
//     TextSelectable,
//     PointerEvents,
// }
//
// public static class StylePropertyIdEx
// {
//     public static bool IsBool(this StylePropertyId id) => id switch
//     {
//         StylePropertyId.TextSelectable => true,
//         StylePropertyId.PointerEvents => true,
//         _ => false,
//     };
//     
//     public static bool IsByte(this StylePropertyId id) => id switch
//     {
//         StylePropertyId.Display => true,
//         StylePropertyId.BoxSizing => true,
//         StylePropertyId.Position => true,
//         StylePropertyId.OverflowX => true,
//         StylePropertyId.OverflowY => true,
//         StylePropertyId.AlignItems => true,
//         StylePropertyId.AlignSelf => true,
//         StylePropertyId.JustifyItems => true,
//         StylePropertyId.JustifySelf => true,
//         StylePropertyId.AlignContent => true,
//         StylePropertyId.JustifyContent => true,
//         StylePropertyId.FlexDirection => true,
//         StylePropertyId.FlexWrap => true,
//         StylePropertyId.TextAlign => true,
//         _ => false,
//     };
//     
//     public static bool IsInt(this StylePropertyId id) => id switch
//     {
//         StylePropertyId.ZIndex => true,
//         StylePropertyId.BorderRadiusMode => true,
//         _ => false,
//     };
//     
//     public static bool IsLengthPercentageAuto(this StylePropertyId id) => id switch
//     {
//         StylePropertyId.Top => true,
//         StylePropertyId.Right => true,
//         StylePropertyId.Bottom => true,
//         StylePropertyId.Left => true,
//         StylePropertyId.MarginTop => true,
//         StylePropertyId.MarginRight => true,
//         StylePropertyId.MarginBottom => true,
//         StylePropertyId.MarginLeft => true,
//         _ => false,
//     };
//     
//     public static bool IsDimension(this StylePropertyId id) => id switch
//     {
//         StylePropertyId.Width => true,
//         StylePropertyId.Height => true,
//         StylePropertyId.MinWidth => true,
//         StylePropertyId.MinHeight => true,
//         StylePropertyId.MaxWidth => true,
//         StylePropertyId.MaxHeight => true,
//         StylePropertyId.FlexBias => true,
//         _ => false,
//     };
//     
//     public static bool IsLengthPercentage(this StylePropertyId id) => id switch
//     {
//         StylePropertyId.PaddingTop => true,
//         StylePropertyId.PaddingRight => true,
//         StylePropertyId.PaddingBottom => true,
//         StylePropertyId.PaddingLeft => true,
//         StylePropertyId.BorderTop => true,
//         StylePropertyId.BorderRight => true,
//         StylePropertyId.BorderBottom => true,
//         StylePropertyId.BorderLeft => true,
//         StylePropertyId.GapX => true,
//         StylePropertyId.GapY => true,
//         _ => false,
//     };
//     
//     public static bool IsLength(this StylePropertyId id) => id switch
//     {
//         StylePropertyId.TextSize => true,
//         _ => false,
//     };
//     
//     public static bool IsFloat(this StylePropertyId id) => id switch
//     {
//         StylePropertyId.AspectRatio => true,
//         StylePropertyId.FlexGrow => true,
//         StylePropertyId.FlexShrink => true,
//         StylePropertyId.Opaque => true,
//         StylePropertyId.BorderRadiusTop => true,
//         StylePropertyId.BorderRadiusRight => true,
//         StylePropertyId.BorderRadiusBottom => true,
//         StylePropertyId.BorderRadiusLeft => true,
//         _ => false,
//     };
//     
//     public static bool IsColor(this StylePropertyId id) => id switch
//     {
//         StylePropertyId.BackgroundColor => true,
//         StylePropertyId.BackgroundImageTint => true,
//         StylePropertyId.BorderColorTop => true,
//         StylePropertyId.BorderColorRight => true,
//         StylePropertyId.BorderColorBottom => true,
//         StylePropertyId.BorderColorLeft => true,
//         StylePropertyId.TextColor => true,
//         _ => false,
//     };
// }
