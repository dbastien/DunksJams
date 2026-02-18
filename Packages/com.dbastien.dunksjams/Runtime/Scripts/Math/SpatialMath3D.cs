using UnityEngine;

public static class SpatialMath3D
{
    public static Vector3 AddVectorLength(Vector3 vec, float size)
    {
        float mag = vec.magnitude;
        return mag > 0 ? vec * ((mag + size) / mag) : Vector3.zero;
    }

    public static Vector3 SetVectorLength(Vector3 vec, float size)
    {
        float mag = vec.magnitude;
        return mag > 0 ? vec * (size / mag) : Vector3.zero;
    }

    public static bool PlanePlaneIntersection
    (
        out Vector3 lineP, out Vector3 lineVec, Vector3 plane1Normal,
        Vector3 plane1Pos, Vector3 plane2Normal, Vector3 plane2Pos
    )
    {
        lineVec = Vector3.Cross(plane1Normal, plane2Normal);
        float denom = lineVec.sqrMagnitude;
        if (denom < 1e-6f) // Parallel
        {
            lineP = Vector3.zero;
            return false;
        }

        lineP = plane2Pos + Vector3.Cross(plane1Pos - plane2Pos, lineVec) * (1 / denom);
        return true;
    }

    public static bool LinePlaneIntersection
    (
        out Vector3 intersection, Vector3 lineP, Vector3 lineVec,
        Vector3 planeNormal, Vector3 planeP
    )
    {
        intersection = Vector3.zero;
        float dot = Vector3.Dot(lineVec, planeNormal);
        if (Mathf.Approximately(dot, 0f)) return false;

        float dist = Vector3.Dot(planeP - lineP, planeNormal) / dot;
        intersection = lineP + lineVec * dist;
        return true;
    }

    public static bool ClosestPointsOnTwoLines
    (
        out Vector3 closestPLine1, out Vector3 closestPLine2, Vector3 lineP1,
        Vector3 lineVec1, Vector3 lineP2, Vector3 lineVec2
    )
    {
        closestPLine1 = closestPLine2 = Vector3.zero;
        if (!GetClosestPoints(out float s, out float t, lineP1, lineVec1, lineP2, lineVec2)) return false;

        closestPLine1 = lineP1 + lineVec1 * s;
        closestPLine2 = lineP2 + lineVec2 * t;
        return true;
    }

    public static Vector3 ClosestPointOnLineSegment
    (
        Vector3 p, Vector3 lineStart, Vector3 lineEnd,
        bool clampToSegment = true
    )
    {
        Vector3 lineDir = lineEnd - lineStart;
        float t = Vector3.Dot(p - lineStart, lineDir) / lineDir.sqrMagnitude;
        if (clampToSegment) t = Mathf.Clamp01(t);
        return lineStart + lineDir * t;
    }

    public static float SignedDistancePlanePoint(Vector3 planeNormal, Vector3 planeP, Vector3 p) =>
        Vector3.Dot(planeNormal, p - planeP);

    public static int PointOnWhichSideOfLineSegment(Vector3 lineP1, Vector3 lineP2, Vector3 p)
    {
        Vector3 lineVec = lineP2 - lineP1;
        Vector3 pointVec = p - lineP1;

        if (Vector3.Dot(pointVec, lineVec) <= 0) return 1; // Point is before lineP1
        return pointVec.sqrMagnitude <= lineVec.sqrMagnitude ? 0 : 2; // 0 if on segment, 2 if past lineP2
    }

    public static bool AreLineSegmentsCrossing(Vector3 pA1, Vector3 pA2, Vector3 pB1, Vector3 pB2)
    {
        if (!GetClosestPoints(out float s, out float t, pA1, (pA2 - pA1).normalized, pB1, (pB2 - pB1).normalized))
            return false;

        return s is >= 0 and <= 1 && t is >= 0 and <= 1;
    }

    public static float MouseDistanceToLine(Vector3 lineP1, Vector3 lineP2)
    {
        Vector3 screenP1 = Camera.main.WorldToScreenPoint(lineP1);
        Vector3 screenP2 = Camera.main.WorldToScreenPoint(lineP2);
        Vector3 mousePos = Input.mousePosition;

        Vector3 closestP = ClosestPointOnLineSegment(mousePos, screenP1, screenP2);
        return (closestP - mousePos).magnitude;
    }

    private static bool GetClosestPoints(out float s, out float t, Vector3 p1, Vector3 d1, Vector3 p2, Vector3 d2)
    {
        float a = Vector3.Dot(d1, d1);
        float b = Vector3.Dot(d1, d2);
        float c = Vector3.Dot(d2, d2);
        float denom = a * c - b * b;

        if (Mathf.Approximately(denom, 0f)) // Parallel
        {
            s = t = 0;
            return false;
        }

        Vector3 r = p1 - p2;
        float e = Vector3.Dot(d1, r);
        float f = Vector3.Dot(d2, r);
        s = (b * f - c * e) / denom;
        t = (a * f - b * e) / denom;
        return true;
    }
}