using System;
using UnityEngine;

public static class HelixGenerator
{
    public static Mesh Generate
    (
        float radius = 0.5f, float height = 2f, float tubeRadius = 0.1f,
        int turns = 3, int segments = 64, int tubeSegments = 8,
        PivotPosition pivot = PivotPosition.Bottom
    )
    {
        segments = Mathf.Max(segments, 4);
        tubeSegments = Mathf.Max(tubeSegments, 3);
        turns = Mathf.Max(turns, 1);

        int numVerts = (segments + 1) * (tubeSegments + 1);
        int numTris = segments * tubeSegments * 6;

        var verts = new Vector3[numVerts];
        var normals = new Vector3[numVerts];
        var uvs = new Vector2[numVerts];
        var tris = new int[numTris];

        Vector3 pivotOff = pivot switch
        {
            PivotPosition.Center => new Vector3(0f, -height / 2f, 0f),
            PivotPosition.Top => new Vector3(0f, -height, 0f),
            _ => Vector3.zero
        };

        var vi = 0;
        for (var i = 0; i <= segments; ++i)
        {
            float t = (float)i / segments;
            float angle = t * turns * 2f * MathF.PI;
            float y = t * height;

            var center = new Vector3(MathF.Cos(angle) * radius, y, MathF.Sin(angle) * radius);

            // Compute Frenet frame
            float tNext = Mathf.Min(t + 0.001f, 1f);
            float aNext = tNext * turns * 2f * MathF.PI;
            float yNext = tNext * height;
            var next = new Vector3(MathF.Cos(aNext) * radius, yNext, MathF.Sin(aNext) * radius);

            Vector3 tangent = (next - center).normalized;
            Vector3 normal = new Vector3(-MathF.Cos(angle), 0f, -MathF.Sin(angle)).normalized;
            Vector3 binormal = Vector3.Cross(tangent, normal).normalized;
            normal = Vector3.Cross(binormal, tangent).normalized;

            for (var j = 0; j <= tubeSegments; ++j)
            {
                float phi = j * 2f * MathF.PI / tubeSegments;
                Vector3 offset = (normal * MathF.Cos(phi) + binormal * MathF.Sin(phi)) * tubeRadius;

                verts[vi] = center + offset + pivotOff;
                normals[vi] = offset.normalized;
                uvs[vi] = new Vector2(t, (float)j / tubeSegments);
                vi++;
            }
        }

        var ti = 0;
        for (var i = 0; i < segments; ++i)
        {
            int cur = i * (tubeSegments + 1);
            int nxt = (i + 1) * (tubeSegments + 1);
            for (var j = 0; j < tubeSegments; ++j)
            {
                tris[ti] = cur + j;
                tris[ti + 1] = nxt + j;
                tris[ti + 2] = cur + j + 1;
                tris[ti + 3] = nxt + j;
                tris[ti + 4] = nxt + j + 1;
                tris[ti + 5] = cur + j + 1;
                ti += 6;
            }
        }

        return MeshBuilder.Build(verts, tris, uvs, normals);
    }
}