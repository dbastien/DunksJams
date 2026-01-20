public interface INoiseGenerator
{
    float GetValue(float x);
    float GetValue(float x, float y);
    float GetValue(float x, float y, float z);
}