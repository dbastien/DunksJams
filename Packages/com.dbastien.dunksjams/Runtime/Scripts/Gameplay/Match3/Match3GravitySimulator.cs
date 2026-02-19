using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Match3GravitySimulator
{
    private readonly Match3Grid _grid;
    private readonly Match3TilePool _tilePool;
    private readonly Match3LevelData _levelData;

    public Match3GravitySimulator(Match3Grid grid, Match3TilePool tilePool, Match3LevelData levelData)
    {
        _grid = grid ?? throw new ArgumentNullException(nameof(grid));
        _tilePool = tilePool ?? throw new ArgumentNullException(nameof(tilePool));
        _levelData = levelData ?? throw new ArgumentNullException(nameof(levelData));
    }

    public IEnumerator ApplyGravityAndRefill(Action onComplete = null)
    {
        bool hadFalls;

        do
        {
            hadFalls = false;

            // Process each column from bottom to top
            for (var x = 0; x < _grid.Width; x++)
            {
                if (ApplyGravityToColumn(x))
                    hadFalls = true;
            }

            if (hadFalls)
                yield return new WaitForSeconds(0.15f);

        } while (hadFalls);

        // Fill empty slots at the top
        yield return FillEmptySlots();

        onComplete?.Invoke();
    }

    private bool ApplyGravityToColumn(int column)
    {
        var hadMovement = false;

        // Start from bottom and work up
        for (int y = _grid.Height - 1; y >= 0; y--)
        {
            var currentPos = new Vector2Int(column, y);
            Match3Tile tile = _grid.GetTile(currentPos);

            // Skip if there's already a tile here
            if (tile != null) continue;

            // Look for a tile above to fall down
            for (int searchY = y - 1; searchY >= 0; searchY--)
            {
                var searchPos = new Vector2Int(column, searchY);
                Match3Tile fallingTile = _grid.GetTile(searchPos);

                if (fallingTile != null && fallingTile.IsMatchable)
                {
                    // Move tile down
                    _grid.SetTile(searchPos, null);
                    _grid.SetTile(currentPos, fallingTile);

                    // Animate the fall
                    Vector3 targetPos = _grid.GridToWorld(currentPos);
                    fallingTile.AnimateToPosition(targetPos, 0.15f);

                    hadMovement = true;
                    break;
                }
            }
        }

        return hadMovement;
    }

    private IEnumerator FillEmptySlots()
    {
        var newTiles = new List<Match3Tile>();

        for (var x = 0; x < _grid.Width; x++)
        {
            for (var y = 0; y < _grid.Height; y++)
            {
                var pos = new Vector2Int(x, y);
                if (_grid.GetTile(pos) != null) continue;

                // Create new tile
                Match3Tile.ColorType color = _levelData.GetRandomColor();
                Match3Tile newTile = _tilePool.GetTile(Match3Tile.TileType.Normal, color);

                if (newTile == null)
                {
                    DLog.LogE($"Match3GravitySimulator: Failed to get tile from pool");
                    continue;
                }

                // Position above the grid initially
                Vector3 startPos = _grid.GridToWorld(new Vector2Int(x, -1));
                newTile.transform.position = startPos;
                newTile.gameObject.SetActive(true);

                // Initialize and place in grid
                newTile.Initialize(Match3Tile.TileType.Normal, color, pos);
                _grid.SetTile(pos, newTile);

                // Animate falling into place
                Vector3 targetPos = _grid.GridToWorld(pos);
                newTile.AnimateToPosition(targetPos, 0.2f);

                newTiles.Add(newTile);
            }
        }

        if (newTiles.Count > 0)
            yield return new WaitForSeconds(0.25f);
    }

    public IEnumerator RemoveTiles(List<Vector2Int> positions, Action<int> onTileRemoved = null)
    {
        if (positions == null || positions.Count == 0)
        {
            yield break;
        }

        var tilesToRemove = new List<Match3Tile>();

        // Collect tiles and trigger animations
        foreach (Vector2Int pos in positions)
        {
            Match3Tile tile = _grid.GetTile(pos);
            if (tile != null)
            {
                tilesToRemove.Add(tile);
                _grid.RemoveTile(pos);

                // Trigger destroy animation
                tile.PlayDestroyAnimation(() =>
                {
                    _tilePool.ReturnTile(tile);
                });
            }
        }

        // Notify for each tile removed
        onTileRemoved?.Invoke(tilesToRemove.Count);

        // Wait for animations to complete
        yield return new WaitForSeconds(0.25f);
    }

    public bool HasEmptySpaces()
    {
        for (var y = 0; y < _grid.Height; y++)
        {
            for (var x = 0; x < _grid.Width; x++)
            {
                if (_grid.GetTile(x, y) == null)
                    return true;
            }
        }

        return false;
    }

    public int CountEmptySpaces()
    {
        var count = 0;

        for (var y = 0; y < _grid.Height; y++)
        for (var x = 0; x < _grid.Width; x++)
            if (_grid.GetTile(x, y) == null)
                count++;

        return count;
    }

    public List<Vector2Int> GetEmptyPositions()
    {
        var emptyPositions = new List<Vector2Int>();

        for (var y = 0; y < _grid.Height; y++)
        for (var x = 0; x < _grid.Width; x++)
        {
            var pos = new Vector2Int(x, y);
            if (_grid.GetTile(pos) == null)
                emptyPositions.Add(pos);
        }

        return emptyPositions;
    }
}
