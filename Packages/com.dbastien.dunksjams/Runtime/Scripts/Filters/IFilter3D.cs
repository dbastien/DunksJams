using UnityEngine;

public interface IFilter3D
{
    public Vector3 CurrentValue { get; }

    public void Update(Vector3 s);

    public void Reset();
}