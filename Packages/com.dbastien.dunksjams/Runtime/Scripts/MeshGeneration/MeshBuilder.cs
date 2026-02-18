using System;
using UnityEngine;

public enum NormalsType
{
    Vertex,
    Face
}

public enum PivotPosition
{
    Bottom,
    Center,
    Top
}

public static class MeshBuilder
{
    public static Mesh Build(Vector3[] verts, int[] tris, Vector2[] uvs = null, Vector3[] normals = null)
    {
        Mesh mesh = new() { vertices = verts, triangles = tris };

        if (uvs != null) mesh.uv = uvs;
        if (normals != null)
            mesh.normals = normals;
        else
            mesh.RecalculateNormals();

        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        return mesh;
    }

    public static void AddTriangle(int[] tris, int i, int v0, int v1, int v2) =>
        (tris[i], tris[i + 1], tris[i + 2]) = (v0, v1, v2);

    public static void AddQuad(int[] tris, int i, int v0, int v1, int v2, int v3) =>
        (tris[i], tris[i + 1], tris[i + 2], tris[i + 3], tris[i + 4], tris[i + 5]) = (v0, v1, v2, v2, v1, v3);

    public static Vector3 PivotOffset(PivotPosition pivot, float height) => pivot switch
    {
        PivotPosition.Bottom => new Vector3(0f, height / 2f, 0f),
        PivotPosition.Top => new Vector3(0f, -height / 2f, 0f),
        _ => Vector3.zero
    };

    public static void ApplyPivot(Vector3[] verts, Vector3 offset)
    {
        for (var i = 0; i < verts.Length; ++i) verts[i] += offset;
    }

    public static void DuplicateForFlatShading(ref Vector3[] verts, ref Vector2[] uvs, int[] tris)
    {
        var newVerts = new Vector3[tris.Length];
        var newUvs = new Vector2[tris.Length];
        for (var i = 0; i < tris.Length; ++i)
        {
            newVerts[i] = verts[tris[i]];
            newUvs[i] = uvs[tris[i]];
            tris[i] = i;
        }

        verts = newVerts;
        uvs = newUvs;
    }

    public static void GeneratePlane
    (
        Vector3 a, Vector3 b, Vector3 c, Vector3 d,
        int segX, int segY,
        Vector3[] verts, Vector2[] uvs, int[] tris,
        ref int vertIdx, ref int triIdx
    )
    {
        float uvFactorX = 1f / segX;
        float uvFactorY = 1f / segY;
        Vector3 vDown = d - a;
        Vector3 vUp = c - b;
        int vertOffset = vertIdx;

        for (var y = 0; y <= segY; ++y)
        for (var x = 0; x <= segX; ++x)
        {
            Vector3 pDown = a + vDown * y * uvFactorY;
            Vector3 pUp = b + vUp * y * uvFactorY;
            verts[vertIdx] = pDown + (pUp - pDown) * x * uvFactorX;
            uvs[vertIdx] = new Vector2(x * uvFactorX, y * uvFactorY);
            vertIdx++;
        }

        int w = segX + 1;
        for (var y = 0; y < segY; ++y)
        for (var x = 0; x < segX; ++x)
        {
            int vi = vertOffset + y * w + x;
            tris[triIdx] = vi;
            tris[triIdx + 1] = vi + w;
            tris[triIdx + 2] = vi + 1;
            tris[triIdx + 3] = vi + w;
            tris[triIdx + 4] = vi + w + 1;
            tris[triIdx + 5] = vi + 1;
            triIdx += 6;
        }
    }

    public static Vector2 SphericalUV(Vector3 p)
    {
        Vector3 n = p.normalized;
        return new Vector2(
            0.5f + MathF.Atan2(n.z, n.x) / (MathF.PI * 2f),
            1f - (0.5f - MathF.Asin(n.y) / MathF.PI));
    }
}