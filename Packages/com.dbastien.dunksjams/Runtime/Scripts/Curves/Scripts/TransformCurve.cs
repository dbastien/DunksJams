using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

public class TransformCurve : MonoBehaviour
{
    public static readonly Dictionary<string, Vector3> DefaultEndOffsets = new()
    {
        { "eulerAngles", new Vector3(0.0f, 360.0f, 0.0f) },
        { "localEulerAngles", new Vector3(0.0f, 360.0f, 0.0f) }
    };

    [NormalizedAnimationCurve] public AnimationCurve curve;
    public Vector3 curveStart;
    public Vector3 curveEnd;
    public bool relativeMode;
    public Vector3 curveOffset;
    [FloatIncremental(.1f)] public float lengthScale = 1.0f;
    public PropertyInfo CurveTarget;
    public string curveTargetName;

    private float _timeElapsed;

    // Cached delegates for performance (avoids reflection every frame)
    private Action<Transform, Vector3> _cachedSetter;
    private Func<Transform, Vector3> _cachedGetter;

    public void Reset()
    {
        curve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
        curve.preWrapMode = WrapMode.PingPong;
        curve.postWrapMode = WrapMode.PingPong;

        curveTargetName = "position";

        ResetStartEnd();
    }

    public void UpdateTarget()
    {
        CurveTarget = typeof(Transform).GetProperty(curveTargetName);
        if (CurveTarget == null) return;

        // Create compiled delegates using expression trees (~50x faster than reflection)
        ParameterExpression transformParam = Expression.Parameter(typeof(Transform), "t");
        MemberExpression propAccess = Expression.Property(transformParam, CurveTarget);

        // Getter: (t) => t.propertyName
        _cachedGetter = Expression.Lambda<Func<Transform, Vector3>>(propAccess, transformParam).Compile();

        // Setter: (t, val) => t.propertyName = val
        ParameterExpression valueParam = Expression.Parameter(typeof(Vector3), "value");
        BinaryExpression assignExpr = Expression.Assign(propAccess, valueParam);
        _cachedSetter = Expression.Lambda<Action<Transform, Vector3>>(assignExpr, transformParam, valueParam).Compile();
    }

    public void ResetStartEnd()
    {
        UpdateTarget();
        if (_cachedGetter == null) return;

        Vector3 currentPos = _cachedGetter(transform);

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

        if (relativeMode && _cachedGetter != null)
        {
            Vector3 currentPos = _cachedGetter(transform);
            curveStart = currentPos;
            curveEnd = currentPos + curveOffset;
        }
    }

    public void Update()
    {
        _timeElapsed += Time.deltaTime / lengthScale;

        if (_cachedSetter == null) return;
        Vector3 interpolatedValue = curveStart.LerpUnclamped(curveEnd, curve.Evaluate(_timeElapsed));
        _cachedSetter(transform, interpolatedValue);
    }
}