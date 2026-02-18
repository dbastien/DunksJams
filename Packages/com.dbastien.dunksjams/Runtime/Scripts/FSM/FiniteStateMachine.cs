using System;
using System.Collections.Generic;
using System.Linq;

public class FiniteStateMachine<T>
{
    private readonly Dictionary<string, FiniteState<T>> _states = new();
    private readonly Dictionary<string, List<StateTransition<T>>> _transitions = new();
    private FiniteState<T> _pendingState;
    private object[] _pendingStateParameters = Array.Empty<object>();
    private bool _isRunning;
    private bool _isPaused;

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

    public void AddTransition
    (
        string from, string to, Func<bool> condition, float? duration = null,
        Action onTransition = null, int priority = 0
    )
    {
        if (!_transitions.TryGetValue(from, out List<StateTransition<T>> transitions))
            _transitions[from] = transitions = new List<StateTransition<T>>();

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
        if (_states.TryGetValue(typeof(TK).Name, out FiniteState<T> state))
            return state as TK;
        DLog.LogW($"State '{typeof(TK).Name}' not found.");
        return null;
    }

    public void Update()
    {
        if (_isPaused || !_isRunning) return;

        RefreshStateIfPending();
        CurrentState?.Update();

        string currentStateName = CurrentState?.StateName ?? "";
        if (!_transitions.TryGetValue(currentStateName, out List<StateTransition<T>> transitions)) return;
        foreach (StateTransition<T> transition in transitions.OrderByDescending(t => t.Priority))
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

    private void RefreshStateIfPending()
    {
        if (_pendingState == null) return;
        NotifyStateChange(StateChangeAction.Ending, CurrentState);
        CurrentState?.Exit();
        CurrentState = _pendingState;
        _pendingState = null;
        CurrentState?.Enter(_pendingStateParameters);
        NotifyStateChange(StateChangeAction.Begun, CurrentState);
    }

    private void NotifyStateChange(StateChangeAction action, FiniteState<T> state)
    {
        var eventArgs = new StateChangeEventArgs<T>(action, state);
        state.OnStateChange(eventArgs);
        OnStateChange?.Invoke(this, eventArgs);
    }
}