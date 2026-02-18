using UnityEngine;

public interface IShape2D
{
    public bool Contains(Vector2 p);
    public Vector2 NearestPoint(Vector2 p);
    public void DrawGizmos();
}