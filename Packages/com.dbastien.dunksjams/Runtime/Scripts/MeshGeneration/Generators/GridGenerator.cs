using UnityEngine;

public static class GridGenerator
{
    public static Mesh Generate
    (
        float width = 1f, float depth = 1f, int resX = 10, int resZ = 10,
        PivotPosition pivot = PivotPosition.Center
    )
    {
        resX = Mathf.Max(resX, 1);
        resZ = Mathf.Max(resZ, 1);

        var verts = new Vector3[(resX + 1) * (resZ + 1)];
        var uvs = new Vector2[(resX + 1) * (resZ + 1)];
        var tris = new int[resX * resZ * 6];

        float stepX = width / resX;
        float stepZ = depth / resZ;
        Vector3 offset = pivot switch
        {
            PivotPosition.Center => new Vector3(-width / 2f, 0f, -depth / 2f),
            _ => Vector3.zero
        };

        for (int z = 0, i = 0; z <= resZ; ++z)
        for (var x = 0; x <= resX; ++x, ++i)
        {
            verts[i] = new Vector3(x * stepX, 0f, z * stepZ) + offset;
            uvs[i] = new Vector2((float)x / resX, (float)z / resZ);
        }

        for (int z = 0, ti = 0, vi = 0; z < resZ; ++z, ++vi)
        for (var x = 0; x < resX; ++x, ti += 6, ++vi)
        {
            tris[ti] = vi;
            tris[ti + 1] = vi + resX + 1;
            tris[ti + 2] = vi + 1;
            tris[ti + 3] = vi + resX + 1;
            tris[ti + 4] = vi + resX + 2;
            tris[ti + 5] = vi + 1;
        }

        return MeshBuilder.Build(verts, tris, uvs);
    }
}