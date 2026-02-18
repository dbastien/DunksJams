using System.Text;
using UnityEngine;

/// <summary>
/// Curve utility methods: emit C# code for an AnimationCurve, pack keyframes
/// to Vector4[] for GPU StructuredBuffer usage.
/// Extracted from LandscapeBuilder's LBCurve.
/// </summary>
public static class CurveUtils
{
    /// <summary>
    /// Emit C# code that recreates the given AnimationCurve (including tangents).
    /// Useful for baking curves into runtime code or logging curve data.
    /// </summary>
    public static string ScriptCurve(AnimationCurve curve, string eol = "\n", string curveName = "newCurve")
    {
        if (curve == null) return string.Empty;
        if (string.IsNullOrEmpty(curveName)) curveName = "newCurve";
        if (string.IsNullOrEmpty(eol)) eol = " ";

        var sb = new StringBuilder(512);
        sb.Append("AnimationCurve ").Append(curveName).Append(" = new AnimationCurve();").Append(eol);

        Keyframe[] keys = curve.keys;
        if (keys is not { Length: > 0 }) return sb.ToString();

        foreach (Keyframe key in keys)
            sb.Append(curveName).
                Append(".AddKey(").
                Append(key.time.ToString("0.00")).
                Append("f,").
                Append(key.value.ToString("0.00")).
                Append("f);").
                Append(eol);

        sb.Append("Keyframe[] ").Append(curveName).Append("Keys = ").Append(curveName).Append(".keys;").Append(eol);

        var idx = 0;
        foreach (Keyframe key in keys)
        {
            sb.Append(curveName).
                Append("Keys[").
                Append(idx).
                Append("].inTangent = ").
                Append(key.inTangent.ToString("0.00")).
                Append("f;").
                Append(eol);
            sb.Append(curveName).
                Append("Keys[").
                Append(idx).
                Append("].outTangent = ").
                Append(key.outTangent.ToString("0.00")).
                Append("f;").
                Append(eol);
            idx++;
        }

        sb.Append(curveName).
            Append(" = new AnimationCurve(").
            Append(curveName).
            Append("Keys);").
            Append(eol).
            Append(eol);
        return sb.ToString();
    }

    /// <summary>
    /// Pack AnimationCurve keyframes into Vector4[] for use with GPU StructuredBuffers.
    /// Format: x = time, y = value, z = inTangent, w = outTangent.
    /// </summary>
    public static Vector4[] GetComputeKeyFrames(AnimationCurve curve)
    {
        if (curve == null) return null;

        Keyframe[] keyframes = curve.keys;
        int count = keyframes?.Length ?? 0;
        if (count == 0) return null;

        var packed = new Vector4[count];
        for (var i = 0; i < count; i++)
            packed[i] = new Vector4(
                keyframes[i].time,
                keyframes[i].value,
                keyframes[i].inTangent,
                keyframes[i].outTangent
            );

        return packed;
    }
}