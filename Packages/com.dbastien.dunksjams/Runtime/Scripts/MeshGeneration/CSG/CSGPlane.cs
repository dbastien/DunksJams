using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BSP splitting plane for CSG operations.
/// Based on Evan Wallace's csg.js (MIT license).
/// </summary>
public class CSGPlane
{
    const float _epsilon = 1e-6f;

    public Vector3 Normal;
    public float Distance;

    public CSGPlane(Vector3 a, Vector3 b, Vector3 c)
    {
        Normal = Vector3.Cross(b - a, c - a).normalized;
        Distance = Vector3.Dot(Normal, a);
    }

    public CSGPlane(CSGPlane other) { Normal = other.Normal; Distance = other.Distance; }

    public void Flip() { Normal = -Normal; Distance = -Distance; }

    [Flags]
    enum PointClass { Coplanar = 0, Front = 1, Back = 2, Spanning = 3 }

    public void Split(CSGPolygon polygon,
        List<CSGPolygon> coplanarFront, List<CSGPolygon> coplanarBack,
        List<CSGPolygon> front, List<CSGPolygon> back)
    {
        var count = polygon.Vertices.Count;
        var classes = new PointClass[count];
        var polyClass = PointClass.Coplanar;

        for (var i = 0; i < count; ++i)
        {
            var t = Vector3.Dot(Normal, polygon.Vertices[i].Position) - Distance;
            var c = t < -_epsilon ? PointClass.Back : t > _epsilon ? PointClass.Front : PointClass.Coplanar;
            polyClass |= c;
            classes[i] = c;
        }

        switch (polyClass)
        {
            case PointClass.Coplanar:
                (Vector3.Dot(Normal, polygon.Plane.Normal) > 0 ? coplanarFront : coplanarBack).Add(polygon);
                break;
            case PointClass.Front:
                front.Add(polygon);
                break;
            case PointClass.Back:
                back.Add(polygon);
                break;
            default: // Spanning
                var frontList = new List<CSGVertex>(4);
                var backList = new List<CSGVertex>(4);

                for (var i = 0; i < count; ++i)
                {
                    var j = (i + 1) % count;
                    var ci = classes[i]; var cj = classes[j];
                    var vi = polygon.Vertices[i]; var vj = polygon.Vertices[j];

                    if (ci != PointClass.Back) frontList.Add(vi);
                    if (ci != PointClass.Front) backList.Add(ci != PointClass.Back ? new CSGVertex(vi) : vi);

                    if ((ci | cj) == PointClass.Spanning)
                    {
                        var t = (Distance - Vector3.Dot(Normal, vi.Position)) /
                                Vector3.Dot(Normal, vj.Position - vi.Position);
                        var mid = CSGVertex.Lerp(vi, vj, t);
                        frontList.Add(mid);
                        backList.Add(new CSGVertex(mid));
                    }
                }

                if (frontList.Count >= 3) front.Add(new CSGPolygon(polygon.Id, frontList));
                if (backList.Count >= 3) back.Add(new CSGPolygon(polygon.Id, backList));
                break;
        }
    }
}
