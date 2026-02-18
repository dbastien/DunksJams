using System.Collections.Generic;

public sealed class PongGameDefinition : GameDefinition
{
    public PongSettings Settings { get; } = new();

    public override string Title => "Pong";

    public override ScoreboardSpec Scoreboard => new(new[]
    {
        new ScoreFieldDef("Player", "Player"),
        new ScoreFieldDef("CPU", "CPU")
    });

    public override IReadOnlyList<ConfigOptionDef> ConfigOptions => new ConfigOptionDef[]
    {
        new IntSliderOption("Win Score", () => Settings.WinScore, v => Settings.WinScore = v, 1, 21),
        new FloatSliderOption("Paddle Speed", () => Settings.PaddleSpeed, v => Settings.PaddleSpeed = v, 5f, 20f,
            "0.0"),
        new FloatSliderOption("Ball Speed", () => Settings.BallSpeed, v => Settings.BallSpeed = v, 5f, 25f, "0.0"),
        new BoolOption("AI Opponent", () => Settings.AiOpponent, v => Settings.AiOpponent = v)
    };

    public override string BuildGameOverSummary(GameFlowManager flow)
    {
        if (flow == null) return "Game Over";

        int player = flow.GetScore("Player");
        int cpu = flow.GetScore("CPU");
        string result = player == cpu ? "Tie Game" : player > cpu ? "Player Wins!" : "CPU Wins!";
        return $"{result}\nPlayer: {player}\nCPU: {cpu}";
    }
}