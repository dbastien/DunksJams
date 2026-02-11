using System;
using System.Linq;

public static class FloatExtensions
{
    public static float Abs(this float f) => MathF.Abs(f);
    public static bool IsNaN(this float f) => float.IsNaN(f);
    public static float Remap01(this float x, float min, float max) => (x - min) / (max - min);
    public static float Remap(this float x, float minIn, float maxIn, float minOut, float maxOut) => minOut + (maxOut - minOut) * x.Remap01(minIn, maxIn);

    public static void NormalizeHalfMatrix(this float[] m)
    {	
        float s = m[0] + 2 * m.Skip(1).Sum();
        for (var i = 0; i < m.Length; ++i) m[i] /= s;
    }

    public static void NormalizeMatrix(this float[] m)
    {
        float s = m.Sum();
        for (var i = 0; i < m.Length; ++i) m[i] /= s;
    }

    public static float[] ExpandHalfMatrixToFull(this float[] m)
    {
        int lO = m.Length * 2 - 1, cO = lO / 2;
        var res = new float[lO];
        for (var i = 1; i < m.Length; ++i) res[cO - i] = res[cO + i] = m[i];
        res[cO] = m[0];
        return res;
    }
    
    public static bool Approximately(this float f, float other) => MathF.Abs(f - other) < 0.0001f;

}