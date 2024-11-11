using System;
using UnityEngine;

public static class MeshExtensions
{
    static int[] _triIdentity = Array.Empty<int>();

    public static void RecalculateFlatNormals(this Mesh mesh)
    {
        var verts = mesh.vertices;
        var tris = mesh.triangles;
        if (verts == null || tris == null || tris.Length % 3 != 0) return;

        var flatVerts = new Vector3[tris.Length];
        var normals = new Vector3[tris.Length];

        for (int i = 0; i < tris.Length; i += 3)
        {
            var n = GetTriangleNormal(verts[tris[i]], verts[tris[i + 1]], verts[tris[i + 2]]);
            for (int j = 0; j < 3; ++j)
            {
                int idx = i + j;
                flatVerts[idx] = verts[tris[idx]];
                normals[idx] = n;
            }
        }
        mesh.vertices = flatVerts;
        mesh.triangles = GetIdentityArray(tris.Length);
        mesh.normals = normals;
    }
    
    public static void Extrude(this Mesh mesh, float dist)
    {
        var verts = mesh.vertices;
        var normals = mesh.normals;
        if (verts == null || normals == null) return;

        int vertCount = verts.Length;
        var newVerts = new Vector3[vertCount * 2];
        Array.Copy(verts, newVerts, vertCount);
        for (int i = 0; i < vertCount; ++i)
            newVerts[i + vertCount] = verts[i] + normals[i] * dist;

        var tris = mesh.triangles;
        var newTris = new int[tris.Length * 2];
        Array.Copy(tris, newTris, tris.Length);
        for (int i = 0; i < tris.Length; ++i)
            newTris[i + tris.Length] = tris[i] + vertCount;

        mesh.vertices = newVerts;
        mesh.triangles = newTris;
        mesh.RecalculateNormals();
    }

    public static void ApplyVerticalGradient(this Mesh mesh, Color topColor, Color bottomColor)
    {
        var verts = mesh.vertices;
        if (verts == null) return;

        var colors = new Color[verts.Length];
        var (minY, maxY) = GetMinMaxY(verts);
        float range = Mathf.Abs(maxY - minY) > MathfConstants.ZeroTolerance ? 1f / (maxY - minY) : 0f;

        for (int i = 0; i < verts.Length; ++i)
            colors[i] = Color.Lerp(bottomColor, topColor, (verts[i].y - minY) * range);

        mesh.colors = colors;
    }

    public static void MakeUniqueVertices(this Mesh mesh)
    {
        var verts = mesh.vertices;
        var tris = mesh.triangles;
        if (verts == null || tris == null) return;

        var uniqueVerts = new Vector3[tris.Length];
        var newTris = GetIdentityArray(tris.Length);

        for (int i = 0; i < tris.Length; ++i)
            uniqueVerts[i] = verts[tris[i]];

        mesh.vertices = uniqueVerts;
        mesh.triangles = newTris;
        mesh.RecalculateNormals();
    }
    
    static Vector3 GetTriangleNormal(Vector3 v1, Vector3 v2, Vector3 v3) =>
        Vector3.Cross(v2 - v1, v3 - v1).normalized;

    static int[] GetIdentityArray(int length)
    {
        int currentLength = _triIdentity.Length;
        if (currentLength != length) 
        {
            Array.Resize(ref _triIdentity, length);
            for (int i = currentLength; i < length; ++i) _triIdentity[i] = i;
        }
        return _triIdentity;
    }

    static (float min, float max) GetMinMaxY(Vector3[] verts)
    {
        float minY = float.MaxValue, maxY = float.MinValue;
        foreach (var v in verts)
        {
            if (v.y < minY) minY = v.y;
            if (v.y > maxY) maxY = v.y;
        }
        return (minY, maxY);
    }
}
