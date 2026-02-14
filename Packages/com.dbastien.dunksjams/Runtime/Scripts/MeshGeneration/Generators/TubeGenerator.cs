using System;
using UnityEngine;

/// <summary>
/// Generates a tube (hollow cylinder) with inner and outer radius.
/// </summary>
public static class TubeGenerator
{
    public static Mesh Generate(
        float innerRadius = 0.3f, float outerRadius = 0.5f, float height = 1f,
        int sides = 16, int heightSegments = 1,
        NormalsType normalsType = NormalsType.Vertex,
        PivotPosition pivot = PivotPosition.Bottom)
    {
        sides = Mathf.Max(sides, 3);
        heightSegments = Mathf.Max(heightSegments, 1);

        int bodyVerts = (sides + 1) * (heightSegments + 1) * 2;
        int bodyTris = sides * 6 * heightSegments * 2;
        int capVerts = (sides + 1) * 2 * 2;
        int capTris = sides * 6 * 2;

        var verts = new Vector3[bodyVerts + capVerts];
        var normals = new Vector3[bodyVerts + capVerts];
        var uvs = new Vector2[bodyVerts + capVerts];
        var tris = new int[bodyTris + capTris];

        var pivotOff = pivot switch
        {
            PivotPosition.Center => new Vector3(0f, -height / 2f, 0f),
            PivotPosition.Top    => new Vector3(0f, -height, 0f),
            _                    => Vector3.zero
        };

        int innerOff = (sides + 1) * (heightSegments + 1);
        int innerTriOff = sides * 6 * heightSegments;
        int vi = 0, ti = 0, triVert = 0;
        var heightStep = height / heightSegments;

        // Outer + inner body
        for (var i = 0; i <= sides; ++i)
        {
            var angle = (float)i / sides * MathF.PI * 2f;
            var dir = new Vector3(MathF.Cos(angle), 0f, MathF.Sin(angle)).normalized;
            var h = 0f;

            for (var j = 0; j <= heightSegments; ++j)
            {
                verts[vi] = dir * outerRadius + new Vector3(0, h, 0) + pivotOff;
                normals[vi] = dir;
                uvs[vi] = new Vector2((float)i / sides, (float)j / heightSegments);

                verts[vi + innerOff] = dir * innerRadius + new Vector3(0, h, 0) + pivotOff;
                normals[vi + innerOff] = -dir;
                uvs[vi + innerOff] = new Vector2((float)i / sides, (float)j / heightSegments);
                vi++;
                h += heightStep;
            }
        }

        for (var i = 0; i < sides; ++i)
        {
            var next = (i + 1) * (heightSegments + 1);
            for (var j = 0; j < heightSegments; ++j)
            {
                // Outer
                tris[ti]     = triVert;
                tris[ti + 1] = triVert + 1;
                tris[ti + 2] = next;
                tris[ti + 3] = next;
                tris[ti + 4] = triVert + 1;
                tris[ti + 5] = next + 1;
                // Inner (reversed)
                tris[ti + innerTriOff]     = next + innerOff;
                tris[ti + innerTriOff + 1] = triVert + innerOff + 1;
                tris[ti + innerTriOff + 2] = triVert + innerOff;
                tris[ti + innerTriOff + 3] = next + innerOff + 1;
                tris[ti + innerTriOff + 4] = triVert + innerOff + 1;
                tris[ti + innerTriOff + 5] = next + innerOff;
                ti += 6;
                triVert++;
                next++;
            }
            triVert++;
        }

        // Caps
        int capVi = bodyVerts;
        int capTi = bodyTris;
        int downOff = capVerts / 2;
        int downTriOff = capTris / 2;
        int capTriVert = capVi;

        for (var i = 0; i <= sides; ++i)
        {
            var angle = (float)i / sides * MathF.PI * 2f;
            var dir = new Vector3(MathF.Cos(angle), 0f, MathF.Sin(angle)).normalized;
            var uvDir = new Vector2(dir.x * 0.5f, dir.z * 0.5f);
            var uvCenter = new Vector2(0.5f, 0.5f);
            var uvRatio = innerRadius / outerRadius;

            // Top
            verts[capVi] = dir * innerRadius + new Vector3(0, height, 0) + pivotOff;
            normals[capVi] = Vector3.up;
            uvs[capVi] = uvCenter + uvDir * uvRatio;
            verts[capVi + 1] = dir * outerRadius + new Vector3(0, height, 0) + pivotOff;
            normals[capVi + 1] = Vector3.up;
            uvs[capVi + 1] = uvCenter + uvDir;
            // Bottom
            verts[capVi + downOff] = dir * innerRadius + pivotOff;
            normals[capVi + downOff] = Vector3.down;
            uvs[capVi + downOff] = uvCenter + uvDir * uvRatio;
            verts[capVi + downOff + 1] = dir * outerRadius + pivotOff;
            normals[capVi + downOff + 1] = Vector3.down;
            uvs[capVi + downOff + 1] = uvCenter + uvDir;
            capVi += 2;
        }

        for (var i = 0; i < sides; ++i)
        {
            int next = bodyVerts + (i + 1) * 2;
            int nextDown = bodyVerts + downOff + (i + 1) * 2;

            // Top
            tris[capTi]     = next;
            tris[capTi + 1] = capTriVert + 1;
            tris[capTi + 2] = capTriVert;
            tris[capTi + 3] = next + 1;
            tris[capTi + 4] = capTriVert + 1;
            tris[capTi + 5] = next;
            // Bottom
            tris[capTi + downTriOff]     = capTriVert + downOff;
            tris[capTi + downTriOff + 1] = capTriVert + downOff + 1;
            tris[capTi + downTriOff + 2] = nextDown;
            tris[capTi + downTriOff + 3] = nextDown;
            tris[capTi + downTriOff + 4] = capTriVert + downOff + 1;
            tris[capTi + downTriOff + 5] = nextDown + 1;

            capTi += 6;
            capTriVert += 2;
        }

        Mesh mesh = new()
        {
            vertices = verts,
            normals = normals,
            uv = uvs,
            triangles = tris
        };
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        return mesh;
    }
}
