using System;

/// <summary>
/// Dynamically modifies edge costs during pathfinding without rebuilding the graph.
/// Use for doors, danger zones, portals, area avoidance, etc.
/// </summary>
public interface IEdgeMod<TNode> where TNode : IEquatable<TNode>
{
    /// <summary>
    /// Modifies the cost of traversing from <paramref name="from"/> to <paramref name="to"/>.
    /// Return false to discard the edge entirely (e.g. locked door).
    /// </summary>
    bool ModifyCost(TNode from, TNode to, ref float cost);
}
