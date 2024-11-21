using System;
using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinder
{
    static readonly PriorityQueue<AStarNode> GlobalOpenSet = new();
    static readonly HashSet<Vector2Int> GlobalClosedSet = new();
    static readonly Dictionary<Vector2Int, AStarNode> GlobalOpenSetLookup = new();

    public delegate float HeuristicFunc(Vector2Int a, Vector2Int b);

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal, int[,] grid, bool allowDiag = false, HeuristicFunc heuristic = null)
    {
        if (grid == null) throw new ArgumentNullException(nameof(grid));
        if (start == goal) return new() { start };
        if (grid.GetLength(0) == 0 || grid.GetLength(1) == 0)
            throw new ArgumentException("Grid must not be empty.");

        heuristic ??= Vector2IntExtensions.ManhattanDistance;

        GlobalOpenSet.Clear();
        GlobalClosedSet.Clear();
        GlobalOpenSetLookup.Clear();

        var startNode = new AStarNode(start, 0, heuristic(start, goal), null);
        GlobalOpenSet.Enqueue(startNode);
        GlobalOpenSetLookup[start] = startNode;

        var neighbors = ConcurrentArrayPool<Vector2Int>.Shared.RentCleared(8);
        try
        {
            while (GlobalOpenSet.Count > 0)
            {
                var current = GlobalOpenSet.Dequeue();
                GlobalOpenSetLookup.Remove(current.Position);

                if (current.Position == goal)
                    return ReconstructPath(current);

                GlobalClosedSet.Add(current.Position);

                int neighborCount = GridHelper2D.GetValidNeighbors(current.Position, grid, neighbors, allowDiag);
                for (int i = 0; i < neighborCount; ++i)
                {
                    var neighbor = neighbors[i];
                    if (GlobalClosedSet.Contains(neighbor)) continue;

                    float g = current.G + GridHelper2D.GetCost(neighbor, grid);
                    float h = heuristic(neighbor, goal);

                    if (!GlobalOpenSetLookup.TryGetValue(neighbor, out var existingNode) || g < existingNode.G)
                    {
                        var neighborNode = new AStarNode(neighbor, g, h, current);
                        GlobalOpenSet.Enqueue(neighborNode);
                        GlobalOpenSetLookup[neighbor] = neighborNode;
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

    private static List<Vector2Int> ReconstructPath(AStarNode node)
    {
        var pathBuffer = ConcurrentArrayPool<Vector2Int>.Shared.RentCleared(64); // Preallocate larger buffer
        int pathIndex = 0;

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
            for (int i = pathIndex - 1; i >= 0; --i)
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
            int compare = F.CompareTo(other.F);
            return compare == 0 ? H.CompareTo(other.H) : compare; // Tie-break on H
        }
    }
}