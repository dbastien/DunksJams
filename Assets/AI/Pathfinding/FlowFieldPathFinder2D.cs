using System.Collections.Generic;
using UnityEngine;

public class FlowFieldPathfinder2D
{
    private readonly int[,] _grid;
    private readonly Vector2[,] _flowField;

    public FlowFieldPathfinder2D(int[,] grid)
    {
        _grid = grid ?? throw new System.ArgumentNullException(nameof(grid));
        _flowField = new Vector2[grid.GetLength(0), grid.GetLength(1)];
    }

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal, bool allowDiag = false)
    {
        ComputeFlowField(goal, allowDiag);
        return TracePath(start, goal);
    }

    /// <summary>
    /// Find path using a pre-allocated list to avoid allocation. Clears the list before use.
    /// </summary>
    public void FindPath(Vector2Int start, Vector2Int goal, List<Vector2Int> result, bool allowDiag = false)
    {
        ComputeFlowField(goal, allowDiag);
        TracePath(start, goal, result);
    }

    private void ComputeFlowField(Vector2Int goal, bool allowDiag)
    {
        var openSet = new Queue<Vector2Int>();
        var costField = new int[_grid.GetLength(0), _grid.GetLength(1)];

        for (int y = 0; y < _grid.GetLength(1); ++y)
            for (int x = 0; x < _grid.GetLength(0); ++x)
            {
                costField[x, y] = int.MaxValue;
                _flowField[x, y] = Vector2.zero;
            }

        costField[goal.x, goal.y] = 0;
        openSet.Enqueue(goal);

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();
            var currentCost = costField[current.x, current.y];

            var neighborCount = GridHelper2D.GetValidNeighborsWithPool(current, _grid, out var neighbors, allowDiag);
            try
            {
                for (int i = 0; i < neighborCount; ++i)
                {
                    var neighbor = neighbors[i];
                    int newCost = currentCost + 1;
                    if (newCost >= costField[neighbor.x, neighbor.y]) continue;

                    costField[neighbor.x, neighbor.y] = newCost;
                    _flowField[neighbor.x, neighbor.y] = new Vector2Int(
                        Mathf.RoundToInt(current.x - neighbor.x),
                        Mathf.RoundToInt(current.y - neighbor.y)
                    ).Normalized();
                    openSet.Enqueue(neighbor);
                }
            }
            finally
            {
                GridHelper2D.ReturnNeighbors(neighbors);
            }
        }
    }

    private List<Vector2Int> TracePath(Vector2Int start, Vector2Int goal)
    {
        var path = new List<Vector2Int>(32);
        TracePath(start, goal, path);
        return path;
    }

    private void TracePath(Vector2Int start, Vector2Int goal, List<Vector2Int> path)
    {
        path.Clear();
        path.Add(start);
        var current = start;

        while (current != goal)
        {
            if (!GridHelper2D.IsInBounds(current, _grid)) break;

            var flow = _flowField[current.x, current.y];
            if (flow == Vector2.zero) break;

            current += new Vector2Int(Mathf.RoundToInt(flow.x), Mathf.RoundToInt(flow.y));
            path.Add(current);
        }
    }

    public void UpdateObstacle(Vector2Int pos, bool isObstacle) =>
        GridHelper2D.UpdateObstacle(pos, _grid, isObstacle);

    public Vector2 GetFlowDir(Vector2Int pos) =>
        GridHelper2D.IsInBounds(pos, _grid) ? _flowField[pos.x, pos.y] : Vector2.zero;
}