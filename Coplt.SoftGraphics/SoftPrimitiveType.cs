namespace Coplt.SoftGraphics;

public enum SoftPrimitiveType
{
    /// <summary>
    /// General triangle，
    /// </summary>
    Triangle,
    /// <summary>
    /// Convex quadrilateral, if the output is not convex it is undefined behavior
    /// </summary>
    ConvexQuadrilateral,
}
