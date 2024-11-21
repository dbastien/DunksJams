using System;
using UnityEngine;

public static class GridHelper2D
{
    static readonly Vector2Int[] CardinalDirections = { new(0, 1), new(0, -1), new(1, 0), new(-1, 0) };
    static readonly Vector2Int[] DiagonalDirections = { new(1, 1), new(-1, -1), new(1, -1), new(-1, 1) };
    public static readonly Vector2Int[] CombinedDirections = Combine(CardinalDirections, DiagonalDirections);

    public static int GetValidNeighbors(Vector2Int pos, int[,] grid, Vector2Int[] result, bool allowDiag = false)
    {
        int count = 0;
        var directions = allowDiag ? CombinedDirections : CardinalDirections;

        foreach (var dir in directions)
        {
            var neighbor = pos + dir;
            if (!IsWalkable(neighbor, grid)) continue;
            if (count >= result.Length) throw new IndexOutOfRangeException("Result array is too small to hold all neighbors.");
            result[count++] = neighbor;
        }
        return count;
    }

    public static int GetValidNeighborsWithPool(Vector2Int pos, int[,] grid, out Vector2Int[] result, bool allowDiag = false)
    {
        result = ConcurrentArrayPool<Vector2Int>.Shared.RentCleared(8); // Rent an array for up to 8 neighbors
        return GetValidNeighbors(pos, grid, result, allowDiag);
    }

    public static void ReturnNeighbors(Vector2Int[] neighbors) =>
        ConcurrentArrayPool<Vector2Int>.Shared.Return(neighbors);

    public static bool IsWalkable(Vector2Int pos, int[,] grid) =>
        IsInBounds(pos, grid) && grid[pos.x, pos.y] != 1;

    public static bool IsInBounds(Vector2Int pos, int[,] grid) =>
        pos.x >= 0 && pos.x < grid.GetLength(0) && pos.y >= 0 && pos.y < grid.GetLength(1);

    public static int GetCost(Vector2Int pos, int[,] grid, int defaultCost = 1) =>
        IsInBounds(pos, grid) ? grid[pos.x, pos.y] : defaultCost;

    public static Vector2Int[] Combine(params Vector2Int[][] arrays)
    {
        var totalLen = 0;
        foreach (var array in arrays) totalLen += array.Length;

        var result = new Vector2Int[totalLen];
        var offset = 0;
        foreach (var array in arrays)
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
}