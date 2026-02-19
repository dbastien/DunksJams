using System.Collections.Generic;
using UnityEngine;

public class Match3MatchDetector
{
    public class Match
    {
        public List<Vector2Int> Positions { get; }
        public Match3Tile.ColorType Color { get; }
        public MatchType Type { get; set; }

        public Match(Match3Tile.ColorType color, MatchType type = MatchType.Normal)
        {
            Positions = new List<Vector2Int>();
            Color = color;
            Type = type;
        }

        public void AddPosition(Vector2Int pos)
        {
            if (!Positions.Contains(pos))
                Positions.Add(pos);
        }

        public void AddPositions(IEnumerable<Vector2Int> positions)
        {
            foreach (Vector2Int pos in positions)
                AddPosition(pos);
        }
    }

    public enum MatchType
    {
        Normal,     // 3+ in a row
        LShape,     // L or T shape
        FiveMatch,  // 5 in a row
        CrossMatch  // + shape
    }

    private readonly Match3Grid _grid;
    private readonly int _minMatchSize;

    public Match3MatchDetector(Match3Grid grid, int minMatchSize = 3)
    {
        _grid = grid;
        _minMatchSize = Mathf.Max(3, minMatchSize);
    }

    public List<Match> FindAllMatches()
    {
        var allMatches = new List<Match>();
        var processedPositions = new HashSet<Vector2Int>();

        // Check all positions for matches
        for (var y = 0; y < _grid.Height; y++)
        {
            for (var x = 0; x < _grid.Width; x++)
            {
                var pos = new Vector2Int(x, y);
                if (processedPositions.Contains(pos)) continue;

                Match3Tile tile = _grid.GetTile(pos);
                if (tile == null || !tile.IsMatchable) continue;

                // Check for special pattern matches first
                Match specialMatch = CheckForSpecialPatterns(pos, tile.Color, processedPositions);
                if (specialMatch != null)
                {
                    allMatches.Add(specialMatch);
                    foreach (Vector2Int matchPos in specialMatch.Positions)
                        processedPositions.Add(matchPos);
                    continue;
                }

                // Check horizontal match
                Match horizontalMatch = CheckDirection(pos, Vector2Int.right, tile.Color);
                if (horizontalMatch != null && horizontalMatch.Positions.Count >= _minMatchSize)
                {
                    allMatches.Add(horizontalMatch);
                    foreach (Vector2Int matchPos in horizontalMatch.Positions)
                        processedPositions.Add(matchPos);
                }

                // Check vertical match
                Match verticalMatch = CheckDirection(pos, Vector2Int.down, tile.Color);
                if (verticalMatch != null && verticalMatch.Positions.Count >= _minMatchSize)
                {
                    allMatches.Add(verticalMatch);
                    foreach (Vector2Int matchPos in verticalMatch.Positions)
                        processedPositions.Add(matchPos);
                }
            }
        }

        return allMatches;
    }

    public Match FindMatchAtPosition(Vector2Int startPos)
    {
        Match3Tile tile = _grid.GetTile(startPos);
        if (tile == null || !tile.IsMatchable) return null;

        var processedPositions = new HashSet<Vector2Int>();

        // Check for special patterns
        Match specialMatch = CheckForSpecialPatterns(startPos, tile.Color, processedPositions);
        if (specialMatch != null) return specialMatch;

        // Check horizontal
        Match horizontalMatch = CheckDirection(startPos, Vector2Int.right, tile.Color);
        if (horizontalMatch != null && horizontalMatch.Positions.Count >= _minMatchSize)
            return horizontalMatch;

        // Check vertical
        Match verticalMatch = CheckDirection(startPos, Vector2Int.down, tile.Color);
        if (verticalMatch != null && verticalMatch.Positions.Count >= _minMatchSize)
            return verticalMatch;

        return null;
    }

    private Match CheckDirection(Vector2Int startPos, Vector2Int direction, Match3Tile.ColorType color)
    {
        var match = new Match(color);
        match.AddPosition(startPos);

        // Check forward
        Vector2Int currentPos = startPos + direction;
        while (_grid.IsValidPosition(currentPos))
        {
            Match3Tile tile = _grid.GetTile(currentPos);
            if (tile == null || !tile.IsMatchable || tile.Color != color)
                break;

            match.AddPosition(currentPos);
            currentPos += direction;
        }

        // Check backward
        currentPos = startPos - direction;
        while (_grid.IsValidPosition(currentPos))
        {
            Match3Tile tile = _grid.GetTile(currentPos);
            if (tile == null || !tile.IsMatchable || tile.Color != color)
                break;

            match.AddPosition(currentPos);
            currentPos -= direction;
        }

        return match.Positions.Count >= _minMatchSize ? match : null;
    }

    private Match CheckForSpecialPatterns(Vector2Int pos, Match3Tile.ColorType color, HashSet<Vector2Int> processed)
    {
        // Check for 5-in-a-row (horizontal or vertical)
        Match fiveMatch = CheckForFiveMatch(pos, color);
        if (fiveMatch != null) return fiveMatch;

        // Check for L or T shapes
        Match lOrTMatch = CheckForLOrTShape(pos, color);
        if (lOrTMatch != null) return lOrTMatch;

        // Check for cross/+ shape
        Match crossMatch = CheckForCrossShape(pos, color);
        if (crossMatch != null) return crossMatch;

        return null;
    }

    private Match CheckForFiveMatch(Vector2Int pos, Match3Tile.ColorType color)
    {
        // Horizontal
        Match horizontal = CheckDirection(pos, Vector2Int.right, color);
        if (horizontal != null && horizontal.Positions.Count >= 5)
        {
            horizontal.Type = MatchType.FiveMatch;
            return horizontal;
        }

        // Vertical
        Match vertical = CheckDirection(pos, Vector2Int.down, color);
        if (vertical != null && vertical.Positions.Count >= 5)
        {
            vertical.Type = MatchType.FiveMatch;
            return vertical;
        }

        return null;
    }

    private Match CheckForLOrTShape(Vector2Int pos, Match3Tile.ColorType color)
    {
        Match horizontal = CheckDirection(pos, Vector2Int.right, color);
        Match vertical = CheckDirection(pos, Vector2Int.down, color);

        if (horizontal != null && vertical != null &&
            horizontal.Positions.Count >= 3 && vertical.Positions.Count >= 3)
        {
            var combined = new Match(color, MatchType.LShape);
            combined.AddPositions(horizontal.Positions);
            combined.AddPositions(vertical.Positions);
            return combined;
        }

        return null;
    }

    private Match CheckForCrossShape(Vector2Int pos, Match3Tile.ColorType color)
    {
        int horizontalCount = CountInDirection(pos, Vector2Int.right, color) +
                              CountInDirection(pos, Vector2Int.left, color) + 1;

        int verticalCount = CountInDirection(pos, Vector2Int.down, color) +
                            CountInDirection(pos, Vector2Int.up, color) + 1;

        if (horizontalCount >= 3 && verticalCount >= 3)
        {
            var crossMatch = new Match(color, MatchType.CrossMatch);

            // Add horizontal positions
            AddPositionsInDirection(crossMatch, pos, Vector2Int.right, color);
            AddPositionsInDirection(crossMatch, pos, Vector2Int.left, color);

            // Add vertical positions
            AddPositionsInDirection(crossMatch, pos, Vector2Int.down, color);
            AddPositionsInDirection(crossMatch, pos, Vector2Int.up, color);

            crossMatch.AddPosition(pos);

            return crossMatch;
        }

        return null;
    }

    private int CountInDirection(Vector2Int startPos, Vector2Int direction, Match3Tile.ColorType color)
    {
        var count = 0;
        Vector2Int currentPos = startPos + direction;

        while (_grid.IsValidPosition(currentPos))
        {
            Match3Tile tile = _grid.GetTile(currentPos);
            if (tile == null || !tile.IsMatchable || tile.Color != color)
                break;

            count++;
            currentPos += direction;
        }

        return count;
    }

    private void AddPositionsInDirection(Match match, Vector2Int startPos, Vector2Int direction, Match3Tile.ColorType color)
    {
        Vector2Int currentPos = startPos + direction;

        while (_grid.IsValidPosition(currentPos))
        {
            Match3Tile tile = _grid.GetTile(currentPos);
            if (tile == null || !tile.IsMatchable || tile.Color != color)
                break;

            match.AddPosition(currentPos);
            currentPos += direction;
        }
    }

    public bool HasPossibleMatches()
    {
        // Check if any adjacent swap would create a match
        for (var y = 0; y < _grid.Height; y++)
        {
            for (var x = 0; x < _grid.Width; x++)
            {
                var pos = new Vector2Int(x, y);
                Match3Tile tile = _grid.GetTile(pos);

                if (tile == null || !tile.IsMatchable) continue;

                // Check swap with neighbors
                foreach (Vector2Int neighbor in _grid.GetNeighbors(pos))
                {
                    Match3Tile neighborTile = _grid.GetTile(neighbor);
                    if (neighborTile == null || !neighborTile.IsMatchable) continue;

                    // Simulate swap and check for match
                    if (WouldCreateMatch(pos, neighbor))
                        return true;
                }
            }
        }

        return false;
    }

    private bool WouldCreateMatch(Vector2Int pos1, Vector2Int pos2)
    {
        Match3Tile tile1 = _grid.GetTile(pos1);
        Match3Tile tile2 = _grid.GetTile(pos2);

        // Temporarily swap
        _grid.SetTile(pos1, tile2);
        _grid.SetTile(pos2, tile1);

        // Check if either position now has a match
        bool hasMatch = FindMatchAtPosition(pos1) != null || FindMatchAtPosition(pos2) != null;

        // Swap back
        _grid.SetTile(pos1, tile1);
        _grid.SetTile(pos2, tile2);

        return hasMatch;
    }
}
