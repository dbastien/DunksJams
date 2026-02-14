using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// IGraph implementation over a 2D grid with variable float costs.
/// Cost of 0 = free, positive = extra traversal cost, PositiveInfinity = unwalkable.
/// Supports 4 (cardinal) or 8 (cardinal + diagonal) neighbor modes.
/// </summary>
public class GridGraph2D : IGraph<Vector2Int>
{
    const float Sqrt2 = 1.41421356237f;

    readonly float[,] _costs;
    readonly bool _allowDiag;

    static readonly Vector2Int[] CardinalDirs = { new(0, 1), new(0, -1), new(1, 0), new(-1, 0) };
    static readonly Vector2Int[] DiagonalDirs = { new(1, 1), new(-1, -1), new(1, -1), new(-1, 1) };

    public int Width => _costs.GetLength(0);
    public int Height => _costs.GetLength(1);
    public bool AllowDiag => _allowDiag;

    public GridGraph2D(float[,] costs, bool allowDiag = false)
    {
        _costs = costs ?? throw new ArgumentNullException(nameof(costs));
        _allowDiag = allowDiag;
    }

    /// <summary>Creates a GridGraph2D from a legacy int[,] grid where 1=obstacle, 0=free.</summary>
    public GridGraph2D(int[,] grid, bool allowDiag = false)
    {
        if (grid == null) throw new ArgumentNullException(nameof(grid));
        int w = grid.GetLength(0), h = grid.GetLength(1);
        _costs = new float[w, h];
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                _costs[x, y] = grid[x, y] == 1 ? float.PositiveInfinity : 0f;
        _allowDiag = allowDiag;
    }

    public void GetEdges(Vector2Int node, List<Edge<Vector2Int>> edgeBuffer)
    {
        edgeBuffer.Clear();

        for (int i = 0; i < CardinalDirs.Length; i++)
        {
            var next = node + CardinalDirs[i];
            if (!InBounds(next)) continue;
            float enterCost = _costs[next.x, next.y];
            if (float.IsPositiveInfinity(enterCost)) continue;
            edgeBuffer.Add(new Edge<Vector2Int>(next, 1f + enterCost));
        }

        if (!_allowDiag) return;

        for (int i = 0; i < DiagonalDirs.Length; i++)
        {
            var next = node + DiagonalDirs[i];
            if (!InBounds(next)) continue;
            float enterCost = _costs[next.x, next.y];
            if (float.IsPositiveInfinity(enterCost)) continue;
            edgeBuffer.Add(new Edge<Vector2Int>(next, Sqrt2 + enterCost));
        }
    }

    public bool InBounds(Vector2Int pos) =>
        pos.x >= 0 && pos.x < _costs.GetLength(0) && pos.y >= 0 && pos.y < _costs.GetLength(1);

    public bool IsWalkable(Vector2Int pos) =>
        InBounds(pos) && !float.IsPositiveInfinity(_costs[pos.x, pos.y]);

    public float GetCost(Vector2Int pos) =>
        InBounds(pos) ? _costs[pos.x, pos.y] : float.PositiveInfinity;

    public void SetCost(Vector2Int pos, float cost)
    {
        if (InBounds(pos)) _costs[pos.x, pos.y] = cost;
    }

    public void SetUnwalkable(Vector2Int pos) => SetCost(pos, float.PositiveInfinity);
    public void SetWalkable(Vector2Int pos, float cost = 0f) => SetCost(pos, cost);
}
