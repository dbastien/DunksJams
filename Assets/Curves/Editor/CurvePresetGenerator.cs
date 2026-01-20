using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class CurvePresetGenerator
{
    public static readonly int StepCount = 50;
    public static readonly float StepSize = StepCount > 1 ? 1f / (StepCount - 1) : 1f;
    
    [MenuItem("‽/Preset Libraries/Generate curves")] 
    public static void GenerateCurvePresets()
    {
        ScriptableObject lib = CurvePresetLibraryWrapper.CreateLibrary();
        
        foreach (EaseType easeType in EnumCache<EaseType>.Values) 
            CurvePresetLibraryWrapper.Add(lib, CreateCurve(Ease.GetEasingFunction(easeType)), easeType.ToString());

        AssetDatabase.CreateAsset(lib, "Assets" + CurveConstants.NormalizedCurvesPath);

        ScriptableObject libUnnormalized = Object.Instantiate(lib);
        AssetDatabase.CreateAsset(libUnnormalized, "Assets" + CurveConstants.CurvesPath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    
    public static AnimationCurve CreateCurve(Func<float, float> f) => CreateCurve(f, StepCount);

    public static AnimationCurve CreateCurve(Func<float, float> f, int steps)
    {
        var curve = new AnimationCurve
        {
            preWrapMode = WrapMode.PingPong,
            postWrapMode = WrapMode.PingPong
        };

        float t = 0f;
        for (var i = 0; i < steps; ++i)
        {
            float clamped = Mathf.Clamp01(t);
            float val = Mathf.Clamp01(f(clamped));
            var keyframe = new Keyframe(clamped, val);
            //AnimationUtility.SetKeyLeftTangentMode
            curve.AddKey(keyframe);
            t += StepSize;
        }

        AnimationCurveUtils.SetTangentMode(curve, AnimationUtility.TangentMode.Auto);
        AnimationCurveUtils.SetKeysBroken(curve, false);
        AnimationCurveUtils.SmoothAllTangents(curve);

        return curve;
    }
}