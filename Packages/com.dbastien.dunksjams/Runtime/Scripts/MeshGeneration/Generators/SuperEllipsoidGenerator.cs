using System;
using UnityEngine;

/// <summary>
/// Generates a superellipsoid (rounded cube / various shapes based on n1, n2 parameters).
/// See: http://en.wikipedia.org/wiki/Superellipsoid
/// </summary>
public static class SuperEllipsoidGenerator
{
    public static Mesh Generate
    (
        float width = 0.5f, float height = 0.5f, float length = 0.5f,
        int segments = 16, float n1 = 1f, float n2 = 1f,
        NormalsType normalsType = NormalsType.Vertex,
        PivotPosition pivot = PivotPosition.Center
    )
    {
        segments = Mathf.Clamp(segments, 1, 100);
        n1 = Mathf.Clamp(n1, 0.01f, 5f);
        n2 = Mathf.Clamp(n2, 0.01f, 5f);

        // Match PrimitivesPro segment count formula
        segments = segments * 4 - 1 + 5;

        int numVerts = (segments + 1) * (segments / 2 + 1);
        int numTris = segments * (segments / 2) * 6;

        var verts = new Vector3[numVerts];
        var uvs = new Vector2[numVerts];
        var tris = new int[numTris];

        Vector3 pivotOff = MeshBuilder.PivotOffset(pivot, height * 2f);

        for (var j = 0; j <= segments / 2; ++j)
        for (var i = 0; i <= segments; ++i)
        {
            int idx = j * (segments + 1) + i;
            float theta = i * 2f * MathF.PI / segments;
            float phi = -0.5f * MathF.PI + MathF.PI * j / (segments / 2f);

            verts[idx].x = RPower(MathF.Cos(phi), n1) * RPower(MathF.Cos(theta), n2) * width;
            verts[idx].z = RPower(MathF.Cos(phi), n1) * RPower(MathF.Sin(theta), n2) * length;
            verts[idx].y = RPower(MathF.Sin(phi), n1) * height;

            uvs[idx].x = MathF.Atan2(verts[idx].z, verts[idx].x) / (2f * MathF.PI);
            if (uvs[idx].x < 0) uvs[idx].x += 1f;
            uvs[idx].y = 0.5f +
                         MathF.Atan2(verts[idx].y,
                             MathF.Sqrt(verts[idx].x * verts[idx].x + verts[idx].z * verts[idx].z)) /
                         MathF.PI;

            // Fix poles
            if (j == 0)
            {
                verts[idx] = new Vector3(0, -height, 0);
                uvs[idx] = new Vector2(0, 0);
            }
            else if (j == segments / 2)
            {
                verts[idx] = new Vector3(0, height, 0);
                uvs[idx] = new Vector2(uvs[(j - 1) * (segments + 1) + i].x, 1f);
            }

            // Fix seam
            if (i == segments)
            {
                Vector3 first = verts[j * (segments + 1)];
                verts[idx].x = first.x;
                verts[idx].z = first.z;
                uvs[idx].x = 1f;
            }

            verts[idx] += pivotOff;
        }

        // Fix bottom row UVs
        for (var i = 0; i <= segments; ++i)
            uvs[i].x = uvs[segments + 1 + i].x;

        var ti = 0;
        for (var j = 0; j < segments / 2; ++j)
        for (var i = 0; i < segments; ++i)
        {
            int i1 = j * (segments + 1) + i;
            int i2 = i1 + 1;
            int i3 = (j + 1) * (segments + 1) + i + 1;
            int i4 = (j + 1) * (segments + 1) + i;
            tris[ti] = i3;
            tris[ti + 1] = i2;
            tris[ti + 2] = i1;
            tris[ti + 3] = i4;
            tris[ti + 4] = i3;
            tris[ti + 5] = i1;
            ti += 6;
        }

        if (normalsType == NormalsType.Face)
            MeshBuilder.DuplicateForFlatShading(ref verts, ref uvs, tris);

        return MeshBuilder.Build(verts, tris, uvs);
    }

    private static float RPower(float v, float n) => v >= 0 ? MathF.Pow(v, n) : -MathF.Pow(-v, n);
}