using UnityEngine;

/// <summary>
/// A point with position and rotation, useful for path following, mesh extrusion,
/// and placing objects along splines.
/// Extracted from LandscapeBuilder's LBOrientedPoint.
/// </summary>
public struct OrientedPoint
{
    public Vector3 Position;
    public Quaternion Rotation;

    public OrientedPoint(Vector3 position, Quaternion rotation)
    {
        Position = position;
        Rotation = rotation;
    }

    /// <summary>Transform a local-space point to world space.</summary>
    public Vector3 LocalToWorld(Vector3 point) => Position + Rotation * point;

    /// <summary>Transform a world-space point to local space.</summary>
    public Vector3 WorldToLocal(Vector3 point) => Quaternion.Inverse(Rotation) * (point - Position);

    /// <summary>Transform a local-space direction to world space.</summary>
    public Vector3 LocalToWorldDirection(Vector3 direction) => Rotation * direction;

    /// <summary>Transform a world-space direction to local space.</summary>
    public Vector3 WorldToLocalDirection(Vector3 direction) => Quaternion.Inverse(Rotation) * direction;

    public override string ToString() => $"OrientedPoint(pos={Position}, rot={Rotation.eulerAngles})";
}