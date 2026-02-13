using UnityEngine;

public class TetherFilter2D1D : IFilter3D
{
    public float TetherLength2D = 0.01f;
    public float TetherMaxChangePerFrame2D = 0.35f;
    public float TetherLength1D = 0.01f;
    public float TetherMaxChangePerFrame1D = 0.35f;
    public int Tether1DAxis = 2;

    public Vector3 CurrentValue => tetherPosition;

    float tetherPosition1D;
    Vector2 tetherPosition2D;
    Vector3 tetherPosition;

    bool hasValue;

    public object Clone() => MemberwiseClone();

    public void Update(Vector3 s)
    {
        if (!hasValue)
        {
            tetherPosition = s;
            tetherPosition2D = s.GetValuesFromExclusionIndex(Tether1DAxis);
            tetherPosition1D = s.GetValueFromIndex(Tether1DAxis);
            hasValue = true;
            return;
        }

        // apply 2D filter
        var rawPosition2D = s.GetValuesFromExclusionIndex(Tether1DAxis);
        if (TetherLength2D > 0.0f)
        {
            var tetherDiff2D = rawPosition2D - tetherPosition2D;

            var distanceBeyondTether = tetherDiff2D.magnitude - TetherLength2D;
            distanceBeyondTether = Mathf.Min(distanceBeyondTether, TetherMaxChangePerFrame2D);

            // is the current position outside the tether circle?
            if (distanceBeyondTether > 0.0f)
            {
                tetherDiff2D.Normalize();
                var tetherDelta = tetherDiff2D * distanceBeyondTether;
                tetherPosition2D += tetherDelta;
            }
        }
        else
        {
            tetherPosition2D = rawPosition2D;
        }

        // apply 1D filter
        var rawPosition1D = s.GetValueFromIndex(Tether1DAxis);
        if (TetherLength1D > 0.0)
        {
            var tetherDiff = rawPosition1D - tetherPosition1D;

            if (Mathf.Abs(tetherDiff) > TetherLength1D)
            {
                var distanceBeyondTether = (Mathf.Abs(tetherDiff) - TetherLength1D) * Mathf.Sign(tetherDiff);
                distanceBeyondTether = Mathf.Min(distanceBeyondTether, TetherMaxChangePerFrame1D);

                tetherPosition1D += distanceBeyondTether;
            }
        }
        else
        {
            tetherPosition1D = rawPosition1D;
        }

        tetherPosition = tetherPosition2D.MergeValues(Tether1DAxis, tetherPosition1D);
    }

    public void Reset()
    {
        tetherPosition = Vector3.zero;
        hasValue = false;
    }
}