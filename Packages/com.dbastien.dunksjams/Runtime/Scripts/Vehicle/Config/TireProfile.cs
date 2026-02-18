using UnityEngine;

[CreateAssetMenu(menuName = "‽/Vehicle/Tire Profile", fileName = "TireProfile")]
public class TireProfile : ScriptableObject
{
    [Header("Dimensions")] [SerializeField]
    private float radius = 0.35f;

    [Header("Friction")] [SerializeField] private AnimationCurve longitudinalFrictionCurve = DefaultLongitudinalCurve();
    [SerializeField] private AnimationCurve lateralFrictionCurve = DefaultLateralCurve();
    [SerializeField] private float maxGrip = 1.2f;

    [Header("Resistance")] [SerializeField]
    private float rollingResistance = 0.015f;

    public float Radius => radius;
    public AnimationCurve LongitudinalFrictionCurve => longitudinalFrictionCurve;
    public AnimationCurve LateralFrictionCurve => lateralFrictionCurve;
    public float MaxGrip => maxGrip;
    public float RollingResistance => rollingResistance;

    /// <summary>Peaks around 0.08 slip, tapers off at higher slip.</summary>
    private static AnimationCurve DefaultLongitudinalCurve()
    {
        var curve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.06f, 0.9f),
            new Keyframe(0.10f, 1f),
            new Keyframe(0.40f, 0.75f),
            new Keyframe(1f, 0.65f)
        );
        return curve;
    }

    /// <summary>Peaks around 8 degrees slip angle, tapers at higher angles.</summary>
    private static AnimationCurve DefaultLateralCurve()
    {
        var curve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(4f, 0.8f),
            new Keyframe(8f, 1f),
            new Keyframe(20f, 0.85f),
            new Keyframe(90f, 0.7f)
        );
        return curve;
    }

    public float EvaluateLongitudinal(float slip) =>
        longitudinalFrictionCurve.Evaluate(Mathf.Abs(slip)) * maxGrip;

    public float EvaluateLateral(float slipAngleDeg) =>
        lateralFrictionCurve.Evaluate(Mathf.Abs(slipAngleDeg)) * maxGrip;
}