using UnityEngine;

public class VehicleIdleState : FiniteState<VehicleController>
{
    public VehicleIdleState(VehicleController owner) : base(owner) { }

    protected override void OnUpdate()
    {
        if (!Owner.IsGrounded)
            Owner.GetComponent<VehicleFSM>()?.TransitionTo<VehicleAirborneState>();
        else if (Mathf.Abs(Owner.ForwardSpeed) > 0.5f || Mathf.Abs(Owner.ThrottleInput) > 0.1f)
            Owner.GetComponent<VehicleFSM>()?.TransitionTo<VehicleDrivingState>();
    }
}

public class VehicleDrivingState : FiniteState<VehicleController>
{
    public VehicleDrivingState(VehicleController owner) : base(owner) { }

    protected override void OnUpdate()
    {
        if (!Owner.IsGrounded)
            Owner.GetComponent<VehicleFSM>()?.TransitionTo<VehicleAirborneState>();
        else if (Mathf.Abs(Owner.ForwardSpeed) < 0.3f && Mathf.Abs(Owner.ThrottleInput) < 0.05f)
            Owner.GetComponent<VehicleFSM>()?.TransitionTo<VehicleIdleState>();
    }
}

public class VehicleAirborneState : FiniteState<VehicleController>
{
    public VehicleAirborneState(VehicleController owner) : base(owner) { }

    protected override void OnUpdate()
    {
        if (Owner.IsGrounded)
        {
            if (Mathf.Abs(Owner.ForwardSpeed) > 0.5f)
                Owner.GetComponent<VehicleFSM>()?.TransitionTo<VehicleDrivingState>();
            else
                Owner.GetComponent<VehicleFSM>()?.TransitionTo<VehicleIdleState>();
        }
    }
}

public class VehicleDestroyedState : FiniteState<VehicleController>
{
    public VehicleDestroyedState(VehicleController owner) : base(owner) { }

    protected override void OnEnter(params object[] parameters) =>
        DLog.Log($"Vehicle {Owner.name} destroyed");
}
