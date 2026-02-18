using System;
using UnityEngine;

public static class TorusGenerator
{
    public static Mesh Generate
    (
        float torusRadius = 0.5f, float tubeRadius = 0.2f,
        int torusSegments = 24, int tubeSegments = 12,
        NormalsType normalsType = NormalsType.Vertex,
        PivotPosition pivot = PivotPosition.Center
    )
    {
        torusSegments = Mathf.Max(torusSegments, 3);
        tubeSegments = Mathf.Max(tubeSegments, 3);

        int numVerts = (torusSegments + 1) * (tubeSegments + 1);
        int numTris = 2 * tubeSegments * torusSegments * 3;

        var verts = new Vector3[numVerts];
        var normals = new Vector3[numVerts];
        var uvs = new Vector2[numVerts];
        var tris = new int[numTris];

        var vi = 0;
        for (var i = 0; i <= torusSegments; ++i)
        {
            float theta = i * 2f * MathF.PI / torusSegments;
            var center = new Vector3(MathF.Cos(theta) * torusRadius, 0f, MathF.Sin(theta) * torusRadius);

            for (var j = 0; j <= tubeSegments; ++j)
            {
                float phi = j * 2f * MathF.PI / tubeSegments;
                var localDir = new Vector3(MathF.Cos(theta) * MathF.Cos(phi),
                    MathF.Sin(phi),
                    MathF.Sin(theta) * MathF.Cos(phi));

                verts[vi] = center + localDir * tubeRadius;
                normals[vi] = localDir;
                uvs[vi] = new Vector2((float)i / torusSegments, (float)j / tubeSegments);
                vi++;
            }
        }

        var ti = 0;
        for (var i = 0; i < torusSegments; ++i)
        {
            int cur = i * (tubeSegments + 1);
            int next = (i + 1) * (tubeSegments + 1);
            for (var j = 0; j < tubeSegments; ++j)
            {
                tris[ti] = next + j;
                tris[ti + 1] = cur + j + 1;
                tris[ti + 2] = cur + j;
                tris[ti + 3] = next + j + 1;
                tris[ti + 4] = cur + j + 1;
                tris[ti + 5] = next + j;
                ti += 6;
            }
        }

        if (normalsType == NormalsType.Face)
            MeshBuilder.DuplicateForFlatShading(ref verts, ref uvs, tris);

        Vector3 pivotOff = MeshBuilder.PivotOffset(pivot, tubeRadius * 2f);
        if (pivotOff != Vector3.zero) MeshBuilder.ApplyPivot(verts, pivotOff);

        return normalsType == NormalsType.Vertex
            ? MeshBuilder.Build(verts, tris, uvs, normals)
            : MeshBuilder.Build(verts, tris, uvs);
    }
}