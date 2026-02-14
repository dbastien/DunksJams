using System;
using System.Collections.Generic;
using UnityEngine;

public class FlowFieldPathfinder2D : IPathFinder2D
{
    readonly int[,] _grid;
    readonly float[,] _costGrid;
    readonly Vector2[,] _flowField;
    readonly float[,] _integrationField;
    readonly bool _useFloatCosts;

    int _width, _height;

    /// <summary>Creates a flow field pathfinder from a legacy int grid (0=free, 1=obstacle).</summary>
    public FlowFieldPathfinder2D(int[,] grid)
    {
        _grid = grid ?? throw new ArgumentNullException(nameof(grid));
        _width = grid.GetLength(0);
        _height = grid.GetLength(1);
        _flowField = new Vector2[_width, _height];
        _integrationField = new float[_width, _height];
    }

    /// <summary>Creates a flow field pathfinder from a float cost grid (0=free, +inf=obstacle).</summary>
    public FlowFieldPathfinder2D(float[,] costGrid)
    {
        _costGrid = costGrid ?? throw new ArgumentNullException(nameof(costGrid));
        _width = costGrid.GetLength(0);
        _height = costGrid.GetLength(1);
        _flowField = new Vector2[_width, _height];
        _integrationField = new float[_width, _height];
        _useFloatCosts = true;
    }

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal, bool allowDiag = false)
    {
        ComputeFlowField(goal, allowDiag);
        return TracePath(start, goal);
    }

    public void FindPath(Vector2Int start, Vector2Int goal, List<Vector2Int> result, bool allowDiag = false)
    {
        ComputeFlowField(goal, allowDiag);
        TracePath(start, goal, result);
    }

    void ComputeFlowField(Vector2Int goal, bool allowDiag)
    {
        var openSet = new Queue<Vector2Int>();

        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
            {
                _integrationField[x, y] = float.MaxValue;
                _flowField[x, y] = Vector2.zero;
            }

        _integrationField[goal.x, goal.y] = 0;
        openSet.Enqueue(goal);

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();
            float currentCost = _integrationField[current.x, current.y];

            int neighborCount;
            Vector2Int[] neighbors;

            if (_useFloatCosts)
                neighborCount = GridHelper2D.GetValidNeighborsWithPool(current, _costGrid, out neighbors, allowDiag);
            else
                neighborCount = GridHelper2D.GetValidNeighborsWithPool(current, _grid, out neighbors, allowDiag);

            try
            {
                for (int i = 0; i < neighborCount; i++)
                {
                    var neighbor = neighbors[i];
                    float moveCost = GetMovementCost(neighbor);
                    float newCost = currentCost + moveCost;

                    if (newCost >= _integrationField[neighbor.x, neighbor.y]) continue;

                    _integrationField[neighbor.x, neighbor.y] = newCost;
                    _flowField[neighbor.x, neighbor.y] = new Vector2Int(
                        current.x - neighbor.x,
                        current.y - neighbor.y
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

    float GetMovementCost(Vector2Int pos)
    {
        if (_useFloatCosts)
            return 1f + (_costGrid != null ? _costGrid[pos.x, pos.y] : 0f);
        return 1f;
    }

    List<Vector2Int> TracePath(Vector2Int start, Vector2Int goal)
    {
        var path = new List<Vector2Int>(32);
        TracePath(start, goal, path);
        return path;
    }

    void TracePath(Vector2Int start, Vector2Int goal, List<Vector2Int> path)
    {
        path.Clear();
        path.Add(start);
        var current = start;
        int safetyLimit = _width * _height;

        while (current != goal && safetyLimit-- > 0)
        {
            if (current.x < 0 || current.x >= _width || current.y < 0 || current.y >= _height) break;

            var flow = _flowField[current.x, current.y];
            if (flow == Vector2.zero) break;

            current += new Vector2Int(Mathf.RoundToInt(flow.x), Mathf.RoundToInt(flow.y));
            path.Add(current);
        }
    }

    public void UpdateObstacle(Vector2Int pos, bool isObstacle)
    {
        if (_useFloatCosts && _costGrid != null)
            GridHelper2D.UpdateObstacle(pos, _costGrid, isObstacle);
        else if (_grid != null)
            GridHelper2D.UpdateObstacle(pos, _grid, isObstacle);
    }

    public Vector2 GetFlowDir(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= _width || pos.y < 0 || pos.y >= _height) return Vector2.zero;
        return _flowField[pos.x, pos.y];
    }

    public float GetIntegrationCost(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= _width || pos.y < 0 || pos.y >= _height) return float.MaxValue;
        return _integrationField[pos.x, pos.y];
    }

    // IPathFinder2D
    List<Vector2Int> IPathFinder2D.FindPath(Vector2Int start, Vector2Int goal, int[,] grid, bool allowDiag) =>
        FindPath(start, goal, allowDiag);
}
