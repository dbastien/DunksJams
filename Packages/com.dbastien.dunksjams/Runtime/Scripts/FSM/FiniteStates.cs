using System;
using UnityEngine;

public abstract class FiniteState<T>
{
    public string StateName => _stateName;

    protected T Owner;
    private readonly string _stateName;

    protected FiniteState(T owner)
    {
        Owner = owner;
        _stateName = GetType().Name;
    }

    public virtual void Enter(params object[] parameters)
    {
        DLog.Log($"Entered {_stateName}");
        OnEnter(parameters);
    }

    public virtual void Update() => OnUpdate();

    public virtual void Exit(bool isAborting = false)
    {
        DLog.Log($"Exiting {_stateName}");
        OnExit(isAborting);
    }

    protected virtual void OnEnter(params object[] parameters) { }

    protected virtual void OnUpdate() { }

    protected virtual void OnExit(bool isAborting) { }

    public virtual void OnStateChange(StateChangeEventArgs<T> e) =>
        DLog.Log($"State Change: {e.Action} -> {e.State.StateName}");
}

public class IdleState<T> : FiniteState<T>
{
    public IdleState(T owner) : base(owner) { }
}

public class MoveState<T> : FiniteState<T>
{
    public MoveState(T owner) : base(owner) { }
}

public class AttackState<T> : FiniteState<T>
{
    private readonly float _attackDuration = 1.0f;
    private float _startTime;

    public AttackState(T owner) : base(owner) { }

    protected override void OnEnter(params object[] parameters)
    {
        _startTime = Time.time;
        DLog.Log("Started Attack");
    }

    protected override void OnUpdate()
    {
        if (Time.time - _startTime >= _attackDuration)
            if (Owner is IHasFSM<T> fsmOwner)
                fsmOwner.FSM.ChangeState(fsmOwner.FSM.GetState<IdleState<T>>());
    }

    protected override void OnExit(bool isAborting) => DLog.Log("Ended Attack");
}

public interface IHasFSM<T>
{
    public FiniteStateMachine<T> FSM { get; }
}

public class StateTransition<T>
{
    public string From { get; }
    public string To { get; }
    public Func<bool> Condition { get; }
    public float? Duration { get; }
    public Action Transition { get; }
    public int Priority { get; }
    private readonly float _startTime;

    public StateTransition
    (
        string from, string to, Func<bool> condition, float? duration = null,
        Action onTransition = null, int priority = 0
    )
    {
        From = from;
        To = to;
        Condition = condition;
        Duration = duration;
        Transition = onTransition;
        Priority = priority;
        if (duration.HasValue) _startTime = Time.time;
    }

    public bool CheckElapsedTime() => Duration.HasValue && Time.time - _startTime >= Duration.Value;
    public void ExecuteTransition() => Transition?.Invoke();
}

public enum StateChangeAction
{
    Begun,
    Ending,
    Ended
}

public class StateChangeEventArgs<T> : EventArgs
{
    public StateChangeAction Action { get; }
    public FiniteState<T> State { get; }

    public StateChangeEventArgs(StateChangeAction action, FiniteState<T> state)
    {
        Action = action;
        State = state;
    }
}