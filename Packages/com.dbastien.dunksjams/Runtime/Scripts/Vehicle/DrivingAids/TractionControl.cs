using UnityEngine;

public class TractionControl
{
    const float MaxAllowedSlip = 0.15f;

    public float Apply(float wheelTorque, VehicleWheel[] driveWheels, float strength)
    {
        if (strength <= 0f || driveWheels == null) return wheelTorque;

        float maxSlip = 0f;
        foreach (var w in driveWheels)
        {
            if (!w.IsGrounded) continue;
            float slip = Mathf.Abs(w.LongitudinalSlip);
            if (slip > maxSlip) maxSlip = slip;
        }

        if (maxSlip <= MaxAllowedSlip) return wheelTorque;

        float reduction = Mathf.InverseLerp(MaxAllowedSlip, 1f, maxSlip) * strength;
        return wheelTorque * (1f - reduction);
    }
}
