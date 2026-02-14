using System;
using UnityEngine;

public static class CapsuleGenerator
{
    public static Mesh Generate(
        float radius = 0.5f, float height = 2f,
        int segments = 16, int heightSegments = 1,
        NormalsType normalsType = NormalsType.Vertex,
        PivotPosition pivot = PivotPosition.Center)
    {
        segments = Mathf.Max(segments, 4);
        heightSegments = Mathf.Max(heightSegments, 1);

        var cylinderHeight = Mathf.Max(height - radius * 2f, 0f);
        int rings = segments;
        int sectors = segments + 1;
        if ((rings & 1) == 0) { rings++; sectors = segments + 1; }

        float R = 1f / (rings - 1);
        float S = 1f / (sectors - 1);
        int midRing = rings / 2 + 1;

        int sphereVerts = rings * sectors + sectors;
        int sphereTris = (rings - 1) * (sectors - 1) * 6;
        int cylVerts = (segments + 1) * (heightSegments + 1);
        int cylTris = segments * 6 * heightSegments;

        var verts = new Vector3[sphereVerts + cylVerts];
        var normals = new Vector3[sphereVerts + cylVerts];
        var uvs = new Vector2[sphereVerts + cylVerts];
        var tris = new int[sphereTris + cylTris];

        var capsuleRadius = radius + cylinderHeight / 2f;
        var pivotOff = pivot switch
        {
            PivotPosition.Bottom => new Vector3(0f, capsuleRadius, 0f),
            PivotPosition.Top    => new Vector3(0f, -capsuleRadius, 0f),
            _                    => Vector3.zero
        };

        int vi = 0, ti = 0;

        // Upper hemisphere
        for (var r = 0; r < midRing; ++r)
        {
            var y = MathF.Cos(-MathF.PI * 2f + MathF.PI * r * R);
            var sinR = MathF.Sin(MathF.PI * r * R);
            for (var s = 0; s < sectors; ++s)
            {
                float x = MathF.Sin(2f * MathF.PI * s * S) * sinR;
                float z = MathF.Cos(2f * MathF.PI * s * S) * sinR;
                verts[vi] = new Vector3(x, y, z) * radius + pivotOff;
                verts[vi].y += cylinderHeight / 2f;
                normals[vi] = new Vector3(x, y, z);
                var uv = MeshBuilder.SphericalUV(verts[vi] - pivotOff);
                uvs[vi] = new Vector2(1f - s * S, uv.y);

                if (r < midRing - 1 && s < sectors - 1)
                {
                    tris[ti]     = (r + 1) * sectors + s;
                    tris[ti + 1] = r * sectors + s + 1;
                    tris[ti + 2] = r * sectors + s;
                    tris[ti + 3] = (r + 1) * sectors + s + 1;
                    tris[ti + 4] = r * sectors + s + 1;
                    tris[ti + 5] = (r + 1) * sectors + s;
                    ti += 6;
                }
                vi++;
            }
        }

        // Central cylinder
        if (cylinderHeight > 0f)
        {
            int cylVi = sphereVerts;
            int cylTi = sphereTris;
            int cylTriVert = sphereVerts;
            var heightStep = cylinderHeight / heightSegments;
            var bottom = new Vector3(0f, -cylinderHeight / 2f, 0f);
            var sinMid = MathF.Sin(MathF.PI * (midRing - 1) * R);

            for (var s = 0; s <= segments; ++s)
            {
                float x = MathF.Sin(2f * MathF.PI * s * S) * sinMid;
                float z = MathF.Cos(2f * MathF.PI * s * S) * sinMid;
                var dir = new Vector3(x, 0f, z);
                var h = 0f;
                for (var j = 0; j <= heightSegments; ++j)
                {
                    verts[cylVi] = bottom + dir * radius + new Vector3(0f, h, 0f) + pivotOff;
                    normals[cylVi] = dir;
                    var uv = MeshBuilder.SphericalUV(verts[cylVi] - pivotOff);
                    uvs[cylVi] = new Vector2(1f - s * S, uv.y);
                    cylVi++;
                    h += heightStep;
                }
            }

            for (var i = 0; i < segments; ++i)
            {
                int next = sphereVerts + (i + 1) * (heightSegments + 1);
                for (var j = 0; j < heightSegments; ++j)
                {
                    tris[cylTi]     = next;
                    tris[cylTi + 1] = next + 1;
                    tris[cylTi + 2] = cylTriVert;
                    tris[cylTi + 3] = next + 1;
                    tris[cylTi + 4] = cylTriVert + 1;
                    tris[cylTi + 5] = cylTriVert;
                    cylTi += 6;
                    cylTriVert++;
                    next++;
                }
                cylTriVert++;
            }
        }

        // Lower hemisphere
        for (var r = midRing - 1; r < rings; ++r)
        {
            var y = MathF.Cos(-MathF.PI * 2f + MathF.PI * r * R);
            var sinR = MathF.Sin(MathF.PI * r * R);
            for (var s = 0; s < sectors; ++s)
            {
                float x = MathF.Sin(2f * MathF.PI * s * S) * sinR;
                float z = MathF.Cos(2f * MathF.PI * s * S) * sinR;
                verts[vi] = new Vector3(x, y, z) * radius + pivotOff;
                verts[vi].y -= cylinderHeight / 2f;
                normals[vi] = new Vector3(x, y, z);
                var uv = MeshBuilder.SphericalUV(verts[vi] - pivotOff);
                uvs[vi] = new Vector2(1f - s * S, uv.y);

                if (r < rings - 1 && s < sectors - 1)
                {
                    tris[ti]     = (r + 2) * sectors + s;
                    tris[ti + 1] = (r + 1) * sectors + s + 1;
                    tris[ti + 2] = (r + 1) * sectors + s;
                    tris[ti + 3] = (r + 2) * sectors + s + 1;
                    tris[ti + 4] = (r + 1) * sectors + s + 1;
                    tris[ti + 5] = (r + 2) * sectors + s;
                    ti += 6;
                }
                vi++;
            }
        }

        Mesh mesh = new()
        {
            vertices = verts,
            normals = normals,
            uv = uvs,
            triangles = tris
        };

        if (normalsType == NormalsType.Face) mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        return mesh;
    }
}
