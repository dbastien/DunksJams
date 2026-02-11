using UnityEngine;
public class TetherFilter3D : IFilter3D
{
    public float TetherLength = 0.01f;

    public float TetherMaxChangePerFrame = 0.35f;

    public Vector3 CurrentValue => tetherPosition;

    public bool JustMoved { get; set; }
    
    private Vector3 tetherPosition;

    private bool hasValue;

    public object Clone() => MemberwiseClone();

    public void Update(Vector3 s)
    {
        // apply 2D filter
        if (TetherLength > 0.0f && hasValue)
        {
            var tetherDiff = s - tetherPosition;

            float distanceBeyondTether = tetherDiff.magnitude - TetherLength;
            distanceBeyondTether = Mathf.Min(distanceBeyondTether, TetherMaxChangePerFrame);

            // is the current position outside the tether circle?
            if (distanceBeyondTether > 0.0f)
            {
                // motion continues past the edge of the circle, relocate circle
                // so that current position is on the outer edge of it
                tetherDiff.Normalize();
                var tetherDelta = tetherDiff * distanceBeyondTether;
                tetherPosition += tetherDelta;

                JustMoved = true;
            }
            else
            {
                JustMoved = false;
            }
        }
        else
        {
            tetherPosition = s;
        }

        if (!hasValue) hasValue = true;
    }

    public void Reset()
    {
        tetherPosition = new Vector3();
        hasValue = false;
    }
}
