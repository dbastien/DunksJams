using System.Collections.Generic;
using UnityEngine;

public sealed class Match3GameDefinition : GameDefinition
{
    public Match3LevelData CurrentLevel { get; set; }
    private Match3Manager _manager;

    public override string Title => "Match 3";

    public override ScoreboardSpec Scoreboard => new(new[]
    {
        new ScoreFieldDef("Score", "Score", 0),
        new ScoreFieldDef("Moves", "Moves", 0)
    });

    public override IReadOnlyList<ConfigOptionDef> ConfigOptions => new ConfigOptionDef[]
    {
        new IntSliderOption("Grid Width", () => GetGridWidth(), v => SetGridWidth(v), 4, 10),
        new IntSliderOption("Grid Height", () => GetGridHeight(), v => SetGridHeight(v), 4, 12),
        new IntSliderOption("Max Moves", () => GetMaxMoves(), v => SetMaxMoves(v), 10, 50)
    };

    public override void OnGameStart(GameFlowManager flow)
    {
        // Find or create Match3Manager
        _manager = Object.FindObjectOfType<Match3Manager>();

        if (_manager == null)
        {
            DLog.LogW("Match3GameDefinition: No Match3Manager found in scene");
            return;
        }

        // Load default level if none set
        if (CurrentLevel == null)
        {
            CurrentLevel = CreateDefaultLevel();
        }

        // Initialize the level
        _manager.InitializeLevel(CurrentLevel);

        // Initialize score display
        if (flow != null)
        {
            flow.SetScore("Score", 0);
            flow.SetScore("Moves", CurrentLevel.MaxMoves);
        }
    }

    public override void OnGameEnd(GameFlowManager flow)
    {
        if (_manager != null)
            _manager.CleanupLevel();
    }

    public override void OnGameRestart(GameFlowManager flow)
    {
        if (_manager != null && CurrentLevel != null)
        {
            _manager.CleanupLevel();
            _manager.InitializeLevel(CurrentLevel);
        }
    }

    public override string BuildGameOverSummary(GameFlowManager flow)
    {
        if (flow == null) return "Game Over";

        int score = flow.GetScore("Score");
        int movesRemaining = flow.GetScore("Moves");

        if (movesRemaining > 0)
            return $"Victory!\n\nFinal Score: {score}\nMoves Remaining: {movesRemaining}";
        else
            return $"Out of Moves!\n\nFinal Score: {score}";
    }

    private Match3LevelData CreateDefaultLevel()
    {
        var level = ScriptableObject.CreateInstance<Match3LevelData>();
        level.Width = 8;
        level.Height = 8;
        level.MaxMoves = 20;
        level.MinMatchSize = 3;

        level.WinConditions = new Match3LevelData.WinCondition[]
        {
            new Match3LevelData.WinCondition
            {
                Type = Match3LevelData.WinCondition.ConditionType.Score,
                TargetValue = 1000
            }
        };

        level.AvailableColors = new Match3Tile.ColorType[]
        {
            Match3Tile.ColorType.Red,
            Match3Tile.ColorType.Blue,
            Match3Tile.ColorType.Green,
            Match3Tile.ColorType.Yellow,
            Match3Tile.ColorType.Purple
        };

        level.PointsPerTile = 10;
        level.ComboMultiplierBonus = 50;

        return level;
    }

    // Config option getters/setters
    private int GetGridWidth() => CurrentLevel?.Width ?? 8;
    private void SetGridWidth(int value)
    {
        if (CurrentLevel != null)
            CurrentLevel.Width = value;
    }

    private int GetGridHeight() => CurrentLevel?.Height ?? 8;
    private void SetGridHeight(int value)
    {
        if (CurrentLevel != null)
            CurrentLevel.Height = value;
    }

    private int GetMaxMoves() => CurrentLevel?.MaxMoves ?? 20;
    private void SetMaxMoves(int value)
    {
        if (CurrentLevel != null)
            CurrentLevel.MaxMoves = value;
    }
}
