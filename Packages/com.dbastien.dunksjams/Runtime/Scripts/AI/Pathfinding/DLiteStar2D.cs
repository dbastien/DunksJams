using System;
using System.Collections.Generic;
using UnityEngine;

public class DStarLitePathfinder2D : IPathFinder2D
{
    private readonly PriorityQueue<DStarNode2D> _openSet = new();
    private readonly Dictionary<Vector2Int, DStarNode2D> _nodes = new();
    private readonly List<Edge<Vector2Int>> _edgeBuffer = new(8);

    private readonly int[,] _grid;
    private readonly IGraph<Vector2Int> _graph;

    private DStarNode2D _startNode2D;
    private DStarNode2D _goalNode2D;

    public DStarLitePathfinder2D(Vector2Int start, Vector2Int goal, int[,] grid, bool allowDiag = false)
    {
        _grid = grid ?? throw new ArgumentNullException(nameof(grid));
        _graph = new GridGraph2D(grid, allowDiag);
        Initialize(start, goal);
    }

    public DStarLitePathfinder2D(Vector2Int start, Vector2Int goal, IGraph<Vector2Int> graph)
    {
        _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        Initialize(start, goal);
    }

    private void Initialize(Vector2Int start, Vector2Int goal)
    {
        _startNode2D = GetNode(start);
        _goalNode2D = GetNode(goal);
        _goalNode2D.RHS = 0;
        _openSet.Enqueue(_goalNode2D);
        ComputeShortestPath();
    }

    private DStarNode2D GetNode(Vector2Int position)
    {
        if (!_nodes.TryGetValue(position, out DStarNode2D node))
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
            DStarNode2D node = _openSet.Dequeue();

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

            _graph.GetEdges(node2D.Position, _edgeBuffer);
            foreach (Edge<Vector2Int> edge in _edgeBuffer)
            {
                DStarNode2D neighbor = GetNode(edge.Next);
                float tentativeRHS = neighbor.G + edge.Cost;
                if (tentativeRHS < node2D.RHS)
                    node2D.RHS = tentativeRHS;
            }
        }

        if (node2D.G != node2D.RHS) _openSet.Enqueue(node2D);
    }

    private void UpdateNeighbors(DStarNode2D node2D)
    {
        _graph.GetEdges(node2D.Position, _edgeBuffer);
        foreach (Edge<Vector2Int> edge in _edgeBuffer)
            UpdateNode(GetNode(edge.Next));
    }

    public void UpdateObstacle(Vector2Int obstaclePosition, bool isObstacle)
    {
        if (_grid != null)
            GridHelper2D.UpdateObstacle(obstaclePosition, _grid, isObstacle);

        // Update affected neighbors
        _graph.GetEdges(obstaclePosition, _edgeBuffer);
        foreach (Edge<Vector2Int> edge in _edgeBuffer)
            UpdateNode(GetNode(edge.Next));

        // Also update the obstacle node itself
        UpdateNode(GetNode(obstaclePosition));
        ComputeShortestPath();
    }

    public List<Vector2Int> GetPath()
    {
        var path = new List<Vector2Int>(64);
        DStarNode2D current = _startNode2D;

        while (current.Position != _goalNode2D.Position)
        {
            path.Add(current.Position);

            _graph.GetEdges(current.Position, _edgeBuffer);
            DStarNode2D nextNode = null;

            foreach (Edge<Vector2Int> edge in _edgeBuffer)
            {
                DStarNode2D neighbor = GetNode(edge.Next);
                if (neighbor.G < float.PositiveInfinity &&
                    (nextNode == null || neighbor.G + edge.Cost < nextNode.G + 1f))
                    nextNode = neighbor;
            }

            if (nextNode == null) return null;
            current = nextNode;
        }

        path.Add(_goalNode2D.Position);
        return path;
    }

    // IPathFinder2D
    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal, int[,] grid, bool allowDiag = false)
    {
        // D* Lite is stateful; for the interface, create a fresh instance internally
        var finder = new DStarLitePathfinder2D(start, goal, grid, allowDiag);
        return finder.GetPath();
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