using UnityEngine;

public static class BoxGenerator
{
    public static Mesh Generate
    (
        float width = 1f, float height = 1f, float depth = 1f,
        int widthSegs = 1, int heightSegs = 1, int depthSegs = 1,
        PivotPosition pivot = PivotPosition.Center
    )
    {
        int numTris = (widthSegs * depthSegs + widthSegs * heightSegs + depthSegs * heightSegs) * 12;
        int numVerts = ((widthSegs + 1) * (depthSegs + 1) +
                        (widthSegs + 1) * (heightSegs + 1) +
                        (depthSegs + 1) * (heightSegs + 1)) *
                       2;

        var verts = new Vector3[numVerts];
        var uvs = new Vector2[numVerts];
        var tris = new int[numTris];

        Vector3 pivotOff = MeshBuilder.PivotOffset(pivot, height);
        float hw = width / 2f;
        float hh = height / 2f;
        float hd = depth / 2f;

        Vector3 a0 = new Vector3(-hw, -hh, -hd) + pivotOff;
        Vector3 b0 = new Vector3(-hw, -hh, hd) + pivotOff;
        Vector3 c0 = new Vector3(hw, -hh, hd) + pivotOff;
        Vector3 d0 = new Vector3(hw, -hh, -hd) + pivotOff;
        Vector3 a1 = new Vector3(-hw, hh, -hd) + pivotOff;
        Vector3 b1 = new Vector3(-hw, hh, hd) + pivotOff;
        Vector3 c1 = new Vector3(hw, hh, hd) + pivotOff;
        Vector3 d1 = new Vector3(hw, hh, -hd) + pivotOff;

        int vi = 0, ti = 0;
        MeshBuilder.GeneratePlane(a0, b0, c0, d0, widthSegs, depthSegs, verts, uvs, tris, ref vi, ref ti);
        MeshBuilder.GeneratePlane(b1, a1, d1, c1, widthSegs, depthSegs, verts, uvs, tris, ref vi, ref ti);
        MeshBuilder.GeneratePlane(b0, b1, c1, c0, widthSegs, heightSegs, verts, uvs, tris, ref vi, ref ti);
        MeshBuilder.GeneratePlane(d0, d1, a1, a0, widthSegs, heightSegs, verts, uvs, tris, ref vi, ref ti);
        MeshBuilder.GeneratePlane(a0, a1, b1, b0, depthSegs, heightSegs, verts, uvs, tris, ref vi, ref ti);
        MeshBuilder.GeneratePlane(c0, c1, d1, d0, depthSegs, heightSegs, verts, uvs, tris, ref vi, ref ti);

        return MeshBuilder.Build(verts, tris, uvs);
    }
}