using System;
using System.Collections.Generic;
using UnityEngine;

public static class GeoSphereGenerator
{
    public enum BaseType { Tetrahedron, Octahedron, Icosahedron }

    public static Mesh Generate(
        float radius = 0.5f, int subdivision = 2,
        BaseType baseType = BaseType.Icosahedron,
        NormalsType normalsType = NormalsType.Vertex,
        PivotPosition pivot = PivotPosition.Center)
    {
        subdivision = Mathf.Clamp(subdivision, 0, 6);

        var (baseVerts, baseTris) = CreateBase(radius, baseType);
        var vertList = new List<Vector3>(baseVerts);
        var triList = new List<int>(baseTris);
        var lookup = new Dictionary<long, int>();

        for (var i = 0; i < subdivision; ++i)
        {
            var newTris = new List<int>(triList.Count * 4);
            for (var t = 0; t < triList.Count; t += 3)
            {
                int v1 = triList[t], v2 = triList[t + 1], v3 = triList[t + 2];
                int a = MidPoint(vertList, radius, v1, v2, lookup);
                int b = MidPoint(vertList, radius, v2, v3, lookup);
                int c = MidPoint(vertList, radius, v3, v1, lookup);
                newTris.AddRange(new[] { v1, a, c, v2, b, a, v3, c, b, a, b, c });
            }
            triList = newTris;
        }

        var verts = vertList.ToArray();
        var tris = triList.ToArray();

        if (normalsType == NormalsType.Face)
        {
            var uvsDummy = new Vector2[verts.Length];
            MeshBuilder.DuplicateForFlatShading(ref verts, ref uvsDummy, tris);
        }

        // Spherical UV
        var uvs = new Vector2[verts.Length];
        for (var i = 0; i < verts.Length; ++i)
            uvs[i] = MeshBuilder.SphericalUV(verts[i]);

        CorrectSeam(ref verts, ref uvs, ref tris);

        // Normals
        var normals = new Vector3[verts.Length];
        for (var i = 0; i < verts.Length; ++i)
            normals[i] = verts[i].normalized;

        // Pivot
        var pivotOff = MeshBuilder.PivotOffset(pivot, radius * 2f);
        if (pivotOff != Vector3.zero) MeshBuilder.ApplyPivot(verts, pivotOff);

        return MeshBuilder.Build(verts, tris, uvs, normalsType == NormalsType.Vertex ? normals : null);
    }

    static int MidPoint(List<Vector3> verts, float radius, int a, int b, Dictionary<long, int> lookup)
    {
        long key = a < b ? ((long)a << 32) + b : ((long)b << 32) + a;
        if (lookup.TryGetValue(key, out var idx)) return idx;

        var mid = ((verts[a] + verts[b]) * 0.5f).normalized * radius;
        verts.Add(mid);
        idx = verts.Count - 1;
        lookup[key] = idx;
        return idx;
    }

    static (Vector3[], int[]) CreateBase(float radius, BaseType type) => type switch
    {
        BaseType.Tetrahedron => CreateTetrahedron(radius),
        BaseType.Octahedron  => CreateOctahedron(radius),
        _                    => CreateIcosahedron(radius)
    };

    static (Vector3[], int[]) CreateTetrahedron(float r)
    {
        var b = 1f / MathF.Sqrt(2f);
        var c = 1f / MathF.Sqrt(1.5f);
        var a2 = 0.67f * r / c;
        var b2 = 0.67f * r * b / c;
        return (
            new[] { new Vector3(a2, 0, -b2), new Vector3(-a2, 0, -b2), new Vector3(0, a2, b2), new Vector3(0, -a2, b2) },
            new[] { 0, 1, 2, 1, 3, 2, 0, 2, 3, 0, 3, 1 }
        );
    }

    static (Vector3[], int[]) CreateOctahedron(float r) => (
        new[]
        {
            new Vector3(0, -r, 0), new Vector3(-r, 0, 0), new Vector3(0, 0, -r),
            new Vector3(r, 0, 0), new Vector3(0, 0, r), new Vector3(0, r, 0)
        },
        new[] { 0,1,2, 0,2,3, 0,3,4, 0,4,1, 5,2,1, 5,3,2, 5,4,3, 5,1,4 }
    );

    static (Vector3[], int[]) CreateIcosahedron(float r)
    {
        var a = 1f;
        var b = (1f + MathF.Sqrt(5f)) / 2f;
        var scale = r / MathF.Sqrt(a * a + b * b);
        a *= scale; b *= scale;
        return (
            new[]
            {
                new Vector3(-a, b, 0), new Vector3(a, b, 0),
                new Vector3(-a, -b, 0), new Vector3(a, -b, 0),
                new Vector3(0, -a, b), new Vector3(0, a, b),
                new Vector3(0, -a, -b), new Vector3(0, a, -b),
                new Vector3(b, 0, -a), new Vector3(b, 0, a),
                new Vector3(-b, 0, -a), new Vector3(-b, 0, a)
            },
            new[]
            {
                0,11,5, 0,5,1, 0,1,7, 0,7,10, 0,10,11,
                1,5,9, 5,11,4, 11,10,2, 10,7,6, 7,1,8,
                3,9,4, 3,4,2, 3,2,6, 3,6,8, 3,8,9,
                4,9,5, 2,4,11, 6,2,10, 8,6,7, 9,8,1
            }
        );
    }

    static void CorrectSeam(ref Vector3[] verts, ref Vector2[] uvs, ref int[] tris)
    {
        var newVerts = new List<Vector3>(verts);
        var newUvs = new List<Vector2>(uvs);
        var cache = new Dictionary<int, int>();

        for (var i = 0; i < tris.Length; i += 3)
        {
            var u0 = uvs[tris[i]]; var u1 = uvs[tris[i + 1]]; var u2 = uvs[tris[i + 2]];
            var cross = Vector3.Cross((Vector3)(u0 - u1), (Vector3)(u2 - u1));
            if (cross.z > 0) continue;

            for (var j = i; j < i + 3; ++j)
            {
                int idx = tris[j];
                if (!(uvs[idx].x >= 0.8f)) continue;

                if (cache.TryGetValue(idx, out var cached))
                {
                    tris[j] = cached;
                }
                else
                {
                    var uv = uvs[idx]; uv.x -= 1f;
                    newVerts.Add(verts[idx]);
                    newUvs.Add(uv);
                    int newIdx = newVerts.Count - 1;
                    cache[idx] = newIdx;
                    tris[j] = newIdx;
                }
            }
        }

        verts = newVerts.ToArray();
        uvs = newUvs.ToArray();
    }
}
