public sealed class PongSettings
{
    public int WinScore { get; set; } = 5;
    public float PaddleSpeed { get; set; } = 12f;
    public float BallSpeed { get; set; } = 12f;
    public bool AiOpponent { get; set; } = true;
}