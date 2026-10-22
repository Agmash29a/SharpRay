using System.Numerics;

namespace SharpRay.Core;

/// <summary>
/// Viewport-agnostic math shared by editor move gizmos and xUnit regressions.
/// Signed motion is measured in world units along a projected unit axis.
/// </summary>
public static class GizmoMath
{
    public static float AxisDisplacement(Vector2 axisStart, Vector2 axisEnd,
        Vector2 mouseDown, Vector2 mouseNow)
    {
        Vector2 axis = axisEnd - axisStart;
        float squared = axis.LengthSquared();
        if (squared < 1e-5f) return 0;
        return Vector2.Dot(mouseNow - mouseDown, axis) / squared;
    }

    public static float DistanceToAxis(Vector2 mouse, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float squared = ab.LengthSquared();
        if (squared < 1e-5f) return (mouse - a).Length();
        float fraction = Math.Clamp(Vector2.Dot(mouse - a, ab) / squared, 0f, 1f);
        return (mouse - (a + fraction * ab)).Length();
    }
}
