namespace Coplt.UI.Core.Geometry;

public record struct TessFillOptions()
{
    /// <summary>
    /// Maximum allowed distance to the path when building an approximation.
    /// </summary>
    public float ToLerance = 0.1f;
    /// <summary>
    /// https://www.w3.org/TR/SVG/painting.html#FillRuleProperty
    /// </summary>
    public FillRule FillRule =  FillRule.EvenOdd;
    /// <summary>
    /// Whether to perform a vertical or horizontal traversal of the geometry.
    /// </summary>
    public Orientation SweepOrientation = Orientation.Vertical;
    /// <summary>
    /// A fast path to avoid some expensive operations if the path is known to not have any self-intersections.
    /// <para>Do not set this to <c>false</c> if the path may have intersecting edges else the tessellator may panic or produce incorrect results. In doubt, do not change the default value.</para>
    /// </summary>
    public bool HandleIntersections = true;
}

public record struct TessStrokeOptions()
{
    /// <summary>
    /// Maximum allowed distance to the path when building an approximation.
    /// </summary>
    public float ToLerance = 0.1f;
    /// <summary>
    /// Line width
    /// </summary>
    public float LineWidth = 1.0f;
    /// <summary>
    /// https://svgwg.org/svg2-draft/painting.html#StrokeMiterlimitProperty
    /// </summary>
    public float MiterLimit = 4.0f;
    /// <summary>
    /// https://svgwg.org/specs/strokes/#StrokeLinecapProperty
    /// </summary>
    public LineCap StartCap = LineCap.Butt;
    /// <summary>
    /// https://svgwg.org/specs/strokes/#StrokeLinecapProperty
    /// </summary>
    public LineCap EndCap = LineCap.Butt;
    /// <summary>
    /// https://svgwg.org/specs/strokes/#StrokeLinejoinProperty
    /// </summary>
    public LineJoin LineJoin = LineJoin.Miter;
}
