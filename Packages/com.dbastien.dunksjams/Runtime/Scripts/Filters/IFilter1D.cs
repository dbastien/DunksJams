public interface IFilter1D
{
    float CurrentValue { get; }

    void Update(float s);

    void Reset();
}