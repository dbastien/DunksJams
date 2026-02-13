using UnityEngine;

public interface IFilter2D
{
    Vector2 CurrentValue { get; }

    void Update(Vector2 s);

    void Reset();
}