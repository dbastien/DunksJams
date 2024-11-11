using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public static class EventManager
{
    delegate void EventListener(GameEvent e);

    static readonly Dictionary<Type, EventListener> _listeners = new();
    static readonly Dictionary<Delegate, EventListener> _listenerLookup = new();
    static readonly Dictionary<Type, List<Delegate>> _oneShotListeners = new();
    static readonly Queue<GameEvent> _eventQueue = new();
    static readonly Queue<GameEvent> _immediateEventQueue = new();
    static readonly RingBuffer<GameEvent> _eventHistory = new(100);
    
    static readonly Dictionary<Type, ObjectPool<GameEvent>> eventPools = new();
    
    public static void AddListener<T>(Action<T> listener) where T : GameEvent
    {
        if (_listenerLookup.ContainsKey(listener)) return;

        EventListener internalListener = e => listener((T)e);
        _listenerLookup[listener] = internalListener;

        var type = typeof(T);
        if (_listeners.TryGetValue(type, out var existingListeners))
            _listeners[type] = existingListeners + internalListener;
        else
            _listeners[type] = internalListener;
    }
    
    public static void AddGenericListener(Action<GameEvent> listener)
    {
        if (_listenerLookup.ContainsKey(listener)) return;
        _listeners[typeof(GameEvent)] += e => listener(e);
    }
    
    public static void AddListenerOneShot<T>(Action<T> listener) where T : GameEvent
    {
        AddListener(listener);
        var type = typeof(T);
        _oneShotListeners.TryGetValue(type, out var oneShotList);
        _oneShotListeners[type] = oneShotList ??= new();
        oneShotList.Add(listener);
    }

    public static void RemoveListener<T>(Action<T> listener) where T : GameEvent
    {
        if (!_listenerLookup.TryGetValue(listener, out var internalListener)) return;

        var eventType = typeof(T);
        if (_listeners.TryGetValue(eventType, out var eventListeners))
        {
            eventListeners -= internalListener;
            if (eventListeners == null) _listeners.Remove(eventType);
            else _listeners[eventType] = eventListeners;
        }

        _listenerLookup.Remove(listener);
    }

    public static void QueueEvent<T>(Action<T> initialize = null, bool isImmediate = false) where T : GameEvent, new()
    {
        var gameEvent = GetPooledEvent<T>();
        initialize?.Invoke(gameEvent);
        (isImmediate ? _immediateEventQueue : _eventQueue).Enqueue(gameEvent);
    }
    
    private static void ReleasePooledEvent(GameEvent gameEvent)
    {
        if (eventPools.TryGetValue(gameEvent.GetType(), out var pool)) pool.Release(gameEvent);
    }
    
    static T GetPooledEvent<T>() where T : GameEvent, new()
    {
        var type = typeof(T);
        if (!eventPools.TryGetValue(type, out var pool))
        {
            pool = new ObjectPool<GameEvent>(
                createFunc: () => new T(),
                actionOnGet: _ => { },
                actionOnRelease: _ => {  },
                collectionCheck: true,
                maxSize: 100
            );
            eventPools[type] = pool;
        }

        return (T)pool.Get();
    }

    public static void TriggerEvent(GameEvent gameEvent)
    {
        if (_listeners.TryGetValue(gameEvent.GetType(), out var eventListener))
        {
            try
            {
                eventListener.Invoke(gameEvent);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in event listener: {ex}");
            }
        }

        if (_oneShotListeners.TryGetValue(gameEvent.GetType(), out var oneShotList))
        {
            foreach (var listener in oneShotList) RemoveListener(gameEvent.GetType(), listener);
            oneShotList.Clear();
        }

        _eventHistory.Add(gameEvent);
        ReleasePooledEvent(gameEvent);
    }

    public static void Update()
    {
        while (_immediateEventQueue.TryDequeue(out var gameEvent)) TriggerEvent(gameEvent);
        while (_eventQueue.TryDequeue(out var gameEvent)) TriggerEvent(gameEvent);
    }

    private static void RemoveListener(Type eventType, Delegate listener)
    {
        if (!_listenerLookup.TryGetValue(listener, out var internalListener)) return;

        if (_listeners.TryGetValue(eventType, out var eventListeners))
        {
            eventListeners -= internalListener;
            if (eventListeners == null) _listeners.Remove(eventType);
            else _listeners[eventType] = eventListeners;
        }

        _listenerLookup.Remove(listener);
    }
}