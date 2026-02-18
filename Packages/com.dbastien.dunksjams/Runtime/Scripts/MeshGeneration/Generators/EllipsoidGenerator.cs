using System;
using UnityEngine;

/// <summary>Generates an ellipsoid mesh with independent width, height, and depth radii.</summary>
public static class EllipsoidGenerator
{
    public static Mesh Generate
    (
        float width = 0.5f, float height = 0.5f, float depth = 0.5f,
        int segments = 16,
        NormalsType normalsType = NormalsType.Vertex,
        PivotPosition pivot = PivotPosition.Center
    )
    {
        segments = Mathf.Max(segments, 4);
        int rings = segments - 1;
        int sectors = segments;

        float R = 1f / (rings - 1);
        float S = 1f / (sectors - 1);

        var verts = new Vector3[rings * sectors];
        var normals = new Vector3[verts.Length];
        var uvs = new Vector2[verts.Length];
        var tris = new int[(rings - 1) * (sectors - 1) * 6];

        int vi = 0, ti = 0;
        for (var r = 0; r < rings; ++r)
        {
            float y = MathF.Cos(-MathF.PI * 2f + MathF.PI * r * R);
            float sinR = MathF.Sin(MathF.PI * r * R);

            for (var s = 0; s < sectors; ++s)
            {
                float x = MathF.Sin(2f * MathF.PI * s * S) * sinR;
                float z = MathF.Cos(2f * MathF.PI * s * S) * sinR;

                verts[vi] = new Vector3(x * width, y * height, z * depth);
                normals[vi] = new Vector3(x, y, z).normalized;
                uvs[vi] = new Vector2(1f - s * S, 1f - r * R);

                if (r < rings - 1 && s < sectors - 1)
                {
                    tris[ti] = (r + 1) * sectors + s;
                    tris[ti + 1] = r * sectors + s + 1;
                    tris[ti + 2] = r * sectors + s;
                    tris[ti + 3] = (r + 1) * sectors + s + 1;
                    tris[ti + 4] = r * sectors + s + 1;
                    tris[ti + 5] = (r + 1) * sectors + s;
                    ti += 6;
                }

                vi++;
            }
        }

        if (normalsType == NormalsType.Face)
            MeshBuilder.DuplicateForFlatShading(ref verts, ref uvs, tris);

        Vector3 pivotOff = MeshBuilder.PivotOffset(pivot, height * 2f);
        if (pivotOff != Vector3.zero) MeshBuilder.ApplyPivot(verts, pivotOff);

        return normalsType == NormalsType.Vertex
            ? MeshBuilder.Build(verts, tris, uvs, normals)
            : MeshBuilder.Build(verts, tris, uvs);
    }
}