using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class TransformCurve : MonoBehaviour
{
    public static readonly Dictionary<string, Vector3> DefaultEndOffsets = new()
    {
        { "eulerAngles", new(0.0f, 360.0f, 0.0f) },
        { "localEulerAngles", new(0.0f, 360.0f, 0.0f) },
    };

    [NormalizedAnimationCurve] public AnimationCurve curve;
    public Vector3 curveStart;
    public Vector3 curveEnd;
    public bool relativeMode;
    public Vector3 curveOffset;
    [FloatIncremental(.1f)] public float lengthScale = 1.0f;
    public PropertyInfo CurveTarget;
    public string curveTargetName;

    float _timeElapsed;

    public void Reset()
    {
        curve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
        curve.preWrapMode = WrapMode.PingPong;
        curve.postWrapMode = WrapMode.PingPong;

        curveTargetName = "position";

        ResetStartEnd();
    }

    public void UpdateTarget() => CurveTarget = typeof(Transform).GetProperty(curveTargetName);

    public void ResetStartEnd()
    {
        UpdateTarget();

        var currentPos = (Vector3)typeof(Transform).GetProperty(curveTargetName)?.GetValue(transform, null);       

        if (!relativeMode)
        {
            curveStart = currentPos;        
            if (DefaultEndOffsets.TryGetValue(curveTargetName, out curveEnd))
                curveEnd += curveStart;
            else
                curveEnd = curveStart + Vector3.up;
        }
        else
        {
            curveStart = Vector3.zero;
            curveEnd = Vector3.zero;
        }
    }

    public void Awake()
    {
        UpdateTarget();

        if (relativeMode)
        {
            var currentPos = (Vector3)typeof(Transform).GetProperty(curveTargetName)?.GetValue(transform, null);
            curveStart = currentPos;
            curveEnd = currentPos + curveOffset;
        }
    }

    public void Update()
    {
        _timeElapsed += Time.deltaTime / lengthScale;

        if (CurveTarget == null) return;
        Vector3 interpolatedValue = curveStart.LerpUnclamped(curveEnd, curve.Evaluate(_timeElapsed));
        CurveTarget.SetValue(transform, interpolatedValue, null);
    }
}
