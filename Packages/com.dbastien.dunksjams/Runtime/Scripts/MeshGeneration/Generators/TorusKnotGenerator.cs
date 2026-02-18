using System;
using UnityEngine;

public static class TorusKnotGenerator
{
    public static Mesh Generate
    (
        float torusRadius = 0.5f, float tubeRadius = 0.15f,
        int torusSegments = 64, int tubeSegments = 8,
        int p = 2, int q = 3,
        NormalsType normalsType = NormalsType.Vertex,
        PivotPosition pivot = PivotPosition.Center
    )
    {
        torusSegments = Mathf.Max(torusSegments, 3);
        tubeSegments = Mathf.Max(tubeSegments, 3);
        p = Mathf.Max(p, 1);
        q = Mathf.Max(q, 1);

        int numVerts = (torusSegments + 1) * (tubeSegments + 1);
        int numTris = 2 * tubeSegments * torusSegments * 3;

        var verts = new Vector3[numVerts];
        var normals = new Vector3[numVerts];
        var uvs = new Vector2[numVerts];
        var tris = new int[numTris];

        float step = 2f * MathF.PI / torusSegments;
        Vector3 prev = Vector3.zero;
        Vector3 cur = Vector3.zero;
        var vi = 0;
        var theta = 0f;

        float minY = float.MaxValue, maxY = float.MinValue;

        for (var i = 0; i <= torusSegments + 1; ++i)
        {
            theta += step;
            float r = torusRadius * 0.5f * (2f + MathF.Sin(q * theta));
            prev = cur;
            cur = new Vector3(r * MathF.Cos(p * theta), r * MathF.Cos(q * theta), r * MathF.Sin(p * theta));

            if (i == 0) continue;

            Vector3 T = cur - prev;
            Vector3 N = cur + prev;
            Vector3 B = Vector3.Cross(T, N);
            N = Vector3.Cross(B, T);
            N.Normalize();
            B.Normalize();

            var theta2 = 0f;
            float step2 = 2f * MathF.PI / tubeSegments;

            for (var j = 0; j <= tubeSegments; ++j)
            {
                theta2 += step2;
                float s = tubeRadius * MathF.Sin(theta2);
                float t = tubeRadius * MathF.Cos(theta2);
                Vector3 u = N * s + B * t;

                verts[vi] = cur + u;
                normals[vi] = u.normalized;
                uvs[vi] = new Vector2((float)(i - 1) / torusSegments, (float)j / tubeSegments);

                if (verts[vi].y < minY) minY = verts[vi].y;
                if (verts[vi].y > maxY) maxY = verts[vi].y;
                vi++;
            }

            if (i <= torusSegments)
            {
                int curSeg = (i - 1) * (tubeSegments + 1);
                int nextSeg = i * (tubeSegments + 1);
                int ti2 = (i - 1) * tubeSegments * 6;
                for (var j = 0; j < tubeSegments; ++j)
                {
                    tris[ti2] = nextSeg + j;
                    tris[ti2 + 1] = curSeg + j + 1;
                    tris[ti2 + 2] = curSeg + j;
                    tris[ti2 + 3] = nextSeg + j + 1;
                    tris[ti2 + 4] = curSeg + j + 1;
                    tris[ti2 + 5] = nextSeg + j;
                    ti2 += 6;
                }
            }
        }

        if (pivot != PivotPosition.Center)
        {
            float off = pivot == PivotPosition.Bottom ? -minY : -maxY;
            for (var i = 0; i < verts.Length; ++i) verts[i].y += off;
        }

        if (normalsType == NormalsType.Face)
            MeshBuilder.DuplicateForFlatShading(ref verts, ref uvs, tris);

        return normalsType == NormalsType.Vertex
            ? MeshBuilder.Build(verts, tris, uvs, normals)
            : MeshBuilder.Build(verts, tris, uvs);
    }
}