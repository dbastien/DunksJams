using System;
using UnityEngine;

/// <summary>
/// Generates an arc shape -- a box-like base with a bezier-curved top surface.
/// </summary>
public static class ArcGenerator
{
    public static Mesh Generate(
        float width = 1f, float height = 1f, float depth = 1f,
        int arcSegments = 8, Vector3? controlPoint = null,
        PivotPosition pivot = PivotPosition.Bottom)
    {
        arcSegments = Mathf.Max(arcSegments, 1);
        var cp = controlPoint ?? new Vector3(0f, height * 1.5f, 0f);

        var pivotOff = pivot switch
        {
            PivotPosition.Center => new Vector3(0f, -height / 2f, 0f),
            PivotPosition.Top    => new Vector3(0f, -height, 0f),
            _                    => Vector3.zero
        };

        var hw = width / 2f; var hd = depth / 2f;

        // Simplified arc: base quad + arc surface + side walls
        // Generate the top arc curve points
        var frontCurve = new Vector3[arcSegments + 1];
        var backCurve = new Vector3[arcSegments + 1];
        var p0Front = new Vector3(-hw, height, -hd);
        var p1Front = new Vector3( hw, height, -hd);
        var p0Back  = new Vector3(-hw, height,  hd);
        var p1Back  = new Vector3( hw, height,  hd);

        for (var i = 0; i <= arcSegments; ++i)
        {
            var t = (float)i / arcSegments;
            frontCurve[i] = BezierQuadratic(p0Front, p1Front, new Vector3(cp.x, cp.y, -hd), t) + pivotOff;
            backCurve[i]  = BezierQuadratic(p0Back, p1Back, new Vector3(cp.x, cp.y, hd), t) + pivotOff;
        }

        // Top surface: arcSegments quads
        int topVerts = (arcSegments + 1) * 2;
        int topTris = arcSegments * 6;
        // Base: 1 quad (4 verts, 6 tris)
        // Left wall: 1 quad, Right wall: 1 quad, Front/Back walls: arcSegments quads each
        int totalVerts = topVerts + 4 + 4 + 4 + (arcSegments + 1) * 2 + (arcSegments + 1) * 2;
        int totalTris = topTris + 6 + 6 + 6 + arcSegments * 6 + arcSegments * 6;

        var verts = new Vector3[totalVerts];
        var uvs = new Vector2[totalVerts];
        var tris = new int[totalTris];
        int vi = 0, ti = 0;

        // Top surface
        for (var i = 0; i <= arcSegments; ++i)
        {
            float u = (float)i / arcSegments;
            verts[vi] = frontCurve[i]; uvs[vi] = new Vector2(u, 0f); vi++;
            verts[vi] = backCurve[i];  uvs[vi] = new Vector2(u, 1f); vi++;
        }
        for (var i = 0; i < arcSegments; ++i)
        {
            int b = i * 2;
            tris[ti] = b; tris[ti+1] = b+1; tris[ti+2] = b+3;
            tris[ti+3] = b+3; tris[ti+4] = b+2; tris[ti+5] = b;
            ti += 6;
        }

        // Base quad
        var bl = new Vector3(-hw, 0, -hd) + pivotOff;
        var br = new Vector3( hw, 0, -hd) + pivotOff;
        var fr = new Vector3( hw, 0,  hd) + pivotOff;
        var fl = new Vector3(-hw, 0,  hd) + pivotOff;
        int bv = vi;
        verts[vi] = bl; uvs[vi] = new Vector2(0,0); vi++;
        verts[vi] = br; uvs[vi] = new Vector2(1,0); vi++;
        verts[vi] = fr; uvs[vi] = new Vector2(1,1); vi++;
        verts[vi] = fl; uvs[vi] = new Vector2(0,1); vi++;
        tris[ti] = bv; tris[ti+1] = bv+1; tris[ti+2] = bv+2;
        tris[ti+3] = bv; tris[ti+4] = bv+2; tris[ti+5] = bv+3;
        ti += 6;

        // Left wall
        bv = vi;
        verts[vi] = bl; uvs[vi] = new Vector2(0,0); vi++;
        verts[vi] = fl; uvs[vi] = new Vector2(1,0); vi++;
        verts[vi] = frontCurve[0]; uvs[vi] = new Vector2(0,1); vi++;
        verts[vi] = backCurve[0]; uvs[vi] = new Vector2(1,1); vi++;
        tris[ti] = bv+3; tris[ti+1] = bv+2; tris[ti+2] = bv;
        tris[ti+3] = bv+1; tris[ti+4] = bv+3; tris[ti+5] = bv;
        ti += 6;

        // Right wall
        bv = vi;
        verts[vi] = br; uvs[vi] = new Vector2(0,0); vi++;
        verts[vi] = fr; uvs[vi] = new Vector2(1,0); vi++;
        verts[vi] = frontCurve[arcSegments]; uvs[vi] = new Vector2(0,1); vi++;
        verts[vi] = backCurve[arcSegments]; uvs[vi] = new Vector2(1,1); vi++;
        tris[ti] = bv; tris[ti+1] = bv+2; tris[ti+2] = bv+3;
        tris[ti+3] = bv; tris[ti+4] = bv+3; tris[ti+5] = bv+1;
        ti += 6;

        // Front wall (base bottom to arc top)
        bv = vi;
        for (var i = 0; i <= arcSegments; ++i)
        {
            float u = (float)i / arcSegments;
            var bottom = Vector3.Lerp(bl, br, u);
            verts[vi] = bottom; uvs[vi] = new Vector2(u, 0f); vi++;
            verts[vi] = frontCurve[i]; uvs[vi] = new Vector2(u, 1f); vi++;
        }
        for (var i = 0; i < arcSegments; ++i)
        {
            int b2 = bv + i * 2;
            tris[ti] = b2; tris[ti+1] = b2+2; tris[ti+2] = b2+1;
            tris[ti+3] = b2+2; tris[ti+4] = b2+3; tris[ti+5] = b2+1;
            ti += 6;
        }

        // Back wall
        bv = vi;
        for (var i = 0; i <= arcSegments; ++i)
        {
            float u = (float)i / arcSegments;
            var bottom = Vector3.Lerp(fl, fr, u);
            verts[vi] = bottom; uvs[vi] = new Vector2(u, 0f); vi++;
            verts[vi] = backCurve[i]; uvs[vi] = new Vector2(u, 1f); vi++;
        }
        for (var i = 0; i < arcSegments; ++i)
        {
            int b2 = bv + i * 2;
            tris[ti] = b2+1; tris[ti+1] = b2+2; tris[ti+2] = b2;
            tris[ti+3] = b2+1; tris[ti+4] = b2+3; tris[ti+5] = b2+2;
            ti += 6;
        }

        return MeshBuilder.Build(verts, tris, uvs);
    }

    static Vector3 BezierQuadratic(Vector3 p0, Vector3 p1, Vector3 cp, float t)
    {
        var oneMinusT = 1f - t;
        return oneMinusT * oneMinusT * p0 + 2f * oneMinusT * t * cp + t * t * p1;
    }
}
