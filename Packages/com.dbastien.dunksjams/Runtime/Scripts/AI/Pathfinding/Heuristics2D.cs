using UnityEngine;

public struct ManhattanHeuristic2D : IHeuristic<Vector2Int>
{
    public float Estimate(Vector2Int from, Vector2Int to) =>
        Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
}

public struct EuclideanHeuristic2D : IHeuristic<Vector2Int>
{
    public float Estimate(Vector2Int from, Vector2Int to)
    {
        float dx = from.x - to.x;
        float dy = from.y - to.y;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }
}

public struct OctileHeuristic2D : IHeuristic<Vector2Int>
{
    const float Sqrt2 = 1.41421356237f;

    public float Estimate(Vector2Int from, Vector2Int to)
    {
        int dx = Mathf.Abs(from.x - to.x);
        int dy = Mathf.Abs(from.y - to.y);
        return dx + dy + (Sqrt2 - 2f) * Mathf.Min(dx, dy);
    }
}

public struct ChebyshevHeuristic2D : IHeuristic<Vector2Int>
{
    public float Estimate(Vector2Int from, Vector2Int to) =>
        Mathf.Max(Mathf.Abs(from.x - to.x), Mathf.Abs(from.y - to.y));
}
