using UnityEngine;

/// <summary>
/// AI driver that steers a VehicleController toward a target using waypoints or a Transform.
/// Uses the vehicle's SetInput API rather than direct physics, so all driving aids and
/// drivetrain logic work identically for AI and players.
/// </summary>
[RequireComponent(typeof(VehicleController))]
public class VehicleAI : MonoBehaviour
{
    [Header("Target")] [SerializeField] private Transform target;
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float waypointReachDistance = 5f;
    [SerializeField] private bool loop = true;

    [Header("Driving")] [SerializeField] private float maxSpeedToTarget = 30f;
    [SerializeField] private float brakeAngleThreshold = 40f;
    [SerializeField] private float brakeSpeedThreshold = 15f;
    [SerializeField] [Range(0f, 1f)] private float throttleAmount = 0.8f;

    [Header("Avoidance")] [SerializeField] private float avoidanceRayLength = 10f;
    [SerializeField] private float avoidanceWidth = 1.5f;
    [SerializeField] private LayerMask obstacleLayers = ~0;

    private VehicleController _vehicle;
    private int _currentWaypointIndex;
    private Transform _currentTarget;

    private void Awake() => _vehicle = GetComponent<VehicleController>();

    private void Update()
    {
        UpdateCurrentTarget();
        if (_currentTarget == null)
        {
            _vehicle.SetInput(0f, 0f, 1f, false);
            return;
        }

        Vector3 toTarget = _currentTarget.position - transform.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;
        float angle = Vector3.SignedAngle(transform.forward, toTarget, Vector3.up);

        // Steering
        float steer = Mathf.Clamp(angle / _vehicle.Profile.MaxSteerAngle, -1f, 1f);

        // Obstacle avoidance
        steer += GetAvoidanceSteer();
        steer = Mathf.Clamp(steer, -1f, 1f);

        // Throttle and braking
        float absAngle = Mathf.Abs(angle);
        float speed = Mathf.Abs(_vehicle.ForwardSpeed);
        bool shouldBrake = absAngle > brakeAngleThreshold && speed > brakeSpeedThreshold;

        float throttle = shouldBrake ? 0f : throttleAmount;
        float brake = shouldBrake ? 0.5f : 0f;

        // Slow down near target if not using waypoints
        if (waypoints == null || waypoints.Length == 0)
            if (distance < 10f)
                throttle *= distance / 10f;

        // Speed limiting
        if (speed > maxSpeedToTarget) throttle = 0f;

        _vehicle.SetInput(throttle, steer, brake, false);
    }

    private void UpdateCurrentTarget()
    {
        if (waypoints != null && waypoints.Length > 0)
        {
            _currentTarget = waypoints[_currentWaypointIndex];
            float dist = Vector3.Distance(transform.position, _currentTarget.position);

            if (dist < waypointReachDistance)
            {
                _currentWaypointIndex++;
                if (_currentWaypointIndex >= waypoints.Length)
                    _currentWaypointIndex = loop ? 0 : waypoints.Length - 1;

                _currentTarget = waypoints[_currentWaypointIndex];
            }
        }
        else { _currentTarget = target; }
    }

    private float GetAvoidanceSteer()
    {
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        var steerAdjust = 0f;

        // Three raycasts: center, left, right
        if (Physics.Raycast(origin, forward, avoidanceRayLength, obstacleLayers))
            steerAdjust += 0.5f; // default steer right

        if (Physics.Raycast(origin, (forward + right * 0.5f).normalized, avoidanceRayLength * 0.8f, obstacleLayers))
            steerAdjust -= 0.3f;

        if (Physics.Raycast(origin, (forward - right * 0.5f).normalized, avoidanceRayLength * 0.8f, obstacleLayers))
            steerAdjust += 0.3f;

        return steerAdjust;
    }

    private void OnDrawGizmosSelected()
    {
        if (waypoints == null) return;

        Gizmos.color = Color.yellow;
        for (var i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawSphere(waypoints[i].position, 0.5f);
            if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            else if (loop && i == waypoints.Length - 1 && waypoints[0] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[0].position);
        }

        if (_currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _currentTarget.position);
        }

        // Avoidance rays
        Gizmos.color = Color.cyan;
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Gizmos.DrawRay(origin, transform.forward * avoidanceRayLength);
        Gizmos.DrawRay(origin, (transform.forward + transform.right * 0.5f).normalized * avoidanceRayLength * 0.8f);
        Gizmos.DrawRay(origin, (transform.forward - transform.right * 0.5f).normalized * avoidanceRayLength * 0.8f);
    }
}