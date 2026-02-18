public interface IFilter1D
{
    public float CurrentValue { get; }

    public void Update(float s);

    public void Reset();
}