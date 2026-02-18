using System.Collections.Generic;

/// <summary>
/// BSP tree node for CSG operations.
/// Based on Evan Wallace's csg.js (MIT license).
/// </summary>
public class CSGNode
{
    private List<CSGPolygon> _polygons = new();
    private CSGPlane _plane;
    private CSGNode _front;
    private CSGNode _back;

    public void Invert()
    {
        foreach (CSGPolygon p in _polygons) p.Flip();
        _plane?.Flip();
        _front?.Invert();
        _back?.Invert();
        (_front, _back) = (_back, _front);
    }

    private List<CSGPolygon> ClipPolygons(List<CSGPolygon> polygons)
    {
        if (_plane == null) return new List<CSGPolygon>(polygons);

        var frontList = new List<CSGPolygon>();
        var backList = new List<CSGPolygon>();

        foreach (CSGPolygon p in polygons)
            _plane.Split(p, frontList, backList, frontList, backList);

        frontList = _front != null ? _front.ClipPolygons(frontList) : frontList;
        backList = _back != null ? _back.ClipPolygons(backList) : new List<CSGPolygon>();

        frontList.AddRange(backList);
        return frontList;
    }

    public void ClipTo(CSGNode node)
    {
        _polygons = node.ClipPolygons(_polygons);
        _front?.ClipTo(node);
        _back?.ClipTo(node);
    }

    public List<CSGPolygon> AllPolygons()
    {
        var list = new List<CSGPolygon>(_polygons);
        if (_front != null) list.AddRange(_front.AllPolygons());
        if (_back != null) list.AddRange(_back.AllPolygons());
        return list;
    }

    public void Build(List<CSGPolygon> polygons)
    {
        if (polygons.Count == 0) return;
        _plane ??= new CSGPlane(polygons[0].Plane);

        var frontList = new List<CSGPolygon>();
        var backList = new List<CSGPolygon>();

        foreach (CSGPolygon p in polygons)
            _plane.Split(p, _polygons, _polygons, frontList, backList);

        if (frontList.Count > 0)
        {
            _front ??= new CSGNode();
            _front.Build(frontList);
        }

        if (backList.Count > 0)
        {
            _back ??= new CSGNode();
            _back.Build(backList);
        }
    }
}