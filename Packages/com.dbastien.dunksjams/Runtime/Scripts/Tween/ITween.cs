public interface ITween
{
    public void Update(float deltaTime);
    public bool IsComplete { get; }
    public void Pause();
    public void Resume();
    public void Rewind();
    public void Restart();
    public void Kill();
    public string Id { get; }
    public string Tag { get; }
    public bool IgnoreTimeScale { get; }
    public float TimeScale { get; }
    public float Duration { get; }
}