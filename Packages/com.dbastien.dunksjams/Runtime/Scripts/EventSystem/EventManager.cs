using System;
using System.Collections.Generic;
using UnityEngine.Pool;

public static class EventManager
{
    private readonly struct PrioritizedListener : IComparable<PrioritizedListener>
    {
        public readonly int Priority;
        public readonly Action<GameEvent> Callback;
        public readonly Delegate OriginalDelegate;

        public PrioritizedListener(int priority, Action<GameEvent> callback, Delegate original)
        {
            Priority = priority;
            Callback = callback;
            OriginalDelegate = original;
        }

        // Higher priority = earlier execution
        public int CompareTo(PrioritizedListener other) => other.Priority.CompareTo(Priority);
    }

    private static readonly Dictionary<Type, List<PrioritizedListener>> _listeners = new();
    private static readonly Dictionary<Type, List<Delegate>> _oneShotListeners = new();
    private static readonly Queue<GameEvent> _eventQueue = new();
    private static readonly Queue<GameEvent> _immediateEventQueue = new();
    private static readonly RingBuffer<GameEvent> _eventHistory = new(100);
    private static readonly Dictionary<Type, ObjectPool<GameEvent>> eventPools = new();

    /// <summary>Add a listener with optional priority (higher = called first, default 0).</summary>
    public static void AddListener<T>(Action<T> listener, int priority = 0) where T : GameEvent
    {
        Type type = typeof(T);
        if (!_listeners.TryGetValue(type, out List<PrioritizedListener> list))
        {
            list = new List<PrioritizedListener>();
            _listeners[type] = list;
        }

        // Check if already registered
        foreach (PrioritizedListener pl in list)
            if (pl.OriginalDelegate.Equals(listener))
                return;

        list.Add(new PrioritizedListener(priority, e => listener((T)e), listener));
        list.Sort();
    }

    public static void AddGenericListener(Action<GameEvent> listener, int priority = 0)
    {
        Type type = typeof(GameEvent);
        if (!_listeners.TryGetValue(type, out List<PrioritizedListener> list))
        {
            list = new List<PrioritizedListener>();
            _listeners[type] = list;
        }

        foreach (PrioritizedListener pl in list)
            if (pl.OriginalDelegate.Equals(listener))
                return;

        list.Add(new PrioritizedListener(priority, listener, listener));
        list.Sort();
    }

    public static void AddListenerOneShot<T>(Action<T> listener, int priority = 0) where T : GameEvent
    {
        AddListener(listener, priority);
        Type type = typeof(T);
        if (!_oneShotListeners.TryGetValue(type, out List<Delegate> oneShotList))
        {
            oneShotList = new List<Delegate>();
            _oneShotListeners[type] = oneShotList;
        }

        oneShotList.Add(listener);
    }

    public static void RemoveListener<T>(Action<T> listener) where T : GameEvent
    {
        RemoveListener(typeof(T), listener);
    }

    public static void QueueEvent<T>(Action<T> initialize = null, bool isImmediate = false) where T : GameEvent, new()
    {
        var gameEvent = GetPooledEvent<T>();
        gameEvent.IsCancelled = false; // Reset cancellation state
        initialize?.Invoke(gameEvent);
        (isImmediate ? _immediateEventQueue : _eventQueue).Enqueue(gameEvent);
    }

    private static void ReleasePooledEvent(GameEvent gameEvent)
    {
        if (eventPools.TryGetValue(gameEvent.GetType(), out ObjectPool<GameEvent> pool)) pool.Release(gameEvent);
    }

    private static T GetPooledEvent<T>() where T : GameEvent, new()
    {
        Type type = typeof(T);
        if (!eventPools.TryGetValue(type, out ObjectPool<GameEvent> pool))
        {
            pool = new ObjectPool<GameEvent>(
                () => new T(),
                _ => { },
                _ => { },
                collectionCheck: true,
                maxSize: 100
            );
            eventPools[type] = pool;
        }

        return (T)pool.Get();
    }

    public static void TriggerEvent(GameEvent gameEvent)
    {
        if (_listeners.TryGetValue(gameEvent.GetType(), out List<PrioritizedListener> list))
            foreach (PrioritizedListener pl in list)
            {
                if (gameEvent.IsCancelled) break; // Support event cancellation

                try { pl.Callback(gameEvent); }
                catch (Exception ex) { DLog.LogE($"Error in event listener: {ex}"); }
            }

        if (_oneShotListeners.TryGetValue(gameEvent.GetType(), out List<Delegate> oneShotList))
        {
            foreach (Delegate listener in oneShotList) RemoveListener(gameEvent.GetType(), listener);
            oneShotList.Clear();
        }

        _eventHistory.Add(gameEvent);
        ReleasePooledEvent(gameEvent);
    }

    public static void Update()
    {
        while (_immediateEventQueue.TryDequeue(out GameEvent gameEvent)) TriggerEvent(gameEvent);
        while (_eventQueue.TryDequeue(out GameEvent gameEvent)) TriggerEvent(gameEvent);
    }

    private static void RemoveListener(Type eventType, Delegate listener)
    {
        if (!_listeners.TryGetValue(eventType, out List<PrioritizedListener> list)) return;

        for (int i = list.Count - 1; i >= 0; i--)
            if (list[i].OriginalDelegate.Equals(listener))
            {
                list.RemoveAt(i);
                break;
            }

        if (list.Count == 0) _listeners.Remove(eventType);
    }
}