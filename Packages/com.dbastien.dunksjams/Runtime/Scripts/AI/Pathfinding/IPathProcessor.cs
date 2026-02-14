using System;
using System.Collections.Generic;

/// <summary>
/// Post-processes a raw node path into a refined result (smoothing, waypoint reduction, etc.).
/// </summary>
public interface IPathProcessor<TNode> where TNode : IEquatable<TNode>
{
    /// <summary>Processes rawPath and writes the result into result. Both lists are caller-managed.</summary>
    void Process(List<TNode> rawPath, List<TNode> result);
}
