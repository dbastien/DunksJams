using UnityEngine;

public class BrakeAssist
{
    private const float LockupThreshold = 0.25f;

    public float Apply(float brakeTorque, VehicleWheel[] allWheels, float strength)
    {
        if (strength <= 0f || brakeTorque <= 0f || allWheels == null) return brakeTorque;

        var maxBrakeSlip = 0f;
        foreach (VehicleWheel w in allWheels)
        {
            if (!w.IsGrounded || !w.IsBrake) continue;
            float slip = Mathf.Abs(w.LongitudinalSlip);
            if (slip > maxBrakeSlip) maxBrakeSlip = slip;
        }

        if (maxBrakeSlip <= LockupThreshold) return brakeTorque;

        // Reduce brake torque proportionally to how far past lockup we are
        float reduction = Mathf.InverseLerp(LockupThreshold, 0.8f, maxBrakeSlip) * strength;
        return brakeTorque * (1f - reduction);
    }
}