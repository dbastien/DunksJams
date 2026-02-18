using System;

public interface IHeuristic<TNode> where TNode : IEquatable<TNode>
{
    public float Estimate(TNode from, TNode to);
}