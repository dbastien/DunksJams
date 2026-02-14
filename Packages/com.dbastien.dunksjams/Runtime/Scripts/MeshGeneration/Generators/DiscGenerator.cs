using System;
using UnityEngine;

/// <summary>Generates a flat disc (circle / ellipse) mesh lying on the XZ plane.</summary>
public static class DiscGenerator
{
    public static Mesh Generate(
        float radiusX = 0.5f, float radiusZ = 0.5f,
        int segments = 32)
    {
        segments = Mathf.Max(segments, 3);

        // Center vertex + ring vertices
        int vertCount = segments + 1;
        var verts = new Vector3[vertCount];
        var normals = new Vector3[vertCount];
        var uvs = new Vector2[vertCount];
        var tris = new int[segments * 3];

        verts[0] = Vector3.zero;
        normals[0] = Vector3.up;
        uvs[0] = new Vector2(0.5f, 0.5f);

        for (var i = 0; i < segments; ++i)
        {
            float a = 2f * MathF.PI * i / segments;
            float x = MathF.Cos(a);
            float z = MathF.Sin(a);

            verts[i + 1] = new Vector3(x * radiusX, 0f, z * radiusZ);
            normals[i + 1] = Vector3.up;
            uvs[i + 1] = new Vector2(x * 0.5f + 0.5f, z * 0.5f + 0.5f);

            tris[i * 3]     = 0;
            tris[i * 3 + 1] = i + 1;
            tris[i * 3 + 2] = i < segments - 1 ? i + 2 : 1;
        }

        return MeshBuilder.Build(verts, tris, uvs, normals);
    }
}
