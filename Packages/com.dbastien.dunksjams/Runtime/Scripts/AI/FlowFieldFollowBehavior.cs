using UnityEngine;

/// <summary>
/// Steering behavior that follows a flow field. The flow field must be computed externally
/// and assigned to this behavior. The agent moves in the direction indicated by the flow field
/// at its current grid position.
/// </summary>
[CreateAssetMenu(menuName = "‽/Steering/Flow Field Follow")]
public class FlowFieldFollowBehavior : SteeringBehavior
{
    [SerializeField] float cellSize = 1f;
    [SerializeField] Vector3 gridOrigin;

    FlowFieldPathfinder2D _flowField;

    /// <summary>Set the flow field this behavior should follow. Call after computing the field.</summary>
    public void SetFlowField(FlowFieldPathfinder2D flowField) => _flowField = flowField;

    /// <summary>Set the world-space mapping parameters.</summary>
    public void SetGridMapping(Vector3 origin, float size)
    {
        gridOrigin = origin;
        cellSize = size;
    }

    public override Vector3 CalculateForce(SteeringAgent agent, Transform target)
    {
        if (_flowField == null) return Vector3.zero;

        var worldPos = agent.transform.position;
        var gridPos = WorldToGrid(worldPos);
        var flowDir = _flowField.GetFlowDir(gridPos);

        if (flowDir == Vector2.zero) return Vector3.zero;

        var desiredVelocity = new Vector3(flowDir.x, 0f, flowDir.y).normalized * agent.maxSpeed;
        return LimitVelocity(desiredVelocity - agent.RigidBody.linearVelocity, agent.maxSpeed);
    }

    Vector2Int WorldToGrid(Vector3 worldPos) => new(
        Mathf.FloorToInt((worldPos.x - gridOrigin.x) / cellSize),
        Mathf.FloorToInt((worldPos.z - gridOrigin.z) / cellSize)
    );
}
