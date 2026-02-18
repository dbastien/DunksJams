using UnityEngine;

public abstract class TargetingStrategy : ScriptableObject
{
    public abstract Transform GetTarget(SteeringAgent agent);
}

[CreateAssetMenu(menuName = "Targeting/No Target")]
public class NoTarget : TargetingStrategy
{
    public override Transform GetTarget(SteeringAgent agent) => null;
}

[CreateAssetMenu(menuName = "Targeting/Fixed Target")]
public class FixedTarget : TargetingStrategy
{
    public Transform target;
    public override Transform GetTarget(SteeringAgent agent) => target;
}

[CreateAssetMenu(menuName = "Targeting/Nearest Target")]
public class NearestTarget : TargetingStrategy
{
    public Transform[] potentialTargets;

    public override Transform GetTarget(SteeringAgent agent)
    {
        if (potentialTargets == null || potentialTargets.Length == 0) return null;

        Transform nearest = null;
        float closestDistance = Mathf.Infinity;
        Vector3 agentPosition = agent.transform.position;

        foreach (Transform target in potentialTargets)
        {
            if (!target) continue;

            float distance = Vector3.Distance(agentPosition, target.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearest = target;
            }
        }

        return nearest;
    }
}