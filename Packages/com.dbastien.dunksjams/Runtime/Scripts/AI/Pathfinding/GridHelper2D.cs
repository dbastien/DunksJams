using System;
using UnityEngine;

public static class GridHelper2D
{
    private static readonly Vector2Int[] CardinalDirections = { new(0, 1), new(0, -1), new(1, 0), new(-1, 0) };
    private static readonly Vector2Int[] DiagonalDirections = { new(1, 1), new(-1, -1), new(1, -1), new(-1, 1) };
    public static readonly Vector2Int[] CombinedDirections = Combine(CardinalDirections, DiagonalDirections);

    public static int GetValidNeighbors(Vector2Int pos, int[,] grid, Vector2Int[] result, bool allowDiag = false)
    {
        var count = 0;
        Vector2Int[] directions = allowDiag ? CombinedDirections : CardinalDirections;

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighbor = pos + dir;
            if (!IsWalkable(neighbor, grid)) continue;
            if (count >= result.Length)
                throw new IndexOutOfRangeException("Result array is too small to hold all neighbors.");
            result[count++] = neighbor;
        }

        return count;
    }

    public static int GetValidNeighborsWithPool
    (
        Vector2Int pos, int[,] grid, out Vector2Int[] result,
        bool allowDiag = false
    )
    {
        result = ConcurrentArrayPool<Vector2Int>.Shared.RentCleared(8);
        return GetValidNeighbors(pos, grid, result, allowDiag);
    }

    public static void ReturnNeighbors(Vector2Int[] neighbors) =>
        ConcurrentArrayPool<Vector2Int>.Shared.Return(neighbors);

    public static bool IsWalkable(Vector2Int pos, int[,] grid) =>
        IsInBounds(pos, grid) && grid[pos.x, pos.y] != 1;

    public static bool IsInBounds(Vector2Int pos, int[,] grid) =>
        pos.x >= 0 && pos.x < grid.GetLength(0) && pos.y >= 0 && pos.y < grid.GetLength(1);

    /// <summary>Returns the cell value as a float cost. Legacy grids use 0=free, 1=obstacle.</summary>
    public static float GetCost(Vector2Int pos, int[,] grid, float defaultCost = 1f) =>
        IsInBounds(pos, grid) ? grid[pos.x, pos.y] : defaultCost;

    // --- float[,] overloads for weighted grids ---

    public static bool IsWalkable(Vector2Int pos, float[,] grid) =>
        IsInBoundsF(pos, grid) && !float.IsPositiveInfinity(grid[pos.x, pos.y]);

    public static bool IsInBoundsF(Vector2Int pos, float[,] grid) =>
        pos.x >= 0 && pos.x < grid.GetLength(0) && pos.y >= 0 && pos.y < grid.GetLength(1);

    public static float GetCostF(Vector2Int pos, float[,] grid, float defaultCost = float.PositiveInfinity) =>
        IsInBoundsF(pos, grid) ? grid[pos.x, pos.y] : defaultCost;

    public static int GetValidNeighbors(Vector2Int pos, float[,] grid, Vector2Int[] result, bool allowDiag = false)
    {
        var count = 0;
        Vector2Int[] directions = allowDiag ? CombinedDirections : CardinalDirections;

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighbor = pos + dir;
            if (!IsWalkable(neighbor, grid)) continue;
            if (count >= result.Length)
                throw new IndexOutOfRangeException("Result array is too small to hold all neighbors.");
            result[count++] = neighbor;
        }

        return count;
    }

    public static int GetValidNeighborsWithPool
    (
        Vector2Int pos, float[,] grid, out Vector2Int[] result,
        bool allowDiag = false
    )
    {
        result = ConcurrentArrayPool<Vector2Int>.Shared.RentCleared(8);
        return GetValidNeighbors(pos, grid, result, allowDiag);
    }

    // --- Utility ---

    public static Vector2Int[] Combine(params Vector2Int[][] arrays)
    {
        var totalLen = 0;
        foreach (Vector2Int[] array in arrays) totalLen += array.Length;

        var result = new Vector2Int[totalLen];
        var offset = 0;
        foreach (Vector2Int[] array in arrays)
        {
            array.CopyTo(result, offset);
            offset += array.Length;
        }

        return result;
    }

    public static void UpdateObstacle(Vector2Int pos, int[,] grid, bool isObstacle)
    {
        if (IsInBounds(pos, grid))
            grid[pos.x, pos.y] = isObstacle ? 1 : 0;
    }

    public static void UpdateObstacle(Vector2Int pos, float[,] grid, bool isObstacle)
    {
        if (IsInBoundsF(pos, grid))
            grid[pos.x, pos.y] = isObstacle ? float.PositiveInfinity : 0f;
    }

    /// <summary>Bresenham line-of-sight check through a float grid. Returns true if no unwalkable cell is hit.</summary>
    public static bool HasLineOfSight(Vector2Int from, Vector2Int to, float[,] grid)
    {
        int dx = Mathf.Abs(to.x - from.x);
        int dy = Mathf.Abs(to.y - from.y);
        int sx = from.x < to.x ? 1 : -1;
        int sy = from.y < to.y ? 1 : -1;
        int err = dx - dy;

        int x = from.x, y = from.y;
        while (true)
        {
            if (!IsWalkable(new Vector2Int(x, y), grid)) return false;
            if (x == to.x && y == to.y) return true;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y += sy;
            }
        }
    }

    /// <summary>Bresenham line-of-sight check through an int grid.</summary>
    public static bool HasLineOfSight(Vector2Int from, Vector2Int to, int[,] grid)
    {
        int dx = Mathf.Abs(to.x - from.x);
        int dy = Mathf.Abs(to.y - from.y);
        int sx = from.x < to.x ? 1 : -1;
        int sy = from.y < to.y ? 1 : -1;
        int err = dx - dy;

        int x = from.x, y = from.y;
        while (true)
        {
            if (!IsWalkable(new Vector2Int(x, y), grid)) return false;
            if (x == to.x && y == to.y) return true;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y += sy;
            }
        }
    }
}