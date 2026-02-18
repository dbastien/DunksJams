using UnityEngine;

public class SteeringAssist
{
    private const float CountersteerThreshold = 5f;
    private const float MaxCountersteer = 15f;

    public float Apply(float steerAngle, VehicleController vehicle)
    {
        if (vehicle.Profile == null) return steerAngle;

        float strength = vehicle.Profile.SteeringAssistStrength;
        if (strength <= 0f) return steerAngle;

        // Compute average rear lateral slip
        var avgRearSlip = 0f;
        var rearCount = 0;
        foreach (VehicleWheel w in vehicle.Wheels)
        {
            if (w.IsFrontAxle || !w.IsGrounded) continue;
            avgRearSlip += w.LateralSlipAngle;
            rearCount++;
        }

        if (rearCount == 0) return steerAngle;
        avgRearSlip /= rearCount;

        if (Mathf.Abs(avgRearSlip) < CountersteerThreshold) return steerAngle;

        // Countersteer proportional to oversteer slip
        float counterAmount = Mathf.InverseLerp(CountersteerThreshold, 45f, Mathf.Abs(avgRearSlip));
        float counterAngle = -Mathf.Sign(avgRearSlip) * counterAmount * MaxCountersteer * strength;

        return steerAngle + counterAngle;
    }
}