using UnityEngine;

public abstract class IncrementalAttribute : PropertyAttribute
{
}

public class FloatIncrementalAttribute : IncrementalAttribute
{
    public float Increment { get; }
    public FloatIncrementalAttribute(float increment) => Increment = increment;
}

public class IntIncrementalAttribute : IncrementalAttribute
{
    public int Increment { get; }
    public IntIncrementalAttribute(int increment) => Increment = increment;
}