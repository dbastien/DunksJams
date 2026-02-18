using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Debug wrapper around AStarPathfinder that records every expanded node and the final path.
/// Use for editor visualization and tuning -- not for production.
/// </summary>
public class PathfindingDebug
{
    private readonly AStarPathfinder _pathfinder = new();
    private readonly List<Vector2Int> _expandedNodes = new();
    private readonly Dictionary<Vector2Int, float> _costMap = new();

    private List<Vector2Int> _lastPath;
    private Vector2Int _lastStart, _lastGoal;

    public IReadOnlyList<Vector2Int> ExpandedNodes => _expandedNodes;
    public IReadOnlyDictionary<Vector2Int, float> CostMap => _costMap;
    public IReadOnlyList<Vector2Int> LastPath => _lastPath;
    public Vector2Int LastStart => _lastStart;
    public Vector2Int LastGoal => _lastGoal;
    public int NodesExpanded => _expandedNodes.Count;

    public List<Vector2Int> FindPath
    (
        Vector2Int start, Vector2Int goal, IGraph<Vector2Int> graph,
        IHeuristic<Vector2Int> heuristic, IEdgeMod<Vector2Int> edgeMod = null
    )
    {
        _expandedNodes.Clear();
        _costMap.Clear();
        _lastStart = start;
        _lastGoal = goal;

        var tracker = new DebugEdgeMod(edgeMod, _expandedNodes, _costMap);
        _lastPath = _pathfinder.FindPath(start, goal, graph, heuristic, tracker);
        return _lastPath;
    }

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal, int[,] grid, bool allowDiag = false)
    {
        var graph = new GridGraph2D(grid, allowDiag);
        IHeuristic<Vector2Int> heuristic = allowDiag
            ? new OctileHeuristic2D()
            : new ManhattanHeuristic2D();
        return FindPath(start, goal, graph, heuristic);
    }

    /// <summary>Draws debug gizmos showing the search expansion and final path.</summary>
    public void DrawGizmos(Vector3 offset = default, float cellSize = 1f)
    {
        // Expanded nodes - color by cost (blue=cheap, red=expensive)
        if (_expandedNodes.Count > 0)
        {
            var maxCost = 0f;
            foreach (KeyValuePair<Vector2Int, float> kvp in _costMap)
                if (kvp.Value > maxCost)
                    maxCost = kvp.Value;

            foreach (Vector2Int node in _expandedNodes)
            {
                float t = maxCost > 0 && _costMap.TryGetValue(node, out float c) ? c / maxCost : 0f;
                Gizmos.color = new Color(t, 0.2f, 1f - t, 0.3f);
                Vector3 worldPos = offset +
                                   new Vector3(node.x * cellSize + cellSize * 0.5f, 0.01f,
                                       node.y * cellSize + cellSize * 0.5f);
                Gizmos.DrawCube(worldPos, new Vector3(cellSize * 0.9f, 0.02f, cellSize * 0.9f));
            }
        }

        // Final path
        if (_lastPath is { Count: > 1 })
        {
            Gizmos.color = Color.yellow;
            for (var i = 0; i < _lastPath.Count - 1; i++)
            {
                Vector3 a = offset +
                            new Vector3(_lastPath[i].x * cellSize + cellSize * 0.5f, 0.05f,
                                _lastPath[i].y * cellSize + cellSize * 0.5f);
                Vector3 b = offset +
                            new Vector3(_lastPath[i + 1].x * cellSize + cellSize * 0.5f, 0.05f,
                                _lastPath[i + 1].y * cellSize + cellSize * 0.5f);
                Gizmos.DrawLine(a, b);
            }
        }

        // Start and goal
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(
            offset +
            new Vector3(_lastStart.x * cellSize + cellSize * 0.5f, 0.1f,
                _lastStart.y * cellSize + cellSize * 0.5f), cellSize * 0.3f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(
            offset +
            new Vector3(_lastGoal.x * cellSize + cellSize * 0.5f, 0.1f,
                _lastGoal.y * cellSize + cellSize * 0.5f), cellSize * 0.3f);
    }

    /// <summary>Internal edge mod that wraps an optional user mod and tracks all expanded nodes.</summary>
    private class DebugEdgeMod : IEdgeMod<Vector2Int>
    {
        private readonly IEdgeMod<Vector2Int> _inner;
        private readonly List<Vector2Int> _expanded;
        private readonly Dictionary<Vector2Int, float> _costs;
        private float _runningCost;

        public DebugEdgeMod
        (
            IEdgeMod<Vector2Int> inner, List<Vector2Int> expanded,
            Dictionary<Vector2Int, float> costs
        )
        {
            _inner = inner;
            _expanded = expanded;
            _costs = costs;
        }

        public bool ModifyCost(Vector2Int from, Vector2Int to, ref float cost)
        {
            if (_inner != null && !_inner.ModifyCost(from, to, ref cost))
                return false;

            if (!_costs.ContainsKey(to))
            {
                _expanded.Add(to);
                float fromCost = _costs.TryGetValue(from, out float fc) ? fc : 0f;
                _costs[to] = fromCost + cost;
            }

            return true;
        }
    }
}