using System;
using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinder
{
    readonly PriorityQueue<AStarNode> _openSet = new();
    readonly HashSet<Vector2Int> _closedSet = new();
    readonly Dictionary<Vector2Int, AStarNode> _openSetLookup = new();

    public delegate float HeuristicFunc(Vector2Int a, Vector2Int b);

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal, int[,] grid, bool allowDiag = false,
        HeuristicFunc heuristic = null)
    {
        if (grid == null) throw new ArgumentNullException(nameof(grid));
        if (start == goal) return new List<Vector2Int> { start };
        if (grid.GetLength(0) == 0 || grid.GetLength(1) == 0)
            throw new ArgumentException("Grid must not be empty.");

        heuristic ??= Vector2IntExtensions.ManhattanDistance;

        _openSet.Clear();
        _closedSet.Clear();
        _openSetLookup.Clear();

        var startNode = new AStarNode(start, 0, heuristic(start, goal), null);
        _openSet.Enqueue(startNode);
        _openSetLookup[start] = startNode;

        var neighbors = ConcurrentArrayPool<Vector2Int>.Shared.RentCleared(8);
        try
        {
            while (_openSet.Count > 0)
            {
                var current = _openSet.Dequeue();
                _openSetLookup.Remove(current.Position);

                if (current.Position == goal)
                    return ReconstructPath(current);

                _closedSet.Add(current.Position);

                var neighborCount = GridHelper2D.GetValidNeighbors(current.Position, grid, neighbors, allowDiag);
                for (var i = 0; i < neighborCount; ++i)
                {
                    var neighbor = neighbors[i];
                    if (_closedSet.Contains(neighbor)) continue;

                    var g = current.G + GridHelper2D.GetCost(neighbor, grid);
                    var h = heuristic(neighbor, goal);

                    if (!_openSetLookup.TryGetValue(neighbor, out var existingNode) || g < existingNode.G)
                    {
                        var neighborNode = new AStarNode(neighbor, g, h, current);
                        _openSet.Enqueue(neighborNode);
                        _openSetLookup[neighbor] = neighborNode;
                    }
                }
            }
        }
        finally
        {
            ConcurrentArrayPool<Vector2Int>.Shared.Return(neighbors);
        }

        return null; // No path found
    }

    static List<Vector2Int> ReconstructPath(AStarNode node)
    {
        var pathBuffer = ConcurrentArrayPool<Vector2Int>.Shared.RentCleared(64); // Preallocate larger buffer
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
            return compare == 0 ? H.CompareTo(other.H) : compare; // Tie-break on H
        }
    }
}