using UnityEngine;

[RequireComponent(typeof(VehicleController))]
public class VehicleFSM : MonoBehaviour, IHasFSM<VehicleController>
{
    VehicleController _vehicle;
    FiniteStateMachine<VehicleController> _fsm;
    Health _health;

    public FiniteStateMachine<VehicleController> FSM => _fsm;

    public string CurrentStateName => _fsm?.CurrentState?.StateName ?? "None";

    void Awake()
    {
        _vehicle = GetComponent<VehicleController>();
        TryGetComponent(out _health);

        var idle = new VehicleIdleState(_vehicle);
        var driving = new VehicleDrivingState(_vehicle);
        var airborne = new VehicleAirborneState(_vehicle);
        var destroyed = new VehicleDestroyedState(_vehicle);

        _fsm = new FiniteStateMachine<VehicleController>(idle, _vehicle);
        _fsm.RegisterState(driving);
        _fsm.RegisterState(airborne);
        _fsm.RegisterState(destroyed);
        _fsm.Start();
    }

    void OnEnable()
    {
        if (_health != null)
            _health.OnDeath += HandleDeath;
    }

    void OnDisable()
    {
        if (_health != null)
            _health.OnDeath -= HandleDeath;
    }

    void Update() => _fsm?.Update();

    public void TransitionTo<T>() where T : FiniteState<VehicleController>
    {
        var state = _fsm.GetState<T>();
        if (state != null) _fsm.ChangeState(state);
    }

    void HandleDeath() => TransitionTo<VehicleDestroyedState>();
}
