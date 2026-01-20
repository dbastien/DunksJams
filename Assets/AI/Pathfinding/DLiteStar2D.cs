using System;
using System.Collections.Generic;
using UnityEngine;

public class DStarLitePathfinder2D
{
    private readonly PriorityQueue<DStarNode2D> _openSet = new();
    private readonly Dictionary<Vector2Int, DStarNode2D> _nodes = new();
    private readonly int[,] _grid;

    private DStarNode2D _startNode2D;
    private DStarNode2D _goalNode2D;

    public DStarLitePathfinder2D(Vector2Int start, Vector2Int goal, int[,] grid)
    {
        _grid = grid ?? throw new ArgumentNullException(nameof(grid));
        _startNode2D = GetNode(start);
        _goalNode2D = GetNode(goal);
        _goalNode2D.RHS = 0;
        _openSet.Enqueue(_goalNode2D);
        ComputeShortestPath();
    }

    private DStarNode2D GetNode(Vector2Int position)
    {
        if (!_nodes.TryGetValue(position, out var node))
        {
            node = new DStarNode2D(position);
            _nodes[position] = node;
        }
        return node;
    }

    private void ComputeShortestPath()
    {
        while (_openSet.Count > 0 && (_startNode2D.G != _startNode2D.RHS || _openSet.Peek().F < _startNode2D.F))
        {
            var node = _openSet.Dequeue();

            if (node.G > node.RHS)
            {
                node.G = node.RHS;
                UpdateNeighbors(node);
            }
            else
            {
                node.G = float.PositiveInfinity;
                UpdateNode(node);
                UpdateNeighbors(node);
            }
        }
    }

    private void UpdateNode(DStarNode2D node2D)
    {
        if (node2D.Position != _goalNode2D.Position)
        {
            node2D.RHS = float.PositiveInfinity;

            var neighborCount = GridHelper2D.GetValidNeighborsWithPool(node2D.Position, _grid, out var neighbors);
            try
            {
                for (int i = 0; i < neighborCount; ++i)
                {
                    var neighborPos = neighbors[i];
                    var neighbor = GetNode(neighborPos);
                    float tentativeRHS = neighbor.G + 1f;
                    if (tentativeRHS < node2D.RHS)
                        node2D.RHS = tentativeRHS;
                }
            }
            finally
            {
                GridHelper2D.ReturnNeighbors(neighbors);
            }
        }

        if (node2D.G != node2D.RHS) _openSet.Enqueue(node2D);
    }

    private void UpdateNeighbors(DStarNode2D node2D)
    {
        var neighborCount = GridHelper2D.GetValidNeighborsWithPool(node2D.Position, _grid, out var neighbors);
        try
        {
            for (int i = 0; i < neighborCount; ++i)
                UpdateNode(GetNode(neighbors[i]));
        }
        finally
        {
            GridHelper2D.ReturnNeighbors(neighbors);
        }
    }

    public void UpdateObstacle(Vector2Int obstaclePosition, bool isObstacle)
    {
        GridHelper2D.UpdateObstacle(obstaclePosition, _grid, isObstacle);

        var neighborCount = GridHelper2D.GetValidNeighborsWithPool(obstaclePosition, _grid, out var neighbors);
        try
        {
            for (int i = 0; i < neighborCount; ++i)
                UpdateNode(GetNode(neighbors[i]));
        }
        finally
        {
            GridHelper2D.ReturnNeighbors(neighbors);
        }

        ComputeShortestPath();
    }

    public List<Vector2Int> GetPath()
    {
        var path = new List<Vector2Int>(64); // Preallocate space to reduce resizing
        var current = _startNode2D;

        while (current.Position != _goalNode2D.Position)
        {
            path.Add(current.Position);

            var neighborCount = GridHelper2D.GetValidNeighborsWithPool(current.Position, _grid, out var neighbors);
            try
            {
                DStarNode2D nextNode = null;

                for (int i = 0; i < neighborCount; ++i)
                {
                    var neighbor = GetNode(neighbors[i]);
                    if (neighbor.G < float.PositiveInfinity &&
                        (nextNode == null || neighbor.G < nextNode.G))
                        nextNode = neighbor;
                }

                if (nextNode == null) return null;

                current = nextNode;
            }
            finally
            {
                GridHelper2D.ReturnNeighbors(neighbors);
            }
        }

        path.Add(_goalNode2D.Position);
        return path;
    }
}

public class DStarNode2D : IComparable<DStarNode2D>
{
    public Vector2Int Position;
    public float G = float.PositiveInfinity;
    public float RHS = float.PositiveInfinity;
    public float F => Mathf.Min(G, RHS);

    public DStarNode2D(Vector2Int position) => Position = position;

    public int CompareTo(DStarNode2D other) => F.CompareTo(other.F);
}