using System;
using System.Collections.Generic;

public readonly struct Edge<TNode> where TNode : IEquatable<TNode>
{
    public readonly TNode Next;
    public readonly float Cost;

    public Edge(TNode next, float cost)
    {
        Next = next;
        Cost = cost;
    }
}

public interface IGraph<TNode> where TNode : IEquatable<TNode>
{
    /// <summary>Fills edgeBuffer with all directed edges from node. Buffer is cleared before call.</summary>
    public void GetEdges(TNode node, List<Edge<TNode>> edgeBuffer);
}