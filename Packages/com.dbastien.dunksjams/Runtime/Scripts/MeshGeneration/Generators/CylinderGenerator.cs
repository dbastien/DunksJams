using UnityEngine;

/// <summary>
/// Convenience wrapper over ConeGenerator with equal radii.
/// </summary>
public static class CylinderGenerator
{
    public static Mesh Generate(
        float radius = 0.5f, float height = 1f,
        int sides = 16, int heightSegments = 1,
        NormalsType normalsType = NormalsType.Vertex,
        PivotPosition pivot = PivotPosition.Bottom) =>
        ConeGenerator.Generate(radius, radius, height, sides, heightSegments, normalsType, pivot);
}
