using System;
using System.Collections.Generic;
using UnityEngine;

public class Match3Grid
{
    private readonly Match3Tile[,] _tiles;
    private readonly int _width;
    private readonly int _height;
    private readonly float _tileSize;
    private readonly Vector3 _gridOrigin;

    public int Width => _width;
    public int Height => _height;
    public float TileSize => _tileSize;
    public Vector3 GridOrigin => _gridOrigin;

    public Match3Grid(int width, int height, float tileSize = 1f, Vector3? origin = null)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

        _width = width;
        _height = height;
        _tileSize = tileSize;
        _gridOrigin = origin ?? Vector3.zero;
        _tiles = new Match3Tile[width, height];
    }

    public Match3Tile GetTile(int x, int y)
    {
        if (!IsValidPosition(x, y)) return null;
        return _tiles[x, y];
    }

    public Match3Tile GetTile(Vector2Int pos) => GetTile(pos.x, pos.y);

    public void SetTile(int x, int y, Match3Tile tile)
    {
        if (!IsValidPosition(x, y))
        {
            DLog.LogW($"Match3Grid.SetTile: Invalid position ({x}, {y})");
            return;
        }

        _tiles[x, y] = tile;

        if (tile != null)
            tile.GridPosition = new Vector2Int(x, y);
    }

    public void SetTile(Vector2Int pos, Match3Tile tile) => SetTile(pos.x, pos.y, tile);

    public void SwapTiles(Vector2Int pos1, Vector2Int pos2)
    {
        if (!IsValidPosition(pos1) || !IsValidPosition(pos2))
        {
            DLog.LogW($"Match3Grid.SwapTiles: Invalid positions");
            return;
        }

        Match3Tile tile1 = GetTile(pos1);
        Match3Tile tile2 = GetTile(pos2);

        SetTile(pos1, tile2);
        SetTile(pos2, tile1);

        // Update visual positions
        if (tile1 != null)
            tile1.transform.position = GridToWorld(pos2);

        if (tile2 != null)
            tile2.transform.position = GridToWorld(pos1);
    }

    public void RemoveTile(Vector2Int pos)
    {
        SetTile(pos, null);
    }

    public bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < _width && y >= 0 && y < _height;
    }

    public bool IsValidPosition(Vector2Int pos) => IsValidPosition(pos.x, pos.y);

    public Vector3 GridToWorld(int x, int y)
    {
        return _gridOrigin + new Vector3(
            x * _tileSize,
            -y * _tileSize,
            0
        );
    }

    public Vector3 GridToWorld(Vector2Int pos) => GridToWorld(pos.x, pos.y);

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 localPos = worldPos - _gridOrigin;
        int x = Mathf.RoundToInt(localPos.x / _tileSize);
        int y = Mathf.RoundToInt(-localPos.y / _tileSize);
        return new Vector2Int(x, y);
    }

    public List<Vector2Int> GetNeighbors(Vector2Int pos, bool includeDiagonals = false)
    {
        var neighbors = new List<Vector2Int>(8);

        // Cardinal directions
        Vector2Int[] cardinalDirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int dir in cardinalDirs)
        {
            Vector2Int neighbor = pos + dir;
            if (IsValidPosition(neighbor))
                neighbors.Add(neighbor);
        }

        if (includeDiagonals)
        {
            Vector2Int[] diagonalDirs = {
                new Vector2Int(1, 1), new Vector2Int(-1, 1),
                new Vector2Int(1, -1), new Vector2Int(-1, -1)
            };

            foreach (Vector2Int dir in diagonalDirs)
            {
                Vector2Int neighbor = pos + dir;
                if (IsValidPosition(neighbor))
                    neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    public void Clear()
    {
        for (var y = 0; y < _height; y++)
        for (var x = 0; x < _width; x++)
            _tiles[x, y] = null;
    }

    public void ForEachTile(Action<Match3Tile, int, int> action)
    {
        if (action == null) return;

        for (var y = 0; y < _height; y++)
        for (var x = 0; x < _width; x++)
        {
            Match3Tile tile = _tiles[x, y];
            if (tile != null)
                action(tile, x, y);
        }
    }

    public int CountTiles(Func<Match3Tile, bool> predicate)
    {
        var count = 0;

        for (var y = 0; y < _height; y++)
        for (var x = 0; x < _width; x++)
        {
            Match3Tile tile = _tiles[x, y];
            if (tile != null && predicate(tile))
                count++;
        }

        return count;
    }
}
