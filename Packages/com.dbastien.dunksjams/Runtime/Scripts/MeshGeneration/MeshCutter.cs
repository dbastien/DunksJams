using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cuts a mesh along a plane, producing two new meshes with properly
/// triangulated cut surfaces, interpolated normals, and UVs.
/// </summary>
public class MeshCutter
{
    private readonly List<Vector3>[] _verts = { new(), new() };
    private readonly List<Vector3>[] _normals = { new(), new() };
    private readonly List<Vector2>[] _uvs = { new(), new() };
    private readonly List<int>[] _tris = { new(), new() };
    private readonly List<int>[] _capTris = { new(), new() };
    private readonly List<Vector3> _cutEdgePoints = new();

    /// <summary>
    /// Cut a mesh by a world-space plane. Returns true if the cut produced two meshes.
    /// </summary>
    public bool Cut(Mesh mesh, Plane plane, out Mesh frontMesh, out Mesh backMesh)
    {
        Clear();

        Vector3[] meshVerts = mesh.vertices;
        Vector3[] meshNormals = mesh.normals;
        Vector2[] meshUVs = mesh.uv;
        int[] meshTris = mesh.triangles;

        for (var i = 0; i < meshTris.Length; i += 3)
        {
            int i0 = meshTris[i], i1 = meshTris[i + 1], i2 = meshTris[i + 2];
            Vector3 v0 = meshVerts[i0];
            Vector3 v1 = meshVerts[i1];
            Vector3 v2 = meshVerts[i2];

            bool s0 = plane.GetSide(v0), s1 = plane.GetSide(v1), s2 = plane.GetSide(v2);

            if (s0 == s1 && s1 == s2)
            {
                // Whole triangle on one side
                int side = s0 ? 0 : 1;
                AddTriangle(side, v0, v1, v2,
                    meshNormals[i0], meshNormals[i1], meshNormals[i2],
                    meshUVs[i0], meshUVs[i1], meshUVs[i2]);
            }
            else
            {
                // Triangle straddles the plane -- split it
                SplitTriangle(plane,
                    v0, v1, v2,
                    meshNormals[i0], meshNormals[i1], meshNormals[i2],
                    meshUVs[i0], meshUVs[i1], meshUVs[i2],
                    s0, s1, s2);
            }
        }

        if (_verts[0].Count < 3 || _verts[1].Count < 3)
        {
            frontMesh = null;
            backMesh = null;
            return false;
        }

        // Triangulate the cut surface and add cap polygons
        FillCutSurface(plane);

        frontMesh = BuildMesh(0);
        backMesh = BuildMesh(1);
        return true;
    }

    private void Clear()
    {
        for (var i = 0; i < 2; ++i)
        {
            _verts[i].Clear();
            _normals[i].Clear();
            _uvs[i].Clear();
            _tris[i].Clear();
            _capTris[i].Clear();
        }

        _cutEdgePoints.Clear();
    }

    private void AddTriangle
    (
        int side, Vector3 v0, Vector3 v1, Vector3 v2,
        Vector3 n0, Vector3 n1, Vector3 n2, Vector2 u0, Vector2 u1, Vector2 u2
    )
    {
        int baseIdx = _verts[side].Count;
        _verts[side].Add(v0);
        _verts[side].Add(v1);
        _verts[side].Add(v2);
        _normals[side].Add(n0);
        _normals[side].Add(n1);
        _normals[side].Add(n2);
        _uvs[side].Add(u0);
        _uvs[side].Add(u1);
        _uvs[side].Add(u2);
        _tris[side].Add(baseIdx);
        _tris[side].Add(baseIdx + 1);
        _tris[side].Add(baseIdx + 2);
    }

    private void SplitTriangle
    (
        Plane plane,
        Vector3 v0, Vector3 v1, Vector3 v2,
        Vector3 n0, Vector3 n1, Vector3 n2,
        Vector2 u0, Vector2 u1, Vector2 u2,
        bool s0, bool s1, bool s2
    )
    {
        // Identify the lone vertex (the one on a different side)
        // Rotate so that the lone vertex is v0/n0/u0, s0 is its side
        if (s0 == s1)
        {
            // v2 is alone
            (v0, v1, v2) = (v2, v0, v1);
            (n0, n1, n2) = (n2, n0, n1);
            (u0, u1, u2) = (u2, u0, u1);
            (s0, s1, s2) = (s2, s0, s1);
        }
        else if (s0 == s2)
        {
            // v1 is alone
            (v0, v1, v2) = (v1, v2, v0);
            (n0, n1, n2) = (n1, n2, n0);
            (u0, u1, u2) = (u1, u2, u0);
            (s0, s1, s2) = (s1, s2, s0);
        }
        // else v0 is already alone

        // Intersect edges v0-v1 and v0-v2
        IntersectEdge(plane, v0, v1, n0, n1, u0, u1, out Vector3 hitA, out Vector3 nA, out Vector2 uA);
        IntersectEdge(plane, v0, v2, n0, n2, u0, u2, out Vector3 hitB, out Vector3 nB, out Vector2 uB);

        int loneSide = s0 ? 0 : 1;
        int otherSide = 1 - loneSide;

        // Lone side: 1 triangle (v0, hitA, hitB)
        AddTriangle(loneSide, v0, hitA, hitB, n0, nA, nB, u0, uA, uB);

        // Other side: 2 triangles (v1, v2, hitA) and (hitA, v2, hitB)
        AddTriangle(otherSide, v1, v2, hitA, n1, n2, nA, u1, u2, uA);
        AddTriangle(otherSide, hitA, v2, hitB, nA, n2, nB, uA, u2, uB);

        // Record cut edge points for cap generation
        _cutEdgePoints.Add(hitA);
        _cutEdgePoints.Add(hitB);
    }

    private static void IntersectEdge
    (
        Plane plane, Vector3 a, Vector3 b,
        Vector3 nA, Vector3 nB, Vector2 uA, Vector2 uB,
        out Vector3 hit, out Vector3 hitN, out Vector2 hitU
    )
    {
        var ray = new Ray(a, b - a);
        plane.Raycast(ray, out float dist);
        float t = dist / (b - a).magnitude;
        t = Mathf.Clamp01(t);
        hit = Vector3.Lerp(a, b, t);
        hitN = Vector3.Lerp(nA, nB, t).normalized;
        hitU = Vector2.Lerp(uA, uB, t);
    }

    private void FillCutSurface(Plane plane)
    {
        if (_cutEdgePoints.Count < 6) return; // need at least 3 unique points

        // Project cut points onto the plane's 2D space for triangulation
        Vector3 normal = plane.normal;
        Vector3 tangent = Vector3.Cross(normal, Mathf.Abs(normal.y) < 0.9f ? Vector3.up : Vector3.right).normalized;
        Vector3 bitangent = Vector3.Cross(normal, tangent).normalized;

        // Deduplicate and collect unique points
        var unique = new List<Vector3>();
        var epsilon2 = 1e-8f;
        for (var i = 0; i < _cutEdgePoints.Count; ++i)
        {
            var found = false;
            for (var j = 0; j < unique.Count; ++j)
                if ((_cutEdgePoints[i] - unique[j]).sqrMagnitude < epsilon2)
                {
                    found = true;
                    break;
                }

            if (!found) unique.Add(_cutEdgePoints[i]);
        }

        if (unique.Count < 3) return;

        // Project to 2D
        Vector3 center = Vector3.zero;
        foreach (Vector3 p in unique) center += p;
        center /= unique.Count;

        var pts2D = new Vector2[unique.Count];
        for (var i = 0; i < unique.Count; ++i)
        {
            Vector3 d = unique[i] - center;
            pts2D[i] = new Vector2(Vector3.Dot(d, tangent), Vector3.Dot(d, bitangent));
        }

        // Sort points by angle for convex hull ordering
        Vector2 centroid2D = Vector2.zero;
        foreach (Vector2 p in pts2D) centroid2D += p;
        centroid2D /= pts2D.Length;

        var indices = new int[unique.Count];
        for (var i = 0; i < indices.Length; ++i) indices[i] = i;
        System.Array.Sort(indices, (a, b) =>
            Mathf.Atan2(pts2D[a].y - centroid2D.y, pts2D[a].x - centroid2D.x).
                CompareTo(Mathf.Atan2(pts2D[b].y - centroid2D.y, pts2D[b].x - centroid2D.x)));

        // Ear-clipping triangulation on the sorted polygon
        var polyIndices = new List<int>(indices);
        var capTriIndices = new List<int>();
        EarClipTriangulate(pts2D, polyIndices, capTriIndices);

        // Add cap triangles to both sides (with opposite winding)
        for (var i = 0; i < capTriIndices.Count; i += 3)
        {
            Vector3 p0 = unique[capTriIndices[i]];
            Vector3 p1 = unique[capTriIndices[i + 1]];
            Vector3 p2 = unique[capTriIndices[i + 2]];

            var uv0 = new Vector2(pts2D[capTriIndices[i]].x * 0.5f + 0.5f, pts2D[capTriIndices[i]].y * 0.5f + 0.5f);
            var uv1 = new Vector2(pts2D[capTriIndices[i + 1]].x * 0.5f + 0.5f,
                pts2D[capTriIndices[i + 1]].y * 0.5f + 0.5f);
            var uv2 = new Vector2(pts2D[capTriIndices[i + 2]].x * 0.5f + 0.5f,
                pts2D[capTriIndices[i + 2]].y * 0.5f + 0.5f);

            // Front side (normal facing plane normal)
            AddTriangle(0, p0, p1, p2, -normal, -normal, -normal, uv0, uv1, uv2);
            // Back side (reversed winding)
            AddTriangle(1, p0, p2, p1, normal, normal, normal, uv0, uv2, uv1);
        }
    }

    private static void EarClipTriangulate(Vector2[] points, List<int> polygon, List<int> outTris)
    {
        while (polygon.Count > 2)
        {
            var earFound = false;
            for (var i = 0; i < polygon.Count; ++i)
            {
                int prev = polygon[(i - 1 + polygon.Count) % polygon.Count];
                int cur = polygon[i];
                int next = polygon[(i + 1) % polygon.Count];

                Vector2 a = points[prev];
                Vector2 b = points[cur];
                Vector2 c = points[next];

                // Check if this is a convex vertex (left turn)
                if (Cross2D(b - a, c - a) <= 0) continue;

                // Check no other point is inside this triangle
                var isEar = true;
                for (var j = 0; j < polygon.Count; ++j)
                {
                    int idx = polygon[j];
                    if (idx == prev || idx == cur || idx == next) continue;
                    if (PointInTriangle(points[idx], a, b, c))
                    {
                        isEar = false;
                        break;
                    }
                }

                if (isEar)
                {
                    outTris.Add(prev);
                    outTris.Add(cur);
                    outTris.Add(next);
                    polygon.RemoveAt(i);
                    earFound = true;
                    break;
                }
            }

            if (!earFound) break; // Degenerate polygon
        }
    }

    private static float Cross2D(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

    private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d0 = Cross2D(b - a, p - a);
        float d1 = Cross2D(c - b, p - b);
        float d2 = Cross2D(a - c, p - c);
        bool hasNeg = d0 < 0 || d1 < 0 || d2 < 0;
        bool hasPos = d0 > 0 || d1 > 0 || d2 > 0;
        return !(hasNeg && hasPos);
    }

    private Mesh BuildMesh(int side)
    {
        var mesh = new Mesh
        {
            vertices = _verts[side].ToArray(),
            normals = _normals[side].ToArray(),
            uv = _uvs[side].ToArray(),
            triangles = _tris[side].ToArray()
        };
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        return mesh;
    }
}