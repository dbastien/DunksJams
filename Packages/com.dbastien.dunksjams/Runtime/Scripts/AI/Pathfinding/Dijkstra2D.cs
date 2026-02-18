using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dijkstra expansion from a start node. Supports:
/// - Finding nearest reachable target from a set of candidates
/// - Expanding all nodes within a cost budget
/// - Getting the cost to any expanded node
/// </summary>
public class Dijkstra2D
{
    private readonly PriorityQueue<DijkstraNode> _openSet = new();
    private readonly Dictionary<Vector2Int, float> _costSoFar = new();
    private readonly Dictionary<Vector2Int, Vector2Int> _cameFrom = new();
    private readonly List<Edge<Vector2Int>> _edgeBuffer = new(8);

    /// <summary>
    /// Expands from start up to maxCost. After calling, use GetCost/GetPath to query results.
    /// </summary>
    public void Expand
    (
        Vector2Int start, IGraph<Vector2Int> graph, float maxCost = float.PositiveInfinity,
        IEdgeMod<Vector2Int> edgeMod = null
    )
    {
        if (graph == null) throw new ArgumentNullException(nameof(graph));

        _openSet.Clear();
        _costSoFar.Clear();
        _cameFrom.Clear();

        _openSet.Enqueue(new DijkstraNode(start, 0f));
        _costSoFar[start] = 0f;

        while (_openSet.Count > 0)
        {
            DijkstraNode current = _openSet.Dequeue();

            if (current.Cost >
                (_costSoFar.TryGetValue(current.Position, out float recorded) ? recorded : float.PositiveInfinity))
                continue;

            graph.GetEdges(current.Position, _edgeBuffer);
            foreach (Edge<Vector2Int> edge in _edgeBuffer)
            {
                float cost = edge.Cost;
                if (edgeMod != null && !edgeMod.ModifyCost(current.Position, edge.Next, ref cost))
                    continue;

                float newCost = current.Cost + cost;
                if (newCost > maxCost) continue;

                if (!_costSoFar.TryGetValue(edge.Next, out float existing) || newCost < existing)
                {
                    _costSoFar[edge.Next] = newCost;
                    _cameFrom[edge.Next] = current.Position;
                    _openSet.Enqueue(new DijkstraNode(edge.Next, newCost));
                }
            }
        }
    }

    /// <summary>
    /// Finds the cheapest reachable target from a set of candidates.
    /// Stops early once the first target is dequeued (guaranteed cheapest by Dijkstra ordering).
    /// </summary>
    public Vector2Int? FindNearest
    (
        Vector2Int start, IGraph<Vector2Int> graph, HashSet<Vector2Int> targets,
        float maxCost = float.PositiveInfinity, IEdgeMod<Vector2Int> edgeMod = null
    )
    {
        if (graph == null) throw new ArgumentNullException(nameof(graph));
        if (targets == null || targets.Count == 0) return null;
        if (targets.Contains(start)) return start;

        _openSet.Clear();
        _costSoFar.Clear();
        _cameFrom.Clear();

        _openSet.Enqueue(new DijkstraNode(start, 0f));
        _costSoFar[start] = 0f;

        while (_openSet.Count > 0)
        {
            DijkstraNode current = _openSet.Dequeue();

            if (current.Cost > (_costSoFar.TryGetValue(current.Position, out float rec) ? rec : float.PositiveInfinity))
                continue;

            if (targets.Contains(current.Position))
                return current.Position;

            graph.GetEdges(current.Position, _edgeBuffer);
            foreach (Edge<Vector2Int> edge in _edgeBuffer)
            {
                float cost = edge.Cost;
                if (edgeMod != null && !edgeMod.ModifyCost(current.Position, edge.Next, ref cost))
                    continue;

                float newCost = current.Cost + cost;
                if (newCost > maxCost) continue;

                if (!_costSoFar.TryGetValue(edge.Next, out float existing) || newCost < existing)
                {
                    _costSoFar[edge.Next] = newCost;
                    _cameFrom[edge.Next] = current.Position;
                    _openSet.Enqueue(new DijkstraNode(edge.Next, newCost));
                }
            }
        }

        return null;
    }

    /// <summary>Returns the cost to reach a previously expanded node, or null if unreachable.</summary>
    public float? GetCost(Vector2Int target) =>
        _costSoFar.TryGetValue(target, out float cost) ? cost : null;

    /// <summary>Reconstructs the path to a previously expanded node.</summary>
    public List<Vector2Int> GetPath(Vector2Int target)
    {
        if (!_cameFrom.ContainsKey(target) && !_costSoFar.ContainsKey(target))
            return null;

        var path = new List<Vector2Int>();
        Vector2Int current = target;

        while (_cameFrom.ContainsKey(current))
        {
            path.Add(current);
            current = _cameFrom[current];
        }

        path.Add(current); // start node
        path.Reverse();
        return path;
    }

    /// <summary>Returns all nodes that were expanded (reachable within budget).</summary>
    public IEnumerable<Vector2Int> ExpandedNodes => _costSoFar.Keys;

    private struct DijkstraNode : IComparable<DijkstraNode>
    {
        public Vector2Int Position;
        public float Cost;

        public DijkstraNode(Vector2Int pos, float cost)
        {
            Position = pos;
            Cost = cost;
        }

        public int CompareTo(DijkstraNode other) => Cost.CompareTo(other.Cost);
    }
}