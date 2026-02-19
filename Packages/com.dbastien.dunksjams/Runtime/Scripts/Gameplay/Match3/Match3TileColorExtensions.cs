using UnityEngine;

public static class Match3TileColorExtensions
{
    public static Color ToColor(this Match3Tile.ColorType colorType)
    {
        return colorType switch
        {
            Match3Tile.ColorType.Red => new Color(0.9f, 0.2f, 0.2f),
            Match3Tile.ColorType.Blue => new Color(0.2f, 0.4f, 0.9f),
            Match3Tile.ColorType.Green => new Color(0.2f, 0.8f, 0.3f),
            Match3Tile.ColorType.Yellow => new Color(0.95f, 0.9f, 0.2f),
            Match3Tile.ColorType.Purple => new Color(0.7f, 0.3f, 0.8f),
            Match3Tile.ColorType.Orange => new Color(0.95f, 0.6f, 0.2f),
            _ => Color.white
        };
    }
}
