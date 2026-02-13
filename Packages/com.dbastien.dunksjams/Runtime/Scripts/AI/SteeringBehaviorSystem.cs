using UnityEngine;

public abstract class SteeringBehavior : ScriptableObject
{
    [Range(0, 1)] public float weight = 1f;
    public abstract Vector3 CalculateForce(SteeringAgent agent, Transform target);

    protected static Vector3 LimitVelocity(Vector3 velocity, float maxSpeed) =>
        velocity.magnitude > maxSpeed ? velocity.normalized * maxSpeed : velocity;

    protected static Vector3 SeekVelocity(SteeringAgent agent, Vector3 targetPos)
    {
        var desiredVelocity = (targetPos - agent.transform.position).normalized * agent.maxSpeed;
        return LimitVelocity(desiredVelocity - agent.RigidBody.linearVelocity, agent.maxSpeed);
    }

    protected static Vector3 FleeVelocity(SteeringAgent agent, Vector3 targetPos)
    {
        var desiredVelocity = (agent.transform.position - targetPos).normalized * agent.maxSpeed;
        return LimitVelocity(desiredVelocity - agent.RigidBody.linearVelocity, agent.maxSpeed);
    }

    protected static Vector3 PredictFuturePosition(SteeringAgent agent, Transform target)
    {
        var targetRb = target.GetComponent<Rigidbody>();
        if (!targetRb) return target.position;

        var toTarget = target.position - agent.transform.position;
        var prediction = toTarget.magnitude / (agent.maxSpeed + targetRb.linearVelocity.magnitude);
        return target.position + targetRb.linearVelocity * prediction;
    }
}

[CreateAssetMenu(menuName = "Steering/Seek")]
public class Seek : SteeringBehavior
{
    public override Vector3 CalculateForce(SteeringAgent agent, Transform target) =>
        target ? SeekVelocity(agent, target.position) : Vector3.zero;
}

[CreateAssetMenu(menuName = "Steering/Flee")]
public class Flee : SteeringBehavior
{
    public override Vector3 CalculateForce(SteeringAgent agent, Transform target) =>
        target ? FleeVelocity(agent, target.position) : Vector3.zero;
}

[CreateAssetMenu(menuName = "Steering/Arrive")]
public class Arrive : SteeringBehavior
{
    public float slowingRadius = 5f;

    public override Vector3 CalculateForce(SteeringAgent agent, Transform target)
    {
        if (!target) return Vector3.zero;

        var toTarget = target.position - agent.transform.position;
        var distance = toTarget.magnitude;

        if (distance < 0.01f) return Vector3.zero;

        // slow down as we approach target
        var speed = agent.maxSpeed;
        if (distance < slowingRadius) speed = agent.maxSpeed * (distance / slowingRadius);

        var desiredVelocity = toTarget.normalized * speed;
        return LimitVelocity(desiredVelocity - agent.RigidBody.linearVelocity, agent.maxSpeed);
    }
}

[CreateAssetMenu(menuName = "Steering/Pursue")]
public class Pursue : SteeringBehavior
{
    public override Vector3 CalculateForce(SteeringAgent agent, Transform target)
    {
        if (!target) return Vector3.zero;
        var futurePosition = PredictFuturePosition(agent, target);
        return SeekVelocity(agent, futurePosition);
    }
}

[CreateAssetMenu(menuName = "Steering/Evade")]
public class Evade : SteeringBehavior
{
    public override Vector3 CalculateForce(SteeringAgent agent, Transform target)
    {
        if (!target) return Vector3.zero;
        var futurePosition = PredictFuturePosition(agent, target);
        return FleeVelocity(agent, futurePosition);
    }
}

[CreateAssetMenu(menuName = "Steering/Wander")]
public class Wander : SteeringBehavior
{
    public float circleDistance = 2f;
    public float circleRadius = 1f;
    public float wanderAngleChange = 0.3f;

    float wanderAngle = 0f;

    public override Vector3 CalculateForce(SteeringAgent agent, Transform target)
    {
        var rb = agent.RigidBody;

        var circleCenter = rb.linearVelocity.normalized * circleDistance;

        var displacement = new Vector3(0, 0, -1) * circleRadius;
        wanderAngle += Random.Range(-wanderAngleChange, wanderAngleChange);
        displacement = Quaternion.Euler(0, wanderAngle * Mathf.Rad2Deg, 0) * displacement;

        return circleCenter + displacement;
    }
}

public abstract class NeighborBasedBehavior : SteeringBehavior
{
    public float neighborRadius = 5f;
    protected Collider[] results = new Collider[8];

    protected int GetNeighbors(SteeringAgent agent) =>
        Physics.OverlapSphereNonAlloc(agent.transform.position, neighborRadius, results);
}

public class Cohesion : NeighborBasedBehavior
{
    public override Vector3 CalculateForce(SteeringAgent agent, Transform target)
    {
        var hitCount = GetNeighbors(agent);
        var centerOfMass = Vector3.zero;
        var count = 0;

        for (var i = 0; i < hitCount; ++i)
        {
            if (results[i] == agent.Collider) continue;
            centerOfMass += results[i].transform.position;
            ++count;
        }

        if (count == 0) return Vector3.zero;

        centerOfMass /= count;
        return SeekVelocity(agent, centerOfMass);
    }
}

public class Separation : NeighborBasedBehavior
{
    public float separationFactor = 2f;

    public override Vector3 CalculateForce(SteeringAgent agent, Transform target)
    {
        var hitCount = GetNeighbors(agent);
        var separationForce = Vector3.zero;

        for (var i = 0; i < hitCount; ++i)
        {
            if (results[i] == agent.Collider) continue;
            var toAgent = agent.transform.position - results[i].transform.position;
            separationForce += toAgent.normalized / toAgent.magnitude;
        }

        return separationForce * separationFactor;
    }

    [CreateAssetMenu(menuName = "Steering/Idle")]
    public class Idle : SteeringBehavior
    {
        public override Vector3 CalculateForce(SteeringAgent agent, Transform target) => Vector3.zero;
    }

    [CreateAssetMenu(menuName = "Steering/Align")]
    public class Align : SteeringBehavior
    {
        public override Vector3 CalculateForce(SteeringAgent agent, Transform target)
        {
            if (!target) return Vector3.zero;
            var targetRb = target.GetComponent<Rigidbody>();
            return targetRb ? SeekVelocity(agent, target.position + targetRb.linearVelocity) : Vector3.zero;
        }
    }
}