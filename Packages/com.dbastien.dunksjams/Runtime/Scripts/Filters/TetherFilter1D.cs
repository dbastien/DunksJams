using UnityEngine;

public class TetherFilter1D : IFilter1D
{
    public float TetherLength = 0.01f;
    public float TetherMaxChangePerFrame = 0.35f;

    public float CurrentValue => tetherPosition;

    float tetherPosition;
    bool hasValue;

    public void Update(float s)
    {
        if (!hasValue)
        {
            tetherPosition = s;
            hasValue = true;
            return;
        }

        // apply 1D filter
        var rawPosition1D = s;
        if (TetherLength > 0.0f)
        {
            var tetherDiff = rawPosition1D - tetherPosition;

            if (!(Mathf.Abs(tetherDiff) > TetherLength)) return;
            var distanceBeyondTether = (Mathf.Abs(tetherDiff) - TetherLength) * Mathf.Sign(tetherDiff);
            distanceBeyondTether = Mathf.Min(distanceBeyondTether, TetherMaxChangePerFrame);

            tetherPosition += distanceBeyondTether;
        }
        else
        {
            tetherPosition = rawPosition1D;
        }
    }

    public void Reset()
    {
        tetherPosition = 0;
        hasValue = false;
    }
}