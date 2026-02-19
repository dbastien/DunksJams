using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Match3Level", menuName = "Game/Match3/Level Data")]
public class Match3LevelData : ScriptableObject
{
    [System.Serializable]
    public class TileDefinition
    {
        public Match3Tile.TileType Type = Match3Tile.TileType.Normal;
        public Match3Tile.ColorType Color = Match3Tile.ColorType.Red;

        public TileDefinition() { }

        public TileDefinition(Match3Tile.TileType type, Match3Tile.ColorType color)
        {
            Type = type;
            Color = color;
        }
    }

    [System.Serializable]
    public class WinCondition
    {
        public enum ConditionType
        {
            Score,
            CollectColor,
            ClearBlockers,
            SurviveMoves
        }

        public ConditionType Type;
        public int TargetValue;
        public Match3Tile.ColorType TargetColor; // For CollectColor type

        public string GetDescription()
        {
            return Type switch
            {
                ConditionType.Score => $"Reach {TargetValue} points",
                ConditionType.CollectColor => $"Collect {TargetValue} {TargetColor} tiles",
                ConditionType.ClearBlockers => $"Clear {TargetValue} blockers",
                ConditionType.SurviveMoves => $"Survive {TargetValue} moves",
                _ => "Unknown objective"
            };
        }
    }

    [Header("Grid Configuration")]
    [Range(4, 10)] public int Width = 8;
    [Range(4, 12)] public int Height = 8;

    [Header("Game Rules")]
    [Range(1, 100)] public int MaxMoves = 20;
    [Range(3, 10)] public int MinMatchSize = 3;

    [Header("Win Conditions")]
    public WinCondition[] WinConditions = new WinCondition[]
    {
        new WinCondition { Type = WinCondition.ConditionType.Score, TargetValue = 1000 }
    };

    [Header("Tile Configuration")]
    [Tooltip("Leave empty to generate random tiles")]
    public TileDefinition[] InitialTiles;

    [Range(0f, 1f)] public float BlockerSpawnChance = 0.1f;
    [Range(0f, 1f)] public float CollectibleSpawnChance = 0.05f;

    [Header("Scoring")]
    public int PointsPerTile = 10;
    public int ComboMultiplierBonus = 50;

    [Header("Available Colors")]
    public Match3Tile.ColorType[] AvailableColors = new Match3Tile.ColorType[]
    {
        Match3Tile.ColorType.Red,
        Match3Tile.ColorType.Blue,
        Match3Tile.ColorType.Green,
        Match3Tile.ColorType.Yellow
    };

    private void OnValidate()
    {
        // Ensure InitialTiles matches grid size if specified
        if (InitialTiles != null && InitialTiles.Length > 0)
        {
            int expectedSize = Width * Height;
            if (InitialTiles.Length != expectedSize)
            {
                DLog.LogW($"InitialTiles size ({InitialTiles.Length}) doesn't match grid size ({expectedSize})");
            }
        }

        // Ensure at least one color is available
        if (AvailableColors == null || AvailableColors.Length == 0)
        {
            AvailableColors = new Match3Tile.ColorType[]
            {
                Match3Tile.ColorType.Red,
                Match3Tile.ColorType.Blue,
                Match3Tile.ColorType.Green
            };
        }
    }

    public TileDefinition GetTileDefinition(int x, int y)
    {
        if (InitialTiles == null || InitialTiles.Length == 0)
            return null;

        int index = x + y * Width;
        if (index < 0 || index >= InitialTiles.Length)
            return null;

        return InitialTiles[index];
    }

    public Match3Tile.ColorType GetRandomColor()
    {
        if (AvailableColors == null || AvailableColors.Length == 0)
            return Match3Tile.ColorType.Red;

        return AvailableColors[UnityEngine.Random.Range(0, AvailableColors.Length)];
    }
}
