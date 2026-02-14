using System;
using UnityEngine;

public static class RingGenerator
{
    public static Mesh Generate(float innerRadius = 0.3f, float outerRadius = 0.5f, int segments = 24)
    {
        segments = Mathf.Max(segments, 3);

        var verts = new Vector3[segments * 2];
        var normals = new Vector3[segments * 2];
        var uvs = new Vector2[segments * 2];
        var tris = new int[segments * 6];

        int vi = 0;
        for (var i = 0; i < segments; ++i)
        {
            var angle = (float)i / segments * MathF.PI * 2f;
            var dir = new Vector3(MathF.Sin(angle), 0f, MathF.Cos(angle));
            var uvRatio = 0.5f * (innerRadius / outerRadius);
            var uvDir = new Vector2(dir.x * 0.5f, dir.z * 0.5f);
            var uvInner = new Vector2(dir.x * uvRatio, dir.z * uvRatio);
            var uvCenter = new Vector2(0.5f, 0.5f);

            verts[vi] = dir * innerRadius;
            normals[vi] = Vector3.up;
            uvs[vi] = uvCenter + uvInner;
            vi++;

            verts[vi] = dir * outerRadius;
            normals[vi] = Vector3.up;
            uvs[vi] = uvCenter + uvDir;
            vi++;
        }

        int ti = 0;
        for (var i = 0; i < segments; ++i)
        {
            int cur = i * 2;
            int next = ((i + 1) % segments) * 2;

            tris[ti]     = cur;
            tris[ti + 1] = cur + 1;
            tris[ti + 2] = next + 1;
            tris[ti + 3] = next;
            tris[ti + 4] = cur;
            tris[ti + 5] = next + 1;
            ti += 6;
        }

        return MeshBuilder.Build(verts, tris, uvs, normals);
    }
}
