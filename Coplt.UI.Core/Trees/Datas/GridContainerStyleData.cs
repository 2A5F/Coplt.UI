using Coplt.Dropping;
using Coplt.UI.Collections;
using Coplt.UI.Core.Styles;

namespace Coplt.UI.Trees.Datas;

[Dropping]
public partial record struct GridContainerStyleData
{
    [Drop]
    public NativeBox<GridContainerStyleInner> Inner;
}

[Dropping]
public partial record struct GridContainerStyleInner
{
    [Drop]
    public NativeList<GridTemplateComponent> GridTemplateRows;
    [Drop]
    public NativeList<GridTemplateComponent> GridTemplateColumns;
    [Drop]
    public NativeList<TrackSizingFunction> GridAutoRows;
    [Drop]
    public NativeList<TrackSizingFunction> GridAutoColumns;
    
    [Drop]
    public NativeList<GridTemplateArea> GridTemplateAreas;
    [Drop]
    public NativeList<NativeList<int>> GridTemplateColumnNames;
    [Drop]
    public NativeList<NativeList<int>> GridTemplateRowNames;
}
