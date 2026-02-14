using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic graph for arbitrary waypoint networks, state machines, etc.
/// Nodes and directed edges can be added/removed at runtime.
/// </summary>
public class NodeGraph<TNode> : IGraph<TNode> where TNode : IEquatable<TNode>
{
    readonly Dictionary<TNode, List<Edge<TNode>>> _adjacency = new();

    public int NodeCount => _adjacency.Count;

    public IEnumerable<TNode> Nodes => _adjacency.Keys;

    public void GetEdges(TNode node, List<Edge<TNode>> edgeBuffer)
    {
        edgeBuffer.Clear();
        if (_adjacency.TryGetValue(node, out var edges))
            edgeBuffer.AddRange(edges);
    }

    public void AddNode(TNode node)
    {
        if (!_adjacency.ContainsKey(node))
            _adjacency[node] = new List<Edge<TNode>>();
    }

    public bool RemoveNode(TNode node)
    {
        if (!_adjacency.Remove(node)) return false;

        foreach (var edges in _adjacency.Values)
            edges.RemoveAll(e => e.Next.Equals(node));

        return true;
    }

    public void AddEdge(TNode from, TNode to, float cost)
    {
        AddNode(from);
        AddNode(to);
        _adjacency[from].Add(new Edge<TNode>(to, cost));
    }

    /// <summary>Adds a bidirectional edge (two directed edges).</summary>
    public void AddEdgeBidirectional(TNode a, TNode b, float cost)
    {
        AddEdge(a, b, cost);
        AddEdge(b, a, cost);
    }

    public bool RemoveEdge(TNode from, TNode to)
    {
        if (!_adjacency.TryGetValue(from, out var edges)) return false;
        return edges.RemoveAll(e => e.Next.Equals(to)) > 0;
    }

    public void RemoveEdgeBidirectional(TNode a, TNode b)
    {
        RemoveEdge(a, b);
        RemoveEdge(b, a);
    }

    public bool HasNode(TNode node) => _adjacency.ContainsKey(node);

    public bool HasEdge(TNode from, TNode to)
    {
        if (!_adjacency.TryGetValue(from, out var edges)) return false;
        foreach (var e in edges)
            if (e.Next.Equals(to)) return true;
        return false;
    }

    public void Clear() => _adjacency.Clear();
}

/// <summary>
/// NodeGraph specialized for Vector2Int nodes with spatial hashing for nearest-node queries.
/// </summary>
public class SpatialNodeGraph2D : NodeGraph<Vector2Int>
{
    readonly SpatialHash2D<Vector2Int> _spatial;

    public SpatialNodeGraph2D(float cellSize = 1f) =>
        _spatial = new SpatialHash2D<Vector2Int>(cellSize);

    public new void AddNode(Vector2Int node)
    {
        base.AddNode(node);
        _spatial.Insert(node, node);
    }

    public new bool RemoveNode(Vector2Int node)
    {
        _spatial.Remove(node, node);
        return base.RemoveNode(node);
    }

    public new void AddEdge(Vector2Int from, Vector2Int to, float cost)
    {
        if (!HasNode(from)) { base.AddNode(from); _spatial.Insert(from, from); }
        if (!HasNode(to)) { base.AddNode(to); _spatial.Insert(to, to); }
        base.AddEdge(from, to, cost);
    }

    public new void AddEdgeBidirectional(Vector2Int a, Vector2Int b, float cost)
    {
        AddEdge(a, b, cost);
        AddEdge(b, a, cost);
    }

    public new void Clear()
    {
        base.Clear();
        _spatial.Clear();
    }

    /// <summary>Finds the nearest node within the given radius of a world position.</summary>
    public Vector2Int? FindNearestNode(Vector2 worldPos, float searchRadius)
    {
        var candidates = _spatial.QueryInRadius(worldPos, searchRadius);
        if (candidates.Count == 0) return null;

        Vector2Int best = candidates[0];
        float bestDist = (worldPos - (Vector2)best).sqrMagnitude;

        for (int i = 1; i < candidates.Count; i++)
        {
            float dist = (worldPos - (Vector2)candidates[i]).sqrMagnitude;
            if (dist < bestDist) { best = candidates[i]; bestDist = dist; }
        }

        return best;
    }

    /// <summary>Finds all nodes within the given radius.</summary>
    public List<Vector2Int> FindNodesInRadius(Vector2 worldPos, float radius) =>
        _spatial.QueryInRadius(worldPos, radius);
}
