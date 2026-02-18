using UnityEngine;

public static class PyramidGenerator
{
    public static Mesh Generate
    (
        float baseSize = 1f, float height = 1f,
        PivotPosition pivot = PivotPosition.Bottom
    )
    {
        float h = height;
        float s = baseSize / 2f;
        Vector3 pivotOff = pivot switch
        {
            PivotPosition.Center => new Vector3(0f, -h / 2f, 0f),
            PivotPosition.Top => new Vector3(0f, -h, 0f),
            _ => Vector3.zero
        };

        Vector3 apex = new Vector3(0, h, 0) + pivotOff;
        Vector3 bl = new Vector3(-s, 0, -s) + pivotOff;
        Vector3 br = new Vector3(s, 0, -s) + pivotOff;
        Vector3 fr = new Vector3(s, 0, s) + pivotOff;
        Vector3 fl = new Vector3(-s, 0, s) + pivotOff;

        // 16 verts: 4 faces x 3 + base x 4
        var verts = new Vector3[]
        {
            // Front face
            fl, fr, apex,
            // Right face
            fr, br, apex,
            // Back face
            br, bl, apex,
            // Left face
            bl, fl, apex,
            // Base
            bl, br, fr, fl
        };

        var uvs = new Vector2[]
        {
            new(0, 0), new(1, 0), new(0.5f, 1),
            new(0, 0), new(1, 0), new(0.5f, 1),
            new(0, 0), new(1, 0), new(0.5f, 1),
            new(0, 0), new(1, 0), new(0.5f, 1),
            new(0, 0), new(1, 0), new(1, 1), new(0, 1)
        };

        var tris = new[]
        {
            0, 2, 1, // front
            3, 5, 4, // right
            6, 8, 7, // back
            9, 11, 10, // left
            12, 13, 14, 12, 14, 15 // base
        };

        return MeshBuilder.Build(verts, tris, uvs);
    }
}