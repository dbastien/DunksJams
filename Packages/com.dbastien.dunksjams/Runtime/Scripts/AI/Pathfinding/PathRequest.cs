using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Lightweight path request for async/batched pathfinding.</summary>
public struct PathRequest
{
    public Vector2Int Start;
    public Vector2Int Goal;
    public bool AllowDiag;
    public Action<List<Vector2Int>> Callback;

    public PathRequest(Vector2Int start, Vector2Int goal, bool allowDiag, Action<List<Vector2Int>> callback)
    {
        Start = start;
        Goal = goal;
        AllowDiag = allowDiag;
        Callback = callback;
    }
}

public class PathRequestCompleteEvent : GameEvent
{
    public PathRequest Request { get; set; }
    public List<Vector2Int> Path { get; set; }
}

/// <summary>
/// Processes path requests spread across frames to avoid spikes.
/// Uses a single AStarPathfinder instance for all requests.
/// </summary>
[DisallowMultipleComponent]
[SingletonAutoCreate]
public class PathRequestManager : SingletonEagerBehaviour<PathRequestManager>
{
    [SerializeField] private int maxRequestsPerFrame = 3;
    [SerializeField] private int maxNodesPerFrame = 500;

    private readonly Queue<PendingRequest> _pending = new();
    private readonly AStarPathfinder _pathfinder = new();

    private struct PendingRequest
    {
        public PathRequest Request;
        public IGraph<Vector2Int> Graph;
        public IHeuristic<Vector2Int> Heuristic;
        public IEdgeMod<Vector2Int> EdgeMod;
    }

    protected override void InitInternal() { }

    /// <summary>Queues a path request against a legacy int grid.</summary>
    public void RequestPath(PathRequest request, int[,] grid)
    {
        var graph = new GridGraph2D(grid, request.AllowDiag);
        IHeuristic<Vector2Int> heuristic = request.AllowDiag
            ? new OctileHeuristic2D()
            : new ManhattanHeuristic2D();

        _pending.Enqueue(new PendingRequest
        {
            Request = request,
            Graph = graph,
            Heuristic = heuristic
        });
    }

    /// <summary>Queues a path request against any IGraph.</summary>
    public void RequestPath
    (
        PathRequest request, IGraph<Vector2Int> graph,
        IHeuristic<Vector2Int> heuristic = null, IEdgeMod<Vector2Int> edgeMod = null
    )
    {
        heuristic ??= new ManhattanHeuristic2D();

        _pending.Enqueue(new PendingRequest
        {
            Request = request,
            Graph = graph,
            Heuristic = heuristic,
            EdgeMod = edgeMod
        });
    }

    public int PendingCount => _pending.Count;

    private void Update()
    {
        var processed = 0;
        while (_pending.Count > 0 && processed < maxRequestsPerFrame)
        {
            PendingRequest pending = _pending.Dequeue();
            List<Vector2Int> path = _pathfinder.FindPath(
                pending.Request.Start,
                pending.Request.Goal,
                pending.Graph,
                pending.Heuristic,
                pending.EdgeMod,
                maxNodesPerFrame);

            pending.Request.Callback?.Invoke(path);

            EventManager.QueueEvent<PathRequestCompleteEvent>(e =>
            {
                e.Request = pending.Request;
                e.Path = path;
            });

            processed++;
        }
    }

    /// <summary>Coroutine that waits until a specific path request is complete.</summary>
    public static IEnumerator WaitForPath
    (
        Vector2Int start, Vector2Int goal, int[,] grid,
        bool allowDiag, Action<List<Vector2Int>> onComplete
    )
    {
        var done = false;
        List<Vector2Int> result = null;

        Instance?.RequestPath(
            new PathRequest(start, goal, allowDiag, path =>
            {
                result = path;
                done = true;
            }),
            grid);

        while (!done)
            yield return null;

        onComplete?.Invoke(result);
    }

    protected override void OnDestroy()
    {
        _pending.Clear();
        base.OnDestroy();
    }
}