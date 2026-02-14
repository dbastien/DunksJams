using System.Collections.Generic;

public class CSGPolygon
{
    public List<CSGVertex> Vertices;
    public CSGPlane Plane;
    public int Id;

    public CSGPolygon(int id, List<CSGVertex> vertices)
    {
        Vertices = vertices;
        Plane = new CSGPlane(vertices[0].Position, vertices[1].Position, vertices[2].Position);
        Id = id;
    }

    public CSGPolygon(int id, params CSGVertex[] vertices)
    {
        Vertices = new List<CSGVertex>(vertices);
        Plane = new CSGPlane(vertices[0].Position, vertices[1].Position, vertices[2].Position);
        Id = id;
    }

    public CSGPolygon(CSGPolygon other)
    {
        Vertices = new List<CSGVertex>(other.Vertices.Count);
        foreach (var v in other.Vertices) Vertices.Add(new CSGVertex(v));
        Plane = new CSGPlane(other.Plane);
        Id = other.Id;
    }

    public void Flip()
    {
        Vertices.Reverse();
        foreach (var v in Vertices) v.Flip();
    }
}
