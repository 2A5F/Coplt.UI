using System.Runtime.InteropServices;
using Coplt.Com;

namespace Coplt.UI.Core.Geometry.Native;

public enum PathBuilderCmdType : uint
{
    Close,
    MoveTo,
    LineTo,
    QuadraticBezierTo,
    CubicBezierTo,
    Arc,
}

[StructLayout(LayoutKind.Explicit)]
public struct PathBuilderCmd
{
    [FieldOffset(0)]
    public PathBuilderCmdType Type;
    [FieldOffset(0)]
    public PathBuilderCmdXToPoint XTo;
    [FieldOffset(0)]
    public PathBuilderCmdQuadraticBezierTo QuadraticBezierTo;
    [FieldOffset(0)]
    public PathBuilderCmdCubicBezierTo CubicBezierTo;
    [FieldOffset(0)]
    public PathBuilderCmdArc Arc;
}

public struct PathBuilderCmdXToPoint
{
    public PathBuilderCmdType Type;
    public float X;
    public float Y;
}

public struct PathBuilderCmdQuadraticBezierTo
{
    public PathBuilderCmdType Type;
    public float CtrlX;
    public float CtrlY;
    public float ToX;
    public float ToY;
}

public struct PathBuilderCmdCubicBezierTo
{
    public PathBuilderCmdType Type;
    public float Ctrl0X;
    public float Ctrl0Y;
    public float Ctrl1X;
    public float Ctrl1Y;
    public float ToX;
    public float ToY;
}

public struct PathBuilderCmdArc
{
    public PathBuilderCmdType Type;
    public float CenterX;
    public float CenterY;
    public float RadiiX;
    public float RadiiY;
    public float SweepAngle;
    public float XRotation;
}

[Interface, Guid("ee1c5b1d-b22d-446a-9eef-128cec82e6c0")]
public unsafe partial struct IPathBuilder
{
    public partial HResult Build(IPath** path);

    /// <summary>
    /// Hints at the builder that a certain number of endpoints and control points will be added.
    /// <para>The Builder implementation may use this information to pre-allocate memory as an optimization.</para>
    /// </summary>
    public partial void Reserve(int Endpoints, int CtrlPoints);
    
    public partial void Batch([ComType<ConstPtr<PathBuilderCmd>>] PathBuilderCmd* cmds, int num_cmds);

    public partial void Close();
    public partial void MoveTo(float x, float y);
    public partial void LineTo(float x, float y);
    public partial void QuadraticBezierTo(float ctrl_x, float ctrl_y, float to_x, float to_y);
    public partial void CubicBezierTo(
        float ctrl0_x, float ctrl0_y,
        float ctrl1_x, float ctrl1_y,
        float to_x, float to_y
    );
    public partial void Arc(
        float center_x, float center_y,
        float radii_x, float radii_y,
        float sweep_angle, float x_rotation
    );
}

[Interface, Guid("dac7a459-b942-4a96-b7d6-ee5c74eca806")]
public unsafe partial struct IPath
{
    public partial void CalcAABB(AABB2DF* out_aabb);
}
