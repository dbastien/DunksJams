using UnityEngine;

public static class GizmoUtils
{
    const float ArrowAngleDegrees = 55.0f;
    const float ArrowLength = .05f;

    public static void DrawArrow(Vector3 from, Vector3 direction)
    {
        Gizmos.DrawRay(from, direction);

        var left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - ArrowAngleDegrees, 0) *
                   Vector3.forward;
        var right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + ArrowAngleDegrees, 0) *
                    Vector3.forward;

        Gizmos.DrawRay(from + direction, left * ArrowLength);
        Gizmos.DrawRay(from + direction, right * ArrowLength);
    }
}