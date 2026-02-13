using UnityEngine;

public interface IFilter3D
{
    Vector3 CurrentValue { get; }

    void Update(Vector3 s);

    void Reset();
}