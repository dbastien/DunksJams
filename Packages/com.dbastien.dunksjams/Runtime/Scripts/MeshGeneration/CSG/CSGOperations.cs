using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Constructive Solid Geometry operations on Unity meshes.
/// Based on Evan Wallace's csg.js (MIT license).
/// Supports Union, Subtract, and Intersect on two meshes.
/// </summary>
public static class CSGOperations
{
    public static Mesh Union(Mesh meshA, Transform transformA, Mesh meshB, Transform transformB)
    {
        CSGNode a = FromMesh(meshA, transformA, 0);
        CSGNode b = FromMesh(meshB, transformB, 1);

        a.ClipTo(b);
        b.ClipTo(a);
        b.Invert();
        b.ClipTo(a);
        b.Invert();
        a.Build(b.AllPolygons());
        return ToMesh(a);
    }

    public static Mesh Subtract(Mesh meshA, Transform transformA, Mesh meshB, Transform transformB)
    {
        CSGNode a = FromMesh(meshA, transformA, 0);
        CSGNode b = FromMesh(meshB, transformB, 1);

        a.Invert();
        a.ClipTo(b);
        b.ClipTo(a);
        b.Invert();
        b.ClipTo(a);
        b.Invert();
        a.Build(b.AllPolygons());
        a.Invert();
        return ToMesh(a);
    }

    public static Mesh Intersect(Mesh meshA, Transform transformA, Mesh meshB, Transform transformB)
    {
        CSGNode a = FromMesh(meshA, transformA, 0);
        CSGNode b = FromMesh(meshB, transformB, 1);

        a.Invert();
        b.ClipTo(a);
        b.Invert();
        a.ClipTo(b);
        b.ClipTo(a);
        a.Build(b.AllPolygons());
        a.Invert();
        return ToMesh(a);
    }

    private static CSGNode FromMesh(Mesh mesh, Transform transform, int id)
    {
        int[] meshTris = mesh.triangles;
        Vector3[] meshVerts = mesh.vertices;
        Vector3[] meshNormals = mesh.normals;
        Vector2[] meshUV = mesh.uv;

        var polygons = new List<CSGPolygon>(meshTris.Length / 3);
        for (var i = 0; i < meshTris.Length; i += 3)
        {
            int i0 = meshTris[i], i1 = meshTris[i + 1], i2 = meshTris[i + 2];
            polygons.Add(new CSGPolygon(id,
                new CSGVertex(transform.TransformPoint(meshVerts[i0]), transform.TransformDirection(meshNormals[i0]),
                    meshUV[i0]),
                new CSGVertex(transform.TransformPoint(meshVerts[i1]), transform.TransformDirection(meshNormals[i1]),
                    meshUV[i1]),
                new CSGVertex(transform.TransformPoint(meshVerts[i2]), transform.TransformDirection(meshNormals[i2]),
                    meshUV[i2])
            ));
        }

        var node = new CSGNode();
        node.Build(polygons);
        return node;
    }

    private static Mesh ToMesh(CSGNode node)
    {
        List<CSGPolygon> polygons = node.AllPolygons();
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        var tris0 = new List<int>();
        var tris1 = new List<int>();
        var vertCache = new Dictionary<long, int>();

        foreach (CSGPolygon poly in polygons)
            for (var i = 2; i < poly.Vertices.Count; ++i)
            {
                int v0 = AddVertex(poly.Vertices[0], vertices, normals, uvs, vertCache);
                int v1 = AddVertex(poly.Vertices[i - 1], vertices, normals, uvs, vertCache);
                int v2 = AddVertex(poly.Vertices[i], vertices, normals, uvs, vertCache);
                (poly.Id == 0 ? tris0 : tris1).AddRange(new[] { v0, v1, v2 });
            }

        var mesh = new Mesh
        {
            vertices = vertices.ToArray(),
            normals = normals.ToArray(),
            uv = uvs.ToArray(),
            subMeshCount = 2
        };
        mesh.SetTriangles(tris0.ToArray(), 0);
        mesh.SetTriangles(tris1.ToArray(), 1);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        return mesh;
    }

    private static int AddVertex
    (
        CSGVertex v, List<Vector3> verts, List<Vector3> normals, List<Vector2> uvs,
        Dictionary<long, int> cache
    )
    {
        // Simple hash based on position
        long hash = ((long)(v.Position.x * 10000) * 73856093L) ^
                    ((long)(v.Position.y * 10000) * 19349663L) ^
                    ((long)(v.Position.z * 10000) * 83492791L);

        if (cache.TryGetValue(hash, out int idx))
            // Verify it's actually the same vertex (not just hash collision)
            if ((verts[idx] - v.Position).sqrMagnitude < 1e-10f &&
                (normals[idx] - v.Normal).sqrMagnitude < 1e-10f)
                return idx;

        verts.Add(v.Position);
        normals.Add(v.Normal);
        uvs.Add(v.UV);
        idx = verts.Count - 1;
        cache[hash] = idx;
        return idx;
    }
}