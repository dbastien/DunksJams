using UnityEngine;

/// <summary>Generates a flat equilateral-triangle mesh on the XZ plane (or custom vertices).</summary>
public static class TriangleGenerator
{
    /// <summary>Create an equilateral triangle with a given edge length centered at the origin on XZ.</summary>
    public static Mesh Generate(float size = 1f) =>
        Generate(
            new Vector3(-size * 0.5f, 0f, -size * 0.2886751f),
            new Vector3(size * 0.5f, 0f, -size * 0.2886751f),
            new Vector3(0f, 0f, size * 0.5773503f));

    /// <summary>Create a triangle from arbitrary vertices (single-sided, facing up / CCW winding).</summary>
    public static Mesh Generate(Vector3 a, Vector3 b, Vector3 c)
    {
        var normal = Vector3.Cross(b - a, c - a).normalized;

        var verts = new[] { a, b, c };
        var normals = new[] { normal, normal, normal };
        var uvs = new[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 1f) };
        var tris = new[] { 0, 1, 2 };

        return MeshBuilder.Build(verts, tris, uvs, normals);
    }
}
