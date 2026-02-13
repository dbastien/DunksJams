public interface ITween
{
    void Update(float deltaTime);
    bool IsComplete { get; }
    void Pause();
    void Resume();
    void Rewind();
    void Restart();
    void Kill();
    string Id { get; }
    string Tag { get; }
    bool IgnoreTimeScale { get; }
    float TimeScale { get; }
    float Duration { get; }
}