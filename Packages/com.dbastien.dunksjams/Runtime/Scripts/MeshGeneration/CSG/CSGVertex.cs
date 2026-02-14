using UnityEngine;

public class CSGVertex
{
    public Vector3 Position;
    public Vector3 Normal;
    public Vector2 UV;

    public CSGVertex(Vector3 position, Vector3 normal, Vector2 uv)
    {
        Position = position;
        Normal = normal;
        UV = uv;
    }

    public CSGVertex(CSGVertex other)
    {
        Position = other.Position;
        Normal = other.Normal;
        UV = other.UV;
    }

    public void Flip() => Normal = -Normal;

    public static CSGVertex Lerp(CSGVertex a, CSGVertex b, float t) => new(
        Vector3.Lerp(a.Position, b.Position, t),
        Vector3.Lerp(a.Normal, b.Normal, t),
        Vector2.Lerp(a.UV, b.UV, t)
    );
}
