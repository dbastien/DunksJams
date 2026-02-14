using UnityEngine;

[CreateAssetMenu(menuName = "‽/Vehicle/Vehicle Profile", fileName = "VehicleProfile")]
public class VehicleProfile : ScriptableObject
{
    [Header("Mass")]
    [SerializeField] float mass = 1500f;
    [SerializeField] [Range(0f, 1f)] float centerOfMassPosition = 0.45f;
    [SerializeField] float centerOfMassHeightOffset = -0.1f;

    [Header("Aerodynamics")]
    [SerializeField] float dragCoefficient = 0.3f;
    [SerializeField] float downforceCoefficient = 0.5f;
    [SerializeField] [Range(0f, 1f)] float aeroBalance = 0.5f;

    [Header("Speed")]
    [SerializeField] float maxForwardSpeed = 50f;
    [SerializeField] float maxReverseSpeed = 15f;

    [Header("Stability")]
    [SerializeField] [Range(0f, 1f)] float antiRollStrength = 0.5f;

    [Header("Steering")]
    [SerializeField] float maxSteerAngle = 35f;
    [SerializeField] AnimationCurve steerSpeedCurve = AnimationCurve.Linear(0f, 1f, 1f, 0.2f);
    [SerializeField] bool useAckermannSteering = true;

    [Header("Driving Aids")]
    [SerializeField] [Range(0f, 1f)] float tractionControlStrength = 0.5f;
    [SerializeField] [Range(0f, 1f)] float brakeAssistStrength = 0.5f;
    [SerializeField] [Range(0f, 1f)] float steeringAssistStrength = 0.5f;

    public float Mass => mass;
    public float CenterOfMassPosition => centerOfMassPosition;
    public float CenterOfMassHeightOffset => centerOfMassHeightOffset;
    public float DragCoefficient => dragCoefficient;
    public float DownforceCoefficient => downforceCoefficient;
    public float AeroBalance => aeroBalance;
    public float MaxForwardSpeed => maxForwardSpeed;
    public float MaxReverseSpeed => maxReverseSpeed;
    public float AntiRollStrength => antiRollStrength;
    public float MaxSteerAngle => maxSteerAngle;
    public AnimationCurve SteerSpeedCurve => steerSpeedCurve;
    public bool UseAckermannSteering => useAckermannSteering;
    public float TractionControlStrength => tractionControlStrength;
    public float BrakeAssistStrength => brakeAssistStrength;
    public float SteeringAssistStrength => steeringAssistStrength;
}
