using System;
using UnityEngine;

public static class SphereGenerator
{
    public static Mesh Generate
    (
        float radius = 0.5f, int segments = 16,
        NormalsType normalsType = NormalsType.Vertex,
        PivotPosition pivot = PivotPosition.Center
    )
    {
        segments = Mathf.Max(segments, 4);
        int rings = segments - 1;
        int sectors = segments;

        float R = 1f / (rings - 1);
        float S = 1f / (sectors - 1);

        int vertCount = (rings + 1) * (sectors + 1);
        var verts = new Vector3[vertCount];
        var normals = new Vector3[vertCount];
        var uvs = new Vector2[vertCount];
        int triCount = (rings - 1) * sectors * 6;
        var tris = new int[triCount];

        var vi = 0;
        for (var r = 0; r < rings; ++r)
        {
            float y = -MathF.Cos(-MathF.PI * 2f + MathF.PI * r * R);
            float sinR = MathF.Sin(MathF.PI * r * R);

            for (var s = 0; s < sectors; ++s)
            {
                float x = MathF.Sin(2f * MathF.PI * s * S) * sinR;
                float z = MathF.Cos(2f * MathF.PI * s * S) * sinR;

                verts[vi] = new Vector3(x, y, z) * radius;
                normals[vi] = new Vector3(x, y, z);
                uvs[vi] = new Vector2(1f - s * S, r * R);
                vi++;
            }
        }

        var ti = 0;
        for (var r = 0; r < rings - 1; ++r)
        for (var s = 0; s < sectors - 1; ++s)
        {
            tris[ti + 5] = (r + 1) * sectors + s;
            tris[ti + 4] = r * sectors + s + 1;
            tris[ti + 3] = r * sectors + s;
            tris[ti + 2] = (r + 1) * sectors + s + 1;
            tris[ti + 1] = r * sectors + s + 1;
            tris[ti] = (r + 1) * sectors + s;
            ti += 6;
        }

        if (normalsType == NormalsType.Face)
            MeshBuilder.DuplicateForFlatShading(ref verts, ref uvs, tris);

        Vector3 pivotOff = MeshBuilder.PivotOffset(pivot, radius * 2f);
        if (pivotOff != Vector3.zero) MeshBuilder.ApplyPivot(verts, pivotOff);

        return normalsType == NormalsType.Vertex
            ? MeshBuilder.Build(verts, tris, uvs, normals)
            : MeshBuilder.Build(verts, tris, uvs);
    }
}