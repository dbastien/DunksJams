using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Post-processor that removes unnecessary waypoints from a grid path using line-of-sight checks.
/// Produces cleaner paths suitable for steering agents.
/// </summary>
public class PathSmoother2D : IPathProcessor<Vector2Int>
{
    readonly float[,] _costGrid;
    readonly int[,] _intGrid;

    public PathSmoother2D(float[,] costGrid) => _costGrid = costGrid;
    public PathSmoother2D(int[,] intGrid) => _intGrid = intGrid;

    public void Process(List<Vector2Int> rawPath, List<Vector2Int> result)
    {
        result.Clear();
        if (rawPath == null || rawPath.Count == 0) return;

        if (rawPath.Count <= 2)
        {
            result.AddRange(rawPath);
            return;
        }

        result.Add(rawPath[0]);
        int current = 0;

        while (current < rawPath.Count - 1)
        {
            int farthestVisible = current + 1;

            for (int i = rawPath.Count - 1; i > current + 1; i--)
            {
                if (HasLineOfSight(rawPath[current], rawPath[i]))
                {
                    farthestVisible = i;
                    break;
                }
            }

            result.Add(rawPath[farthestVisible]);
            current = farthestVisible;
        }
    }

    bool HasLineOfSight(Vector2Int from, Vector2Int to)
    {
        if (_costGrid != null) return GridHelper2D.HasLineOfSight(from, to, _costGrid);
        if (_intGrid != null) return GridHelper2D.HasLineOfSight(from, to, _intGrid);
        return true;
    }
}
