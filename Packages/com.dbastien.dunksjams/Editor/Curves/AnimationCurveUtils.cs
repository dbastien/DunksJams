using UnityEditor;
using UnityEngine;

public static class AnimationCurveUtils
{
    public static void SmoothAllTangents(AnimationCurve curve)
    {
        for (var k = 0; k < curve.length; ++k) curve.SmoothTangents(k, 0f);
    }
     
    public static void SetTangentMode(AnimationCurve curve, AnimationUtility.TangentMode tangentMode)
    {
        for (var k = 0; k < curve.length; ++k)
        {
            AnimationUtility.SetKeyLeftTangentMode(curve, k, tangentMode);
            AnimationUtility.SetKeyRightTangentMode(curve, k, tangentMode);
        }
    }

    public static void SetKeysBroken(AnimationCurve curve, bool broken)
    {
        for (var k = 0; k < curve.length; ++k) AnimationUtility.SetKeyBroken(curve, k, broken);
    }
}