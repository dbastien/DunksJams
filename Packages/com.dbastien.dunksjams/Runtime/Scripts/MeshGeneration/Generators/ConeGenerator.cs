using System;
using UnityEngine;

/// <summary>
/// Generates a cone or cylinder mesh. Set radius0 == radius1 for a cylinder.
/// </summary>
public static class ConeGenerator
{
    public static Mesh Generate(
        float radius0 = 0.5f, float radius1 = 0f, float height = 1f,
        int sides = 16, int heightSegments = 1,
        NormalsType normalsType = NormalsType.Vertex,
        PivotPosition pivot = PivotPosition.Bottom)
    {
        sides = Mathf.Max(sides, 3);
        heightSegments = Mathf.Max(heightSegments, 1);

        int bodyVerts = (sides + 1) * (heightSegments + 1);
        int capVerts = 2 * (sides + 1);
        int bodyTris = sides * 6 * heightSegments;
        int capTris = 2 * 3 * sides;

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

        var coneH = (new Vector3(radius0, 0f, 0f) - new Vector3(radius1, height, 0f)).magnitude;
        var heightStep = coneH / heightSegments;
        int vi = 0, ti = 0, triVert = 0;

        // Body
        for (var i = 0; i <= sides; ++i)
        {
            var angle = (float)i / sides * MathF.PI * 2f;
            var dir = new Vector3(MathF.Cos(angle), 0f, MathF.Sin(angle)).normalized;
            var up = (new Vector3(0f, height, 0f) + dir * radius1 - dir * radius0).normalized;
            var h = 0f;

            for (var j = 0; j <= heightSegments; ++j)
            {
                verts[vi] = dir * radius0 + up * h + pivotOff;
                normals[vi] = dir;
                uvs[vi] = new Vector2((float)i / sides, (float)j / heightSegments);
                vi++;
                h += heightStep;
            }
        }

        for (var i = 0; i < sides; ++i)
        {
            var next = (i + 1) * (heightSegments + 1);
            for (var j = 0; j < heightSegments; ++j)
            {
                tris[ti]     = triVert;
                tris[ti + 1] = triVert + 1;
                tris[ti + 2] = next;
                tris[ti + 3] = next;
                tris[ti + 4] = triVert + 1;
                tris[ti + 5] = next + 1;
                ti += 6;
                triVert++;
                next++;
            }
            triVert++;
        }

        // Caps
        int capVi = bodyVerts;
        int capTi = bodyTris;
        int capTriVert = capVi;

        for (var i = 0; i < sides; ++i)
        {
            var angle = (float)i / sides * MathF.PI * 2f;
            var dir = new Vector3(MathF.Cos(angle), 0f, MathF.Sin(angle)).normalized;
            var uvV = new Vector2(dir.x * 0.5f, dir.z * 0.5f);
            var uvCenter = new Vector2(0.5f, 0.5f);

            verts[capVi]     = dir * radius0 + pivotOff;
            normals[capVi]   = Vector3.down;
            uvs[capVi]       = uvCenter + uvV;

            verts[capVi + 1]   = dir * radius1 + new Vector3(0, height, 0) + pivotOff;
            normals[capVi + 1] = Vector3.up;
            uvs[capVi + 1]     = uvCenter + uvV;
            capVi += 2;
        }

        verts[capVi]     = pivotOff;
        normals[capVi]   = Vector3.down;
        uvs[capVi]       = new Vector2(0.5f, 0.5f);
        verts[capVi + 1]   = new Vector3(0, height, 0) + pivotOff;
        normals[capVi + 1] = Vector3.up;
        uvs[capVi + 1]     = new Vector2(0.5f, 0.5f);
        capVi += 2;

        for (var i = 0; i < sides; ++i)
        {
            tris[capTi]     = capTriVert;
            tris[capTi + 2] = capVi - 2;
            tris[capTi + 3] = capTriVert + 1;
            tris[capTi + 4] = capVi - 1;

            if (i == sides - 1)
            {
                tris[capTi + 1] = bodyVerts;
                tris[capTi + 5] = bodyVerts + 1;
            }
            else
            {
                tris[capTi + 1] = capTriVert + 2;
                tris[capTi + 5] = capTriVert + 3;
            }

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
