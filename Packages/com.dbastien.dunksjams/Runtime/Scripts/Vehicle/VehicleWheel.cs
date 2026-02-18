using UnityEngine;

public class VehicleWheel : MonoBehaviour
{
    [Header("Role")] [SerializeField] private bool isSteer;
    [SerializeField] private bool isDrive;
    [SerializeField] private bool isBrake = true;
    [SerializeField] private bool isHandbrake;
    [SerializeField] private bool isFrontAxle;

    [Header("Suspension")] [SerializeField]
    private float suspensionRestLength = 0.35f;

    [SerializeField] private float suspensionTravel = 0.15f;
    [SerializeField] private float springRate = 35000f;
    [SerializeField] private float damperRate = 4500f;

    [Header("Tire")] [SerializeField] private TireProfile tireProfile;

    [Header("Visual")] [SerializeField] private Transform wheelVisual;
    [SerializeField] private bool isLeftSide;

    [Header("Ground Detection")] [SerializeField]
    private float castRadius = 0.05f;

    [SerializeField] private LayerMask groundLayers = ~0;

    // Runtime state (read by VehicleController, effects, telemetry)
    public bool IsGrounded { get; private set; }
    public float Compression { get; private set; }
    public float SuspensionForce { get; private set; }
    public Vector3 ContactPoint { get; private set; }
    public Vector3 ContactNormal { get; private set; }
    public float LongitudinalSlip { get; private set; }
    public float LateralSlipAngle { get; private set; }
    public float CombinedSlip { get; private set; }
    public Vector3 TireForceWorld { get; private set; }
    public GroundSurface CurrentSurface { get; private set; }
    public float AngularVelocity { get; set; }

    // Configuration accessors
    public bool IsSteer => isSteer;
    public bool IsDrive => isDrive;
    public bool IsBrake => isBrake;
    public bool IsHandbrake => isHandbrake;
    public bool IsFrontAxle => isFrontAxle;
    public TireProfile TireProfile => tireProfile;
    public float Radius => tireProfile != null ? tireProfile.Radius : 0.35f;

    private float _steerAngle;
    private float _driveTorque;
    private float _brakeTorque;
    private float _previousLength;
    private Rigidbody _rb;
    private float _wheelSpin;

    public void Initialize(Rigidbody rb)
    {
        _rb = rb;
        _previousLength = suspensionRestLength;
    }

    public void SetSteerAngle(float angle) => _steerAngle = angle;
    public void SetDriveTorque(float torque) => _driveTorque = torque;
    public void SetBrakeTorque(float torque) => _brakeTorque = torque;

    public void UpdateWheel(float dt)
    {
        if (_rb == null || tireProfile == null) return;

        UpdateSteering();
        UpdateSuspension(dt);

        if (IsGrounded)
        {
            UpdateTireForces(dt);
            ApplyForces();
        }
        else
        {
            LongitudinalSlip = 0f;
            LateralSlipAngle = 0f;
            CombinedSlip = 0f;
            TireForceWorld = Vector3.zero;
            SuspensionForce = 0f;

            // Spin down freely in air
            AngularVelocity *= 1f - dt * 0.5f;
        }

        UpdateVisual(dt);
    }

    private void UpdateSteering()
    {
        if (!isSteer) return;
        transform.localRotation = Quaternion.Euler(0f, _steerAngle, 0f);
    }

    private void UpdateSuspension(float dt)
    {
        float maxLength = suspensionRestLength + suspensionTravel;
        float castDist = maxLength + Radius;
        Vector3 origin = transform.position;
        Vector3 down = -transform.up;

        if (Physics.SphereCast(origin, castRadius, down, out RaycastHit hit, castDist, groundLayers,
                QueryTriggerInteraction.Ignore))
        {
            IsGrounded = true;
            ContactPoint = hit.point;
            ContactNormal = hit.normal;

            float springLength = hit.distance - Radius;
            springLength = Mathf.Clamp(springLength, suspensionRestLength - suspensionTravel, maxLength);
            Compression = 1f - (springLength - (suspensionRestLength - suspensionTravel)) / (suspensionTravel * 2f);

            // Spring force (Hooke's law)
            float displacement = suspensionRestLength - springLength;
            float springForce = springRate * displacement;

            // Damper force
            float velocity = (_previousLength - springLength) / dt;
            float damperForce = damperRate * velocity;

            SuspensionForce = Mathf.Max(0f, springForce + damperForce);
            _previousLength = springLength;

            // Ground surface lookup
            CurrentSurface = GroundSurfaceManager.Instance?.GetSurface(hit.collider);
        }
        else
        {
            IsGrounded = false;
            Compression = 0f;
            SuspensionForce = 0f;
            _previousLength = maxLength;
            CurrentSurface = null;
        }
    }

    private void UpdateTireForces(float dt)
    {
        if (tireProfile == null || !IsGrounded) return;

        Vector3 worldVelocity = _rb.GetPointVelocity(ContactPoint);
        Vector3 localForward = transform.forward;
        Vector3 localRight = transform.right;

        float forwardVel = Vector3.Dot(worldVelocity, localForward);
        float lateralVel = Vector3.Dot(worldVelocity, localRight);

        // Longitudinal slip
        float wheelSpeed = AngularVelocity * Radius;
        float absForwardVel = Mathf.Max(Mathf.Abs(forwardVel), 0.5f);
        LongitudinalSlip = (wheelSpeed - forwardVel) / absForwardVel;
        LongitudinalSlip = Mathf.Clamp(LongitudinalSlip, -1f, 1f);

        // Lateral slip angle (degrees)
        LateralSlipAngle = Mathf.Atan2(lateralVel, absForwardVel) * Mathf.Rad2Deg;

        // Combined slip magnitude for effects
        float normLongSlip = Mathf.Abs(LongitudinalSlip);
        float normLatSlip = Mathf.Abs(LateralSlipAngle) / 90f;
        CombinedSlip = Mathf.Sqrt(normLongSlip * normLongSlip + normLatSlip * normLatSlip);

        // Ground surface grip modifier
        float surfaceGrip = CurrentSurface != null ? CurrentSurface.GripMultiplier : 1f;
        float surfaceDrag = CurrentSurface != null ? CurrentSurface.DragMultiplier : 0f;

        // Evaluate friction from tire profile curves
        float longFriction = tireProfile.EvaluateLongitudinal(LongitudinalSlip) * surfaceGrip;
        float latFriction = tireProfile.EvaluateLateral(LateralSlipAngle) * surfaceGrip;

        // Normal load
        float normalLoad = SuspensionForce;

        // Tire forces
        float longForce = -longFriction * normalLoad * Mathf.Sign(LongitudinalSlip);
        float latForce = -latFriction * normalLoad * Mathf.Sign(LateralSlipAngle);

        // Drive torque -> force at contact
        float driveForce = _driveTorque / Radius;
        longForce += driveForce;

        // Brake torque
        if (_brakeTorque > 0f)
        {
            float brakeForce = _brakeTorque / Radius;
            float brakeDecel = -Mathf.Sign(forwardVel) * Mathf.Min(brakeForce, Mathf.Abs(forwardVel) * normalLoad);
            longForce += brakeDecel;
        }

        // Rolling resistance
        float rollingRes = tireProfile.RollingResistance;
        float surfaceRollingRes = CurrentSurface != null ? CurrentSurface.RollingResistanceMultiplier : 1f;
        longForce -= rollingRes * surfaceRollingRes * normalLoad * Mathf.Sign(forwardVel);

        // Surface drag
        longForce -= surfaceDrag * normalLoad * forwardVel;

        TireForceWorld = localForward * longForce + localRight * latForce;

        // Update wheel angular velocity from ground contact
        float effectiveForwardForce = Vector3.Dot(TireForceWorld, localForward);
        float angularAccel = effectiveForwardForce * Radius / (Radius * Radius * 10f); // simplified inertia
        AngularVelocity = forwardVel / Radius + angularAccel * dt;
    }

    private void ApplyForces()
    {
        if (_rb == null) return;

        // Suspension
        _rb.AddForceAtPosition(transform.up * SuspensionForce, transform.position);

        // Tire
        if (TireForceWorld.sqrMagnitude > 0.001f)
            _rb.AddForceAtPosition(TireForceWorld, ContactPoint);
    }

    private void UpdateVisual(float dt)
    {
        if (wheelVisual == null) return;

        // Position: drop visual to contact or full extension
        float visualDrop;
        if (IsGrounded)
        {
            float springLength = _previousLength;
            visualDrop = springLength + Radius;
        }
        else { visualDrop = suspensionRestLength + suspensionTravel + Radius; }

        wheelVisual.position = transform.position - transform.up * visualDrop;

        // Spin
        _wheelSpin += AngularVelocity * Mathf.Rad2Deg * dt;
        float spinAxis = isLeftSide ? -1f : 1f;
        wheelVisual.localRotation = Quaternion.Euler(spinAxis * _wheelSpin, _steerAngle, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        float maxLength = suspensionRestLength + suspensionTravel;
        Vector3 origin = transform.position;
        Vector3 down = -transform.up;

        // Suspension range
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, origin + down * (suspensionRestLength - suspensionTravel + Radius));

        Gizmos.color = Color.green;
        Gizmos.DrawLine(origin + down * (suspensionRestLength - suspensionTravel + Radius),
            origin + down * (suspensionRestLength + Radius));

        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin + down * (suspensionRestLength + Radius),
            origin + down * (maxLength + Radius));

        // Wheel sphere
        Gizmos.color = IsGrounded ? Color.green : Color.gray;
        Gizmos.DrawWireSphere(origin + down * (suspensionRestLength + Radius), Radius);

        // Contact point
        if (IsGrounded)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(ContactPoint, 0.03f);

            // Force vector
            if (TireForceWorld.sqrMagnitude > 1f)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(ContactPoint, TireForceWorld.normalized * 0.5f);
            }
        }
    }
}