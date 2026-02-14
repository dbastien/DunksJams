using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class VehicleController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] VehicleProfile profile;
    [SerializeField] DrivetrainProfile drivetrain;

    [Header("Wheels")]
    [SerializeField] VehicleWheel[] wheels;

    [Header("Center of Mass")]
    [SerializeField] Transform centerOfMassOverride;

    Rigidbody _rb;
    VehicleWheel[] _frontWheels;
    VehicleWheel[] _rearWheels;
    VehicleWheel[] _driveWheels;
    VehicleWheel[] _steerWheels;

    // Drivetrain state
    int _currentGear;
    float _engineRPM;
    float _shiftTimer;
    bool _isShifting;

    // Input state (set by VehicleInput or AI)
    float _throttleInput;
    float _steerInput;
    float _brakeInput;
    bool _handbrakeInput;

    // Driving aids
    TractionControl _tractionControl;
    BrakeAssist _brakeAssist;
    SteeringAssist _steeringAssist;

    // Computed values
    float _forwardSpeed;
    float _speedNormalized;
    float _wheelBase;
    float _rearTrack;

    // Public API
    public VehicleProfile Profile => profile;
    public DrivetrainProfile Drivetrain => drivetrain;
    public Rigidbody Rb => _rb;
    public IReadOnlyList<VehicleWheel> Wheels => wheels;
    public float ForwardSpeed => _forwardSpeed;
    public float SpeedKmh => _forwardSpeed * 3.6f;
    public float SpeedMph => _forwardSpeed * 2.237f;
    public float SpeedNormalized => _speedNormalized;
    public int CurrentGear => _currentGear;
    public float EngineRPM => _engineRPM;
    public bool IsShifting => _isShifting;
    public bool IsGrounded { get; private set; }
    public float ThrottleInput => _throttleInput;
    public float SteerInput => _steerInput;
    public float BrakeInput => _brakeInput;
    public bool HandbrakeInput => _handbrakeInput;

    public event Action<Vector3, Vector3, float> OnImpact;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        if (wheels == null || wheels.Length == 0)
            wheels = GetComponentsInChildren<VehicleWheel>();

        CategorizeWheels();
        InitializeWheels();
        SetupCenterOfMass();
        SetupRigidbody();
        InitializeDrivingAids();
        CalculateGeometry();
    }

    void OnEnable()
    {
        VehicleManager.Instance?.Register(this);
        EventManager.QueueEvent<VehicleSpawnEvent>(e => e.Vehicle = this);
    }

    void OnDisable()
    {
        VehicleManager.Instance?.Unregister(this);
        EventManager.QueueEvent<VehicleDestroyEvent>(e => e.Vehicle = this);
    }

    void FixedUpdate()
    {
        if (profile == null || drivetrain == null) return;

        float dt = Time.fixedDeltaTime;

        ComputeSpeed();
        UpdateGearbox(dt);
        ApplySteering();
        ApplyDriveTorque();
        ApplyBrakes();

        foreach (var w in wheels)
            w.UpdateWheel(dt);

        ApplyAerodynamics();
        ApplyAntiRoll();
        UpdateGroundedState();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.impulse.magnitude < 50f) return;

        var contact = collision.GetContact(0);
        var surface = GroundSurfaceManager.Instance?.GetSurface(collision.collider);

        OnImpact?.Invoke(contact.point, contact.normal, collision.impulse.magnitude);

        EventManager.QueueEvent<VehicleImpactEvent>(e =>
        {
            e.Vehicle = this;
            e.Point = contact.point;
            e.Normal = contact.normal;
            e.Impulse = collision.impulse.magnitude;
            e.Surface = surface;
        });
    }

    // --- Input API (called by VehicleInput or AI) ---

    public void SetInput(float throttle, float steer, float brake, bool handbrake)
    {
        _throttleInput = Mathf.Clamp(throttle, -1f, 1f);
        _steerInput = Mathf.Clamp(steer, -1f, 1f);
        _brakeInput = Mathf.Clamp01(brake);
        _handbrakeInput = handbrake;
    }

    // --- Setup ---

    void CategorizeWheels()
    {
        var front = new List<VehicleWheel>();
        var rear = new List<VehicleWheel>();
        var drive = new List<VehicleWheel>();
        var steer = new List<VehicleWheel>();

        foreach (var w in wheels)
        {
            if (w.IsFrontAxle) front.Add(w);
            else rear.Add(w);

            if (w.IsDrive) drive.Add(w);
            if (w.IsSteer) steer.Add(w);
        }

        _frontWheels = front.ToArray();
        _rearWheels = rear.ToArray();
        _driveWheels = drive.ToArray();
        _steerWheels = steer.ToArray();
    }

    void InitializeWheels()
    {
        foreach (var w in wheels)
            w.Initialize(_rb);
    }

    void SetupCenterOfMass()
    {
        if (centerOfMassOverride != null)
        {
            _rb.centerOfMass = transform.InverseTransformPoint(centerOfMassOverride.position);
            return;
        }

        if (profile == null || wheels.Length == 0) return;

        // Parametric CoM between front and rear axle centers
        var frontCenter = Vector3.zero;
        var rearCenter = Vector3.zero;

        foreach (var w in _frontWheels)
            frontCenter += w.transform.localPosition;
        foreach (var w in _rearWheels)
            rearCenter += w.transform.localPosition;

        if (_frontWheels.Length > 0) frontCenter /= _frontWheels.Length;
        if (_rearWheels.Length > 0) rearCenter /= _rearWheels.Length;

        var com = Vector3.Lerp(rearCenter, frontCenter, profile.CenterOfMassPosition);
        com.y += profile.CenterOfMassHeightOffset;
        _rb.centerOfMass = com;
    }

    void SetupRigidbody()
    {
        if (profile != null)
            _rb.mass = profile.Mass;

        _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void InitializeDrivingAids()
    {
        _tractionControl = new TractionControl();
        _brakeAssist = new BrakeAssist();
        _steeringAssist = new SteeringAssist();
    }

    void CalculateGeometry()
    {
        if (_frontWheels.Length == 0 || _rearWheels.Length == 0) return;

        var frontCenter = Vector3.zero;
        var rearCenter = Vector3.zero;
        foreach (var w in _frontWheels) frontCenter += w.transform.localPosition;
        foreach (var w in _rearWheels) rearCenter += w.transform.localPosition;
        frontCenter /= _frontWheels.Length;
        rearCenter /= _rearWheels.Length;

        _wheelBase = Mathf.Abs(frontCenter.z - rearCenter.z);

        if (_rearWheels.Length >= 2)
        {
            float minX = float.MaxValue, maxX = float.MinValue;
            foreach (var w in _rearWheels)
            {
                float x = w.transform.localPosition.x;
                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
            }
            _rearTrack = maxX - minX;
        }
    }

    // --- Physics Update Steps ---

    void ComputeSpeed()
    {
        _forwardSpeed = Vector3.Dot(_rb.linearVelocity, transform.forward);

        float maxSpeed = _forwardSpeed >= 0 ? profile.MaxForwardSpeed : profile.MaxReverseSpeed;
        _speedNormalized = Mathf.Clamp01(Mathf.Abs(_forwardSpeed) / maxSpeed);
    }

    void UpdateGearbox(float dt)
    {
        if (_isShifting)
        {
            _shiftTimer -= dt;
            if (_shiftTimer <= 0f) _isShifting = false;
            return;
        }

        // Auto-shift logic
        float speedForRPM = Mathf.Abs(_forwardSpeed);
        float wheelRadius = _driveWheels.Length > 0 ? _driveWheels[0].Radius : 0.35f;
        float wheelRPM = speedForRPM / (2f * Mathf.PI * wheelRadius) * 60f;
        float totalRatio = drivetrain.GetTotalRatio(_currentGear);
        _engineRPM = Mathf.Clamp(wheelRPM * totalRatio, drivetrain.IdleRPM, drivetrain.MaxRPM);

        int prevGear = _currentGear;

        // Shift up
        if (_currentGear >= 0 && _currentGear < drivetrain.GearCount - 1 && _engineRPM >= drivetrain.ShiftUpRPM)
        {
            _currentGear++;
            _isShifting = true;
            _shiftTimer = drivetrain.ShiftDuration;
        }
        // Shift down
        else if (_currentGear > 0 && _engineRPM <= drivetrain.ShiftDownRPM)
        {
            _currentGear--;
            _isShifting = true;
            _shiftTimer = drivetrain.ShiftDuration;
        }
        // Reverse
        else if (_throttleInput < -0.1f && Mathf.Abs(_forwardSpeed) < 1f && _currentGear >= 0)
        {
            _currentGear = -1;
        }
        else if (_throttleInput > 0.1f && _currentGear < 0)
        {
            _currentGear = 0;
        }

        if (prevGear != _currentGear)
        {
            EventManager.QueueEvent<VehicleGearChangeEvent>(e =>
            {
                e.Vehicle = this;
                e.PreviousGear = prevGear;
                e.NewGear = _currentGear;
            });
        }
    }

    void ApplySteering()
    {
        if (profile == null) return;

        float speedFactor = profile.SteerSpeedCurve.Evaluate(_speedNormalized);
        float baseAngle = _steerInput * profile.MaxSteerAngle * speedFactor;

        // Steering assist (countersteer)
        if (profile.SteeringAssistStrength > 0f)
            baseAngle = _steeringAssist.Apply(baseAngle, this);

        if (profile.UseAckermannSteering && _wheelBase > 0.01f && _rearTrack > 0.01f &&
            Mathf.Abs(baseAngle) > 0.1f)
        {
            ApplyAckermannSteering(baseAngle);
        }
        else
        {
            foreach (var w in _steerWheels)
                w.SetSteerAngle(baseAngle);
        }
    }

    void ApplyAckermannSteering(float baseAngle)
    {
        float turnRadius = _wheelBase / Mathf.Tan(baseAngle * Mathf.Deg2Rad);
        float innerAngle = Mathf.Atan(_wheelBase / (turnRadius - _rearTrack * 0.5f)) * Mathf.Rad2Deg;
        float outerAngle = Mathf.Atan(_wheelBase / (turnRadius + _rearTrack * 0.5f)) * Mathf.Rad2Deg;

        foreach (var w in _steerWheels)
        {
            float localX = w.transform.localPosition.x;
            bool isInner = (baseAngle > 0f && localX > 0f) || (baseAngle < 0f && localX < 0f);
            w.SetSteerAngle(isInner ? innerAngle * Mathf.Sign(baseAngle) : outerAngle * Mathf.Sign(baseAngle));
        }
    }

    void ApplyDriveTorque()
    {
        if (_driveWheels.Length == 0 || _isShifting) return;

        float throttle = Mathf.Abs(_throttleInput);
        if (_currentGear < 0) throttle = Mathf.Abs(Mathf.Min(_throttleInput, 0f));

        // Speed limiter
        float maxSpeed = _currentGear < 0 ? profile.MaxReverseSpeed : profile.MaxForwardSpeed;
        float speedRatio = Mathf.Abs(_forwardSpeed) / maxSpeed;
        if (speedRatio > 0.95f) throttle *= Mathf.Max(0f, 1f - (speedRatio - 0.95f) * 20f);

        // Engine torque
        float rpmNorm = Mathf.InverseLerp(drivetrain.IdleRPM, drivetrain.MaxRPM, _engineRPM);
        float torqueMultiplier = drivetrain.TorqueCurve.Evaluate(rpmNorm);
        float engineTorque = drivetrain.MaxTorque * torqueMultiplier * throttle;
        float totalRatio = drivetrain.GetTotalRatio(_currentGear);
        float wheelTorque = engineTorque * totalRatio;

        if (_currentGear < 0) wheelTorque = -wheelTorque;

        // Traction control
        if (profile.TractionControlStrength > 0f)
            wheelTorque = _tractionControl.Apply(wheelTorque, _driveWheels, profile.TractionControlStrength);

        // Distribute to drive wheels
        float[] distribution = ComputeDriveDistribution();
        for (int i = 0; i < _driveWheels.Length; i++)
            _driveWheels[i].SetDriveTorque(wheelTorque * distribution[i]);
    }

    float[] ComputeDriveDistribution()
    {
        var dist = new float[_driveWheels.Length];

        if (drivetrain.Type == DrivetrainProfile.DriveType.AWD && _frontWheels.Length > 0 && _rearWheels.Length > 0)
        {
            float frontShare = drivetrain.FrontRearBias;
            float rearShare = 1f - frontShare;
            int frontDriveCount = 0, rearDriveCount = 0;

            for (int i = 0; i < _driveWheels.Length; i++)
            {
                if (_driveWheels[i].IsFrontAxle) frontDriveCount++;
                else rearDriveCount++;
            }

            for (int i = 0; i < _driveWheels.Length; i++)
            {
                if (_driveWheels[i].IsFrontAxle)
                    dist[i] = frontDriveCount > 0 ? frontShare / frontDriveCount : 0f;
                else
                    dist[i] = rearDriveCount > 0 ? rearShare / rearDriveCount : 0f;
            }
        }
        else
        {
            float share = 1f / _driveWheels.Length;
            for (int i = 0; i < dist.Length; i++)
                dist[i] = share;
        }

        return dist;
    }

    void ApplyBrakes()
    {
        float brakeTorque = _brakeInput * drivetrain.MaxBrakeTorque;

        // Brake assist (ABS)
        if (profile.BrakeAssistStrength > 0f)
            brakeTorque = _brakeAssist.Apply(brakeTorque, wheels, profile.BrakeAssistStrength);

        foreach (var w in wheels)
        {
            float torque = 0f;

            if (w.IsBrake)
            {
                float balance = w.IsFrontAxle ? drivetrain.BrakeBalance : (1f - drivetrain.BrakeBalance);
                torque += brakeTorque * balance;
            }

            if (w.IsHandbrake && _handbrakeInput)
                torque += drivetrain.HandbrakeTorque;

            w.SetBrakeTorque(torque);
        }
    }

    void ApplyAerodynamics()
    {
        float speed = _forwardSpeed;
        float speedSqr = speed * speed;
        var forward = transform.forward;

        // Drag (opposes motion)
        float dragForce = profile.DragCoefficient * speedSqr * Mathf.Sign(speed);
        _rb.AddForce(-forward * dragForce);

        // Downforce
        float downforce = profile.DownforceCoefficient * speedSqr;
        if (downforce > 0.01f)
        {
            float frontShare = profile.AeroBalance;
            float rearShare = 1f - frontShare;

            if (_frontWheels.Length > 0)
            {
                var frontPos = Vector3.zero;
                foreach (var w in _frontWheels) frontPos += w.transform.position;
                frontPos /= _frontWheels.Length;
                _rb.AddForceAtPosition(-transform.up * downforce * frontShare, frontPos);
            }

            if (_rearWheels.Length > 0)
            {
                var rearPos = Vector3.zero;
                foreach (var w in _rearWheels) rearPos += w.transform.position;
                rearPos /= _rearWheels.Length;
                _rb.AddForceAtPosition(-transform.up * downforce * rearShare, rearPos);
            }
        }
    }

    void ApplyAntiRoll()
    {
        if (profile.AntiRollStrength <= 0f) return;

        ApplyAntiRollForAxle(_frontWheels);
        ApplyAntiRollForAxle(_rearWheels);
    }

    void ApplyAntiRollForAxle(VehicleWheel[] axle)
    {
        if (axle.Length < 2) return;

        // Simple pair-based anti-roll
        var left = axle[0];
        var right = axle[1];

        float leftComp = left.IsGrounded ? left.Compression : 0f;
        float rightComp = right.IsGrounded ? right.Compression : 0f;
        float diff = leftComp - rightComp;

        float antiRollForce = diff * profile.AntiRollStrength * _rb.mass * Physics.gravity.magnitude;

        if (left.IsGrounded)
            _rb.AddForceAtPosition(-transform.up * antiRollForce, left.transform.position);
        if (right.IsGrounded)
            _rb.AddForceAtPosition(transform.up * antiRollForce, right.transform.position);
    }

    void UpdateGroundedState()
    {
        IsGrounded = false;
        foreach (var w in wheels)
        {
            if (w.IsGrounded) { IsGrounded = true; break; }
        }
    }
}
