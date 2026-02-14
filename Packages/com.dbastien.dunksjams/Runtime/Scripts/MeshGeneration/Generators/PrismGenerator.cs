using System;
using UnityEngine;

/// <summary>
/// Generates an N-sided prism. sides=3 for triangular, sides=6 for hexagonal, etc.
/// </summary>
public static class PrismGenerator
{
    public static Mesh Generate(int sides = 6, float radius = 0.5f, float height = 1f,
                                PivotPosition pivot = PivotPosition.Center)
    {
        sides = Mathf.Max(sides, 3);
        // Unique verts per face for proper normals/UVs
        int sideVerts = sides * 4;
        int capVerts = sides * 2;
        int totalVerts = sideVerts + capVerts + 2; // +2 for cap centers
        int sideTris = sides * 6;
        int capTris = sides * 3 * 2;

        var verts = new Vector3[totalVerts];
        var uvs = new Vector2[totalVerts];
        var tris = new int[sideTris + capTris];
        var angleStep = 2f * MathF.PI / sides;

        var pivotOff = pivot switch
        {
            PivotPosition.Bottom => new Vector3(0f, 0f, 0f),
            PivotPosition.Top    => new Vector3(0f, -height, 0f),
            _                    => new Vector3(0f, -height / 2f, 0f)
        };

        int vi = 0, ti = 0;

        // Side faces (each quad has 4 unique verts)
        for (var i = 0; i < sides; ++i)
        {
            var a0 = i * angleStep;
            var a1 = ((i + 1) % sides) * angleStep;
            var c0 = MathF.Cos(a0); var s0 = MathF.Sin(a0);
            var c1 = MathF.Cos(a1); var s1 = MathF.Sin(a1);

            var bl = new Vector3(c0 * radius, 0f, s0 * radius) + pivotOff;
            var br = new Vector3(c1 * radius, 0f, s1 * radius) + pivotOff;
            var tl = bl + Vector3.up * height;
            var tr = br + Vector3.up * height;

            float u0 = (float)i / sides;
            float u1 = (float)(i + 1) / sides;

            verts[vi] = bl; uvs[vi] = new Vector2(u0, 0f); vi++;
            verts[vi] = br; uvs[vi] = new Vector2(u1, 0f); vi++;
            verts[vi] = tl; uvs[vi] = new Vector2(u0, 1f); vi++;
            verts[vi] = tr; uvs[vi] = new Vector2(u1, 1f); vi++;

            int bv = vi - 4;
            tris[ti] = bv;     tris[ti + 1] = bv + 2; tris[ti + 2] = bv + 1;
            tris[ti + 3] = bv + 1; tris[ti + 4] = bv + 2; tris[ti + 5] = bv + 3;
            ti += 6;
        }

        // Bottom cap
        int botCenter = vi;
        verts[vi] = pivotOff;
        uvs[vi] = new Vector2(0.5f, 0.5f);
        vi++;

        for (var i = 0; i < sides; ++i)
        {
            var angle = i * angleStep;
            var dir = new Vector3(MathF.Cos(angle), 0f, MathF.Sin(angle));
            verts[vi] = dir * radius + pivotOff;
            uvs[vi] = new Vector2(dir.x * 0.5f + 0.5f, dir.z * 0.5f + 0.5f);

            var next = botCenter + 1 + (i + 1) % sides;
            tris[ti] = botCenter; tris[ti + 1] = vi; tris[ti + 2] = next;
            ti += 3;
            vi++;
        }

        // Top cap
        int topCenter = vi;
        verts[vi] = new Vector3(0f, height, 0f) + pivotOff;
        uvs[vi] = new Vector2(0.5f, 0.5f);
        vi++;

        for (var i = 0; i < sides; ++i)
        {
            var angle = i * angleStep;
            var dir = new Vector3(MathF.Cos(angle), 0f, MathF.Sin(angle));
            verts[vi] = dir * radius + new Vector3(0f, height, 0f) + pivotOff;
            uvs[vi] = new Vector2(dir.x * 0.5f + 0.5f, dir.z * 0.5f + 0.5f);

            var next = topCenter + 1 + (i + 1) % sides;
            tris[ti] = topCenter; tris[ti + 1] = next; tris[ti + 2] = vi;
            ti += 3;
            vi++;
        }

        return MeshBuilder.Build(verts, tris, uvs);
    }
}
