using System;
using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinder : IPathFinder2D
{
    readonly PriorityQueue<AStarNode> _openSet = new();
    readonly HashSet<Vector2Int> _closedSet = new();
    readonly Dictionary<Vector2Int, AStarNode> _openSetLookup = new();
    readonly List<Edge<Vector2Int>> _edgeBuffer = new(8);

    int[,] _lastGrid;

    // --- IGraph-based API (primary) ---

    public List<Vector2Int> FindPath<THeuristic>(
        Vector2Int start, Vector2Int goal,
        IGraph<Vector2Int> graph,
        THeuristic heuristic,
        IEdgeMod<Vector2Int> edgeMod = null,
        int maxExpand = int.MaxValue) where THeuristic : IHeuristic<Vector2Int>
    {
        if (graph == null) throw new ArgumentNullException(nameof(graph));
        if (start == goal) return new List<Vector2Int> { start };

        _openSet.Clear();
        _closedSet.Clear();
        _openSetLookup.Clear();

        var startNode = new AStarNode(start, 0, heuristic.Estimate(start, goal), null);
        _openSet.Enqueue(startNode);
        _openSetLookup[start] = startNode;

        int expanded = 0;
        while (_openSet.Count > 0 && expanded < maxExpand)
        {
            var current = _openSet.Dequeue();
            _openSetLookup.Remove(current.Position);

            if (current.Position == goal)
                return ReconstructPath(current);

            _closedSet.Add(current.Position);
            expanded++;

            graph.GetEdges(current.Position, _edgeBuffer);
            foreach (var edge in _edgeBuffer)
            {
                if (_closedSet.Contains(edge.Next)) continue;

                float cost = edge.Cost;
                if (edgeMod != null && !edgeMod.ModifyCost(current.Position, edge.Next, ref cost))
                    continue;

                float g = current.G + cost;
                float h = heuristic.Estimate(edge.Next, goal);

                if (!_openSetLookup.TryGetValue(edge.Next, out var existing) || g < existing.G)
                {
                    var node = new AStarNode(edge.Next, g, h, current);
                    _openSet.Enqueue(node);
                    _openSetLookup[edge.Next] = node;
                }
            }
        }

        return null;
    }

    /// <summary>Non-generic overload accepting interface references directly.</summary>
    public List<Vector2Int> FindPath(
        Vector2Int start, Vector2Int goal,
        IGraph<Vector2Int> graph,
        IHeuristic<Vector2Int> heuristic,
        IEdgeMod<Vector2Int> edgeMod = null,
        int maxExpand = int.MaxValue) =>
        FindPathWithInterface(start, goal, graph, heuristic, edgeMod, maxExpand);

    // --- Legacy grid-based API (backward compatible, implements IPathFinder2D) ---

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal, int[,] grid, bool allowDiag = false)
    {
        _lastGrid = grid;
        var graph = new GridGraph2D(grid, allowDiag);
        var heuristic = allowDiag ? (IHeuristic<Vector2Int>)new OctileHeuristic2D() : new ManhattanHeuristic2D();
        return FindPathWithInterface(start, goal, graph, heuristic);
    }

    public void UpdateObstacle(Vector2Int pos, bool isObstacle)
    {
        if (_lastGrid != null)
            GridHelper2D.UpdateObstacle(pos, _lastGrid, isObstacle);
    }

    /// <summary>Legacy overload with delegate heuristic for backward compatibility.</summary>
    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal, int[,] grid, bool allowDiag,
        Func<Vector2Int, Vector2Int, float> heuristic)
    {
        _lastGrid = grid;
        var graph = new GridGraph2D(grid, allowDiag);
        if (heuristic != null)
            return FindPathWithInterface(start, goal, graph, new DelegateHeuristic2D(heuristic));
        var defaultH = allowDiag ? (IHeuristic<Vector2Int>)new OctileHeuristic2D() : new ManhattanHeuristic2D();
        return FindPathWithInterface(start, goal, graph, defaultH);
    }

    List<Vector2Int> FindPathWithInterface(Vector2Int start, Vector2Int goal, IGraph<Vector2Int> graph,
        IHeuristic<Vector2Int> heuristic, IEdgeMod<Vector2Int> edgeMod = null, int maxExpand = int.MaxValue)
    {
        if (graph == null) throw new ArgumentNullException(nameof(graph));
        if (start == goal) return new List<Vector2Int> { start };

        _openSet.Clear();
        _closedSet.Clear();
        _openSetLookup.Clear();

        var startNode = new AStarNode(start, 0, heuristic.Estimate(start, goal), null);
        _openSet.Enqueue(startNode);
        _openSetLookup[start] = startNode;

        int expanded = 0;
        while (_openSet.Count > 0 && expanded < maxExpand)
        {
            var current = _openSet.Dequeue();
            _openSetLookup.Remove(current.Position);

            if (current.Position == goal)
                return ReconstructPath(current);

            _closedSet.Add(current.Position);
            expanded++;

            graph.GetEdges(current.Position, _edgeBuffer);
            foreach (var edge in _edgeBuffer)
            {
                if (_closedSet.Contains(edge.Next)) continue;

                float cost = edge.Cost;
                if (edgeMod != null && !edgeMod.ModifyCost(current.Position, edge.Next, ref cost))
                    continue;

                float g = current.G + cost;
                float h = heuristic.Estimate(edge.Next, goal);

                if (!_openSetLookup.TryGetValue(edge.Next, out var existing) || g < existing.G)
                {
                    var node = new AStarNode(edge.Next, g, h, current);
                    _openSet.Enqueue(node);
                    _openSetLookup[edge.Next] = node;
                }
            }
        }

        return null;
    }

    // --- Path reconstruction ---

    static List<Vector2Int> ReconstructPath(AStarNode node)
    {
        var pathBuffer = ConcurrentArrayPool<Vector2Int>.Shared.RentCleared(64);
        var pathIndex = 0;

        try
        {
            while (node != null)
            {
                if (pathIndex >= pathBuffer.Length)
                {
                    var newBuffer = ConcurrentArrayPool<Vector2Int>.Shared.Rent(pathBuffer.Length * 2);
                    Array.Copy(pathBuffer, newBuffer, pathIndex);
                    ConcurrentArrayPool<Vector2Int>.Shared.Return(pathBuffer);
                    pathBuffer = newBuffer;
                }

                pathBuffer[pathIndex++] = node.Position;
                node = node.Parent;
            }

            var path = new List<Vector2Int>(pathIndex);
            for (var i = pathIndex - 1; i >= 0; --i)
                path.Add(pathBuffer[i]);

            return path;
        }
        finally
        {
            ConcurrentArrayPool<Vector2Int>.Shared.Return(pathBuffer);
        }
    }

    // --- Internal types ---

    public class AStarNode : IComparable<AStarNode>
    {
        public Vector2Int Position;
        public float G, H;
        public float F => G + H;
        public AStarNode Parent;

        public AStarNode(Vector2Int pos, float g, float h, AStarNode parent)
        {
            Position = pos;
            G = g;
            H = h;
            Parent = parent;
        }

        public int CompareTo(AStarNode other)
        {
            var compare = F.CompareTo(other.F);
            return compare == 0 ? H.CompareTo(other.H) : compare;
        }
    }

    struct DelegateHeuristic2D : IHeuristic<Vector2Int>
    {
        readonly Func<Vector2Int, Vector2Int, float> _func;
        public DelegateHeuristic2D(Func<Vector2Int, Vector2Int, float> func) => _func = func;
        public float Estimate(Vector2Int from, Vector2Int to) => _func(from, to);
    }
}
