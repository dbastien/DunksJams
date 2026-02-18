using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SteeringAgent : MonoBehaviour
{
    public float maxSpeed = 10f;
    public float maxForce = 10f;
    public SteeringBehavior[] behaviors;
    public TargetingStrategy targetingStrategy;

    public Rigidbody RigidBody { get; private set; }
    public Collider Collider { get; private set; }

    private void Awake()
    {
        Collider = GetComponent<Collider>();
        RigidBody = GetComponent<Rigidbody>();
        RigidBody.linearDamping = 0f;
        RigidBody.angularDamping = 0f;
    }

    private void FixedUpdate()
    {
        if (behaviors == null || behaviors.Length == 0) return;

        Transform target = targetingStrategy?.GetTarget(this);
        if (!target) return;

        Vector3 totalForce = Vector3.zero;

        foreach (SteeringBehavior behavior in behaviors)
        {
            if (!behavior) continue;
            totalForce += behavior.CalculateForce(this, target) * behavior.weight;
        }

        ApplyForce(totalForce);
    }

    private void ApplyForce(Vector3 force)
    {
        force = Vector3.ClampMagnitude(force, maxForce);
        RigidBody.AddForce(force);
        RigidBody.linearVelocity = Vector3.ClampMagnitude(RigidBody.linearVelocity, maxSpeed);

        if (RigidBody.linearVelocity.magnitude > 0.01f)
            RigidBody.rotation = Quaternion.LookRotation(RigidBody.linearVelocity);
    }
}