using UnityEngine;

[CreateAssetMenu(menuName = "‽/Vehicle/Drivetrain Profile", fileName = "DrivetrainProfile")]
public class DrivetrainProfile : ScriptableObject
{
    public enum DriveType { FWD, RWD, AWD }

    [Header("Drive")]
    [SerializeField] DriveType driveType = DriveType.RWD;
    [SerializeField] [Range(0f, 1f)] float frontRearBias = 0.5f;

    [Header("Engine")]
    [SerializeField] float maxTorque = 400f;
    [SerializeField] AnimationCurve torqueCurve = AnimationCurve.Linear(0f, 0.8f, 1f, 1f);
    [SerializeField] float idleRPM = 800f;
    [SerializeField] float maxRPM = 7000f;

    [Header("Gears")]
    [SerializeField] float[] gearRatios = { 3.5f, 2.5f, 1.8f, 1.3f, 1.0f, 0.8f };
    [SerializeField] float reverseGearRatio = 3.2f;
    [SerializeField] float finalDriveRatio = 3.7f;
    [SerializeField] float shiftUpRPM = 6500f;
    [SerializeField] float shiftDownRPM = 2500f;
    [SerializeField] float shiftDuration = 0.3f;

    [Header("Braking")]
    [SerializeField] float maxBrakeTorque = 3000f;
    [SerializeField] [Range(0f, 1f)] float brakeBalance = 0.6f;
    [SerializeField] float handbrakeTorque = 2000f;

    public DriveType Type => driveType;
    public float FrontRearBias => frontRearBias;
    public float MaxTorque => maxTorque;
    public AnimationCurve TorqueCurve => torqueCurve;
    public float IdleRPM => idleRPM;
    public float MaxRPM => maxRPM;
    public float[] GearRatios => gearRatios;
    public float ReverseGearRatio => reverseGearRatio;
    public float FinalDriveRatio => finalDriveRatio;
    public float ShiftUpRPM => shiftUpRPM;
    public float ShiftDownRPM => shiftDownRPM;
    public float ShiftDuration => shiftDuration;
    public float MaxBrakeTorque => maxBrakeTorque;
    public float BrakeBalance => brakeBalance;
    public float HandbrakeTorque => handbrakeTorque;

    public int GearCount => gearRatios?.Length ?? 0;

    public float GetGearRatio(int gear) =>
        gear < 0 ? reverseGearRatio :
        gear >= 0 && gear < gearRatios.Length ? gearRatios[gear] : 1f;

    public float GetTotalRatio(int gear) => GetGearRatio(gear) * finalDriveRatio;
}
