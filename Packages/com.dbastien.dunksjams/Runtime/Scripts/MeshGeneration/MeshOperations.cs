using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// General-purpose mesh utility operations: submesh extraction, normal flipping,
/// tangent calculation (Lengyel), and bounds computation.
/// Extracted from LandscapeBuilder's LBMeshOperations.
/// </summary>
public static class MeshOperations
{
    /// <summary>
    /// Create a new mesh from a submesh of baseMesh at the given index.
    /// </summary>
    public static Mesh CreateMeshFromSubmesh(Mesh baseMesh, int submeshIndex)
    {
        var newMesh = new Mesh();
        var triangles = new List<int>();
        triangles.AddRange(baseMesh.GetTriangles(submeshIndex));

        var newVerts = new List<Vector3>();
        var newUvs = new List<Vector2>();
        var newNormals = new List<Vector3>();
        var newColors = new List<Color>();
        var newTangents = new List<Vector4>();

        var oldToNew = new Dictionary<int, int>();
        int newIdx = 0;

        var verts = baseMesh.vertices;
        var normals = baseMesh.normals ?? System.Array.Empty<Vector3>();
        var colors = baseMesh.colors ?? System.Array.Empty<Color>();
        var tangents = baseMesh.tangents ?? System.Array.Empty<Vector4>();
        var uvs = baseMesh.uv ?? System.Array.Empty<Vector2>();

        for (int i = 0; i < verts.Length; i++)
        {
            if (!triangles.Contains(i)) continue;

            newVerts.Add(verts[i]);
            newNormals.Add(i < normals.Length ? normals[i] : Vector3.up);
            newColors.Add(i < colors.Length ? colors[i] : Color.white);
            newTangents.Add(i < tangents.Length ? tangents[i] : new Vector4(1, 0, 0, 1));
            newUvs.Add(i < uvs.Length ? uvs[i] : Vector2.zero);

            oldToNew[i] = newIdx++;
        }

        var newTris = new int[triangles.Count];
        for (int i = 0; i < triangles.Count; i++)
            newTris[i] = oldToNew[triangles[i]];

        newMesh.vertices = newVerts.ToArray();
        newMesh.triangles = newTris;
        if (newUvs.Count > 0) newMesh.uv = newUvs.ToArray();
        if (newNormals.Count > 0) newMesh.normals = newNormals.ToArray();
        else newMesh.RecalculateNormals();
        if (newColors.Count > 0) newMesh.colors = newColors.ToArray();
        if (newTangents.Count > 0) newMesh.tangents = newTangents.ToArray();
        else newMesh.RecalculateTangents();

        newMesh.RecalculateBounds();
        return newMesh;
    }

    /// <summary>
    /// Flip all normals and reverse triangle winding on every submesh.
    /// </summary>
    public static void FlipNormals(Mesh mesh)
    {
        if (mesh?.normals == null || mesh.normals.Length == 0) return;

        var flipped = mesh.normals;
        for (int i = 0; i < flipped.Length; i++)
            flipped[i] = -flipped[i];

        for (int sub = 0; sub < mesh.subMeshCount; sub++)
        {
            var tris = mesh.GetTriangles(sub);
            for (int t = 0; t < tris.Length; t += 3)
                (tris[t], tris[t + 1]) = (tris[t + 1], tris[t]);
            mesh.SetTriangles(tris, sub);
        }

        mesh.normals = flipped;
    }

    /// <summary>
    /// Calculate tangents using Lengyel's method.
    /// http://www.terathon.com/code/tangent.html
    /// </summary>
    public static void CalculateTangents(Mesh mesh)
    {
        var tris = mesh.triangles;
        var verts = mesh.vertices;
        var uv = mesh.uv;
        var normals = mesh.normals;

        int triCount = tris.Length;
        int vertCount = verts.Length;

        var tan1 = new Vector3[vertCount];
        var tan2 = new Vector3[vertCount];
        var tangents = new Vector4[vertCount];

        for (int a = 0; a < triCount; a += 3)
        {
            int i1 = tris[a], i2 = tris[a + 1], i3 = tris[a + 2];

            Vector3 v1 = verts[i1], v2 = verts[i2], v3 = verts[i3];
            Vector2 w1 = uv[i1], w2 = uv[i2], w3 = uv[i3];

            float x1 = v2.x - v1.x, x2 = v3.x - v1.x;
            float y1 = v2.y - v1.y, y2 = v3.y - v1.y;
            float z1 = v2.z - v1.z, z2 = v3.z - v1.z;
            float s1 = w2.x - w1.x, s2 = w3.x - w1.x;
            float t1 = w2.y - w1.y, t2 = w3.y - w1.y;

            float div = s1 * t2 - s2 * t1;
            float r = div == 0f ? 0f : 1f / div;

            var sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
            var tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

            tan1[i1] += sdir; tan1[i2] += sdir; tan1[i3] += sdir;
            tan2[i1] += tdir; tan2[i2] += tdir; tan2[i3] += tdir;
        }

        for (int a = 0; a < vertCount; a++)
        {
            Vector3 n = normals[a], t = tan1[a];
            Vector3.OrthoNormalize(ref n, ref t);
            tangents[a] = new Vector4(t.x, t.y, t.z,
                Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0f ? -1f : 1f);
        }

        mesh.tangents = tangents;
    }

    /// <summary>
    /// Get world-space bounds encapsulating all child MeshRenderers of a transform.
    /// </summary>
    public static Bounds GetBounds(Transform transform, bool includeInactive = true)
    {
        if (transform == null) return default;

        var bounds = new Bounds(transform.position, Vector3.zero);
        var renderers = transform.GetComponentsInChildren<MeshRenderer>(includeInactive);

        if (renderers != null)
            foreach (var r in renderers)
                bounds.Encapsulate(r.bounds);

        return bounds;
    }
}
