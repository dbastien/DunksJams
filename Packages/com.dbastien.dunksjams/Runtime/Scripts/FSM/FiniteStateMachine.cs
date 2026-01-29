using System;
using System.Collections.Generic;
using System.Linq;

public class FiniteStateMachine<T>
{
    readonly Dictionary<string, FiniteState<T>> _states = new();
    readonly Dictionary<string, List<StateTransition<T>>> _transitions = new();
    FiniteState<T> _pendingState;
    object[] _pendingStateParameters = Array.Empty<object>();
    bool _isRunning;
    bool _isPaused;

    public FiniteState<T> CurrentState { get; private set; }
    public event EventHandler<StateChangeEventArgs<T>> OnStateChange;

    public FiniteStateMachine(FiniteState<T> initialState, T owner)
    {
        RegisterState(initialState);
        CurrentState = initialState;
        CurrentState?.Enter();
    }

    public void RegisterState(FiniteState<T> state)
    {
        if (!_states.TryAdd(state.StateName, state))
            DLog.LogW($"State '{state.StateName}' is already registered.");
    }

    public void AddTransition(string from, string to, Func<bool> condition, float? duration = null, Action onTransition = null, int priority = 0)
    {
        if (!_transitions.TryGetValue(from, out var transitions))
            _transitions[from] = transitions = new();
        
        transitions.Add(new StateTransition<T>(from, to, condition, duration, onTransition, priority));
    }

    public void Start() => _isRunning = true;
    public void Pause() => _isPaused = true;
    public void Resume() => _isPaused = false;

    public void ChangeState(FiniteState<T> newState, params object[] parameters)
    {
        if (newState == CurrentState || !_states.ContainsValue(newState)) return;
        _pendingState = newState;
        _pendingStateParameters = parameters.Length > 0 ? parameters : Array.Empty<object>();
    }

    public TK GetState<TK>() where TK : FiniteState<T>
    {
        if (_states.TryGetValue(typeof(TK).Name, out var state))
            return state as TK;
        DLog.LogW($"State '{typeof(TK).Name}' not found.");
        return null;
    }

    public void Update()
    {
        if (_isPaused || !_isRunning) return;
        
        RefreshStateIfPending();
        CurrentState?.Update();

        var currentStateName = CurrentState?.StateName ?? "";
        if (!_transitions.TryGetValue(currentStateName, out var transitions)) return;
        foreach (var transition in transitions.OrderByDescending(t => t.Priority))
        {
            if (!transition.Condition() && !transition.CheckElapsedTime()) continue;
            transition.ExecuteTransition();
            ChangeState(_states[transition.To]);
            RefreshStateIfPending();
            break;
        }
    }
    
    public void Reset()
    {
        _isRunning = false;
        _isPaused = false;
        _pendingState = null;
        _pendingStateParameters = Array.Empty<object>();
        CurrentState?.Exit();
        CurrentState = null;
    }

    void RefreshStateIfPending()
    {
        if (_pendingState == null) return;
        NotifyStateChange(StateChangeAction.Ending, CurrentState);
        CurrentState?.Exit();
        CurrentState = _pendingState;
        _pendingState = null;
        CurrentState?.Enter(_pendingStateParameters);
        NotifyStateChange(StateChangeAction.Begun, CurrentState);
    }
    
    void NotifyStateChange(StateChangeAction action, FiniteState<T> state)
    {
        var eventArgs = new StateChangeEventArgs<T>(action, state);
        state.OnStateChange(eventArgs);
        OnStateChange?.Invoke(this, eventArgs);
    }
}