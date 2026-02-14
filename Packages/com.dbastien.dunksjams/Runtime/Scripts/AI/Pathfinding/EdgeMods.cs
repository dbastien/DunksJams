using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Blocks pathfinding through specific nodes (locked doors, destroyed bridges, etc.).</summary>
public class ExcludeNodes<TNode> : IEdgeMod<TNode> where TNode : IEquatable<TNode>
{
    readonly HashSet<TNode> _excluded = new();

    public void Exclude(TNode node) => _excluded.Add(node);
    public void Include(TNode node) => _excluded.Remove(node);
    public void Clear() => _excluded.Clear();
    public bool IsExcluded(TNode node) => _excluded.Contains(node);

    public bool ModifyCost(TNode from, TNode to, ref float cost) => !_excluded.Contains(to);
}

/// <summary>Blocks specific directed edges (one-way barriers, closed doors between specific rooms).</summary>
public class ExcludeEdges<TNode> : IEdgeMod<TNode> where TNode : IEquatable<TNode>
{
    readonly HashSet<(TNode from, TNode to)> _excluded = new();

    public void Exclude(TNode from, TNode to) => _excluded.Add((from, to));
    public void ExcludeBidirectional(TNode a, TNode b) { Exclude(a, b); Exclude(b, a); }
    public void Include(TNode from, TNode to) => _excluded.Remove((from, to));
    public void IncludeBidirectional(TNode a, TNode b) { Include(a, b); Include(b, a); }
    public void Clear() => _excluded.Clear();

    public bool ModifyCost(TNode from, TNode to, ref float cost) => !_excluded.Contains((from, to));
}

/// <summary>
/// Adds extra cost to specific nodes (danger zones, enemy territory, mud).
/// Stacks on top of the graph's base cost.
/// </summary>
public class CostOverlay<TNode> : IEdgeMod<TNode> where TNode : IEquatable<TNode>
{
    readonly Dictionary<TNode, float> _extraCosts = new();

    public void SetCost(TNode node, float extraCost) => _extraCosts[node] = extraCost;
    public void RemoveCost(TNode node) => _extraCosts.Remove(node);
    public void Clear() => _extraCosts.Clear();

    public bool ModifyCost(TNode from, TNode to, ref float cost)
    {
        if (_extraCosts.TryGetValue(to, out float extra))
            cost += extra;
        return true;
    }
}

/// <summary>
/// Adds virtual portal edges that don't exist in the base graph.
/// Useful for teleporters, warp points, shortcuts.
/// </summary>
public class PortalEdges : IEdgeMod<Vector2Int>
{
    readonly Dictionary<Vector2Int, List<(Vector2Int target, float cost)>> _portals = new();

    public void AddPortal(Vector2Int from, Vector2Int to, float cost = 0f)
    {
        if (!_portals.TryGetValue(from, out var list))
        {
            list = new List<(Vector2Int, float)>();
            _portals[from] = list;
        }
        list.Add((to, cost));
    }

    public void AddPortalBidirectional(Vector2Int a, Vector2Int b, float cost = 0f)
    {
        AddPortal(a, b, cost);
        AddPortal(b, a, cost);
    }

    public void RemovePortal(Vector2Int from, Vector2Int to)
    {
        if (_portals.TryGetValue(from, out var list))
            list.RemoveAll(p => p.target == to);
    }

    public void Clear() => _portals.Clear();

    // Portal edges are injected by the graph wrapper, not by ModifyCost.
    // This modifier passes through normal edges unchanged.
    public bool ModifyCost(Vector2Int from, Vector2Int to, ref float cost) => true;

    public bool HasPortals(Vector2Int from) => _portals.ContainsKey(from);

    public IReadOnlyList<(Vector2Int target, float cost)> GetPortals(Vector2Int from) =>
        _portals.TryGetValue(from, out var list) ? list : null;
}

/// <summary>
/// Combines multiple edge modifiers. All must allow the edge; costs stack additively.
/// </summary>
public class CompositeEdgeMod<TNode> : IEdgeMod<TNode> where TNode : IEquatable<TNode>
{
    readonly List<IEdgeMod<TNode>> _mods = new();

    public void Add(IEdgeMod<TNode> mod) => _mods.Add(mod);
    public void Remove(IEdgeMod<TNode> mod) => _mods.Remove(mod);
    public void Clear() => _mods.Clear();

    public bool ModifyCost(TNode from, TNode to, ref float cost)
    {
        foreach (var mod in _mods)
            if (!mod.ModifyCost(from, to, ref cost)) return false;
        return true;
    }
}
