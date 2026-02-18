using UnityEngine;

public interface IFilter2D
{
    public Vector2 CurrentValue { get; }

    public void Update(Vector2 s);

    public void Reset();
}