using System;

public interface IHeuristic<TNode> where TNode : IEquatable<TNode>
{
    float Estimate(TNode from, TNode to);
}
