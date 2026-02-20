using System;
using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEngine;

public sealed class GaussianBlurKernelGeneratorWindow : EditorWindow
{
    private enum KernelKind
    {
        Full1D,
        LinearOptimized1D,
        Both
    }

    private enum Symmetry
    {
        HalfPlusCenter,
        Full
    }

    private enum OutputFlavor
    {
        RawList,
        UnityHLSL,
        GLSL
    }

    [SerializeField] private float _sigma = 1f;
    [SerializeField] private int _radius = 3;

    [SerializeField] private KernelKind _kernelKind = KernelKind.Both;
    [SerializeField] private Symmetry _symmetry = Symmetry.HalfPlusCenter;
    [SerializeField] private OutputFlavor _outputFlavor = OutputFlavor.UnityHLSL;

    [SerializeField] private bool _normalize = true;
    [SerializeField] private bool _includeCenterSeparateForFull = false;
    [SerializeField] private bool _includeHeader = true;
    [SerializeField] private int _floatPrecision = 7;

    private Vector2 _scroll;
    private string _result = string.Empty;

    [MenuItem("‽/Gaussian Blur Kernel Generator")]
    public static void Init() => GetWindow<GaussianBlurKernelGeneratorWindow>("Gaussian Kernel");

    private void OnGUI()
    {
        using var _ = new EditorGUILayout.VerticalScope();

        EditorGUILayout.LabelField("Gaussian Blur Kernel Generator", EditorStyles.boldLabel);

        _sigma = EditorGUILayout.FloatField(
            new GUIContent("Sigma (σ)", "Standard deviation of the Gaussian. Higher = blurrier."),
            _sigma);

        _radius = EditorGUILayout.IntSlider(
            new GUIContent("Radius (R)", "Kernel covers samples from -R to +R (full)."),
            _radius, 0, 64);

        _kernelKind = (KernelKind)EditorGUILayout.EnumPopup(
            new GUIContent("Kernel Type", "Full taps, linear-optimized taps, or both."),
            _kernelKind);

        _symmetry = (Symmetry)EditorGUILayout.EnumPopup(
            new GUIContent("Generation Domain", "Half+center generates [0..R]. Full generates [-R..R]."),
            _symmetry);

        _outputFlavor = (OutputFlavor)EditorGUILayout.EnumPopup(
            new GUIContent("Output Format", "How arrays are printed."),
            _outputFlavor);

        _normalize = EditorGUILayout.Toggle(
            new GUIContent("Normalize Weights", "Scale so total weight sums to 1."),
            _normalize);

        using (new EditorGUI.DisabledScope(_kernelKind == KernelKind.LinearOptimized1D))
        {
            _includeCenterSeparateForFull = EditorGUILayout.Toggle(
                new GUIContent("Full: Center Separate", "Emit center tap separately (sometimes convenient in shaders)."),
                _includeCenterSeparateForFull);
        }

        _includeHeader = EditorGUILayout.Toggle(
            new GUIContent("Include Header", "Adds a short comment/header with parameters and tap counts."),
            _includeHeader);

        _floatPrecision = EditorGUILayout.IntSlider(
            new GUIContent("Float Precision", "Digits after decimal in output."),
            _floatPrecision, 1, 10);

        EditorGUILayout.Space(6);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Generate", GUILayout.Height(28)))
                Generate();

            if (GUILayout.Button("Copy to Clipboard", GUILayout.Height(28))) EditorGUIUtility.systemCopyBuffer = _result ?? string.Empty;

            if (GUILayout.Button("Clear", GUILayout.Height(28))) _result = string.Empty;
        }

        EditorGUILayout.Space(6);

        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        EditorGUILayout.TextArea(_result, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    private void Generate()
    {
        var sb = new StringBuilder(2048);

        if (!ValidateInputs(sb))
        {
            _result = sb.ToString();
            return;
        }

        var half = GaussianHalfPlusCenter(_sigma, _radius); // [0..R]
        var full = half.ExpandHalfPlusCenterToFull();       // [-R..R]

        if (_normalize)
        {
            half.NormalizeHalfPlusCenterInPlace();
            full.NormalizeFullInPlace();
        }

        if (_includeHeader)
        {
            int fullTapCount = 2 * _radius + 1;
            int optTapCount = LinearOptimizedTapCount(half.Length);

            sb.AppendLine(HeaderLine($"σ={_sigma.ToString(CultureInfo.InvariantCulture)}  R={_radius}"));
            sb.AppendLine(HeaderLine($"Full taps = {fullTapCount}  |  Linear taps = {optTapCount} (uses bilinear pairs)"));
            sb.AppendLine();
        }

        bool wantFull = _kernelKind is KernelKind.Full1D or KernelKind.Both;
        bool wantOpt = _kernelKind is KernelKind.LinearOptimized1D or KernelKind.Both;

        if (wantFull)
        {
            EmitFull(sb, half, full);
            if (wantOpt) sb.AppendLine();
        }

        if (wantOpt)
            EmitLinearOptimized(sb, half);

        _result = sb.ToString();
    }

    private bool ValidateInputs(StringBuilder sb)
    {
        if (!(_sigma > 0f) || float.IsNaN(_sigma) || float.IsInfinity(_sigma))
        {
            sb.AppendLine("Sigma must be a finite number > 0.");
            return false;
        }

        if (_radius < 0)
        {
            sb.AppendLine("Radius must be >= 0.");
            return false;
        }

        return true;
    }

    private void EmitFull(StringBuilder sb, float[] half, float[] full)
    {
        sb.AppendLine(HeaderLine("FULL 1D KERNEL"));

        if (_symmetry == Symmetry.HalfPlusCenter)
        {
            sb.AppendLine(HeaderLine("Half+Center domain [0..R]"));
            sb.AppendLine(FormatArrayBlock("halfWeights", half));
            sb.AppendLine(HeaderLine($"sum(half interpreted symmetrically) = {SymmetricSumHalfPlusCenter(half).ToString("G9", CultureInfo.InvariantCulture)}"));
        }
        else
        {
            sb.AppendLine(HeaderLine("Full domain [-R..R]"));
            sb.AppendLine(FormatArrayBlock("weights", full));
            sb.AppendLine(HeaderLine($"sum = {Sum(full).ToString("G9", CultureInfo.InvariantCulture)}"));
        }

        if (_symmetry == Symmetry.Full)
        {
            if (_includeCenterSeparateForFull)
            {
                float center = full[_radius];
                var sideWeights = new float[_radius];
                for (int i = 0; i < _radius; i++)
                    sideWeights[i] = full[_radius + 1 + i];

                sb.AppendLine();
                sb.AppendLine(FormatScalar("centerWeight", center));
                sb.AppendLine(FormatArrayBlock("sideWeightsPlus", sideWeights));
                sb.AppendLine(HeaderLine("Offsets are 1..R for sideWeightsPlus (positive side); sample both sides in shader."));
            }
            else
            {
                sb.AppendLine();
                sb.AppendLine(HeaderLine("Offsets are integer -R..+R matching weights array indices."));
                sb.AppendLine(FormatArrayBlock("offsets", MakeIntegerOffsets(-_radius, _radius)));
            }
        }
        else
        {
            sb.AppendLine();
            sb.AppendLine(HeaderLine("Offsets are integer 0..R for halfWeights; sample symmetrically in shader."));
            sb.AppendLine(FormatArrayBlock("offsets", MakeIntegerOffsets(0, _radius)));
        }
    }

    private void EmitLinearOptimized(StringBuilder sb, float[] half)
    {
        sb.AppendLine(HeaderLine("LINEAR-OPTIMIZED 1D KERNEL (BILINEAR PAIRS)"));
        sb.AppendLine(HeaderLine("Assumes sampling hardware performs bilinear filtering."));

        GaussianHalfPlusCenterToLinearOptimized(half, out var weights, out var offsets);

        if (_normalize)
            weights.NormalizeHalfPlusCenterOptimizedInPlace();

        sb.AppendLine(FormatArrayBlock("weights", weights));
        sb.AppendLine(FormatArrayBlock("offsets", offsets));
        sb.AppendLine(HeaderLine($"sum(weights interpreted symmetrically) = {SymmetricSumOptimized(weights).ToString("G9", CultureInfo.InvariantCulture)}"));
        sb.AppendLine(HeaderLine("Use offsets as fractional texel offsets from center in either direction; mirror samples for both sides."));
    }

    private static float[] GaussianHalfPlusCenter(float sigma, int radius)
    {
        var res = new float[radius + 1];
        float invTwoSigma2 = 1f / (2f * sigma * sigma);

        for (int x = 0; x <= radius; x++)
            res[x] = MathF.Exp(-(x * x) * invTwoSigma2);

        return res;
    }

    private static void GaussianHalfPlusCenterToLinearOptimized(float[] halfIn, out float[] weights, out float[] offsets)
    {
        int outLen = (halfIn.Length + 1) / 2;
        weights = new float[outLen];
        offsets = new float[outLen];

        weights[0] = halfIn[0];
        offsets[0] = 0f;

        for (int i = 1, o = 1; o < outLen; o++, i += 2)
        {
            float w0 = halfIn[i];
            float w1 = (i + 1 < halfIn.Length) ? halfIn[i + 1] : 0f;

            float w = w0 + w1;
            weights[o] = w;

            offsets[o] = (w > 0f)
                ? ((i * w0 + (i + 1) * w1) / w)
                : i;
        }
    }

    private int LinearOptimizedTapCount(int halfLen) => (halfLen + 1) / 2;

    private string FormatArrayBlock(string name, float[] arr)
    {
        return _outputFlavor switch
        {
            OutputFlavor.RawList => $"{name}: {{ {JoinFloats(arr)} }}",
            OutputFlavor.UnityHLSL => $"static const float {name}[{arr.Length}] = {{ {JoinFloats(arr)} }};",
            OutputFlavor.GLSL => $"const float {name}[{arr.Length}] = float[]({JoinFloats(arr)});",
            _ => $"{name}: {{ {JoinFloats(arr)} }}"
        };
    }

    private string FormatArrayBlock(string name, int[] arr)
    {
        return _outputFlavor switch
        {
            OutputFlavor.RawList => $"{name}: {{ {string.Join(", ", arr)} }}",
            OutputFlavor.UnityHLSL => $"static const int {name}[{arr.Length}] = {{ {string.Join(", ", arr)} }};",
            OutputFlavor.GLSL => $"const int {name}[{arr.Length}] = int[]({string.Join(", ", arr)});",
            _ => $"{name}: {{ {string.Join(", ", arr)} }}"
        };
    }

    private string FormatScalar(string name, float value)
    {
        string v = value.ToString("F" + _floatPrecision, CultureInfo.InvariantCulture);
        return _outputFlavor switch
        {
            OutputFlavor.RawList => $"{name}: {v}",
            OutputFlavor.UnityHLSL => $"static const float {name} = {v};",
            OutputFlavor.GLSL => $"const float {name} = {v};",
            _ => $"{name}: {v}"
        };
    }

    private string JoinFloats(float[] arr)
    {
        var sb = new StringBuilder(arr.Length * (_floatPrecision + 6));
        for (int i = 0; i < arr.Length; i++)
        {
            if (i != 0) sb.Append(", ");
            sb.Append(arr[i].ToString("F" + _floatPrecision, CultureInfo.InvariantCulture));
        }
        return sb.ToString();
    }

    private static string HeaderLine(string s) => $"// {s}";

    private static int[] MakeIntegerOffsets(int min, int max)
    {
        int len = max - min + 1;
        var res = new int[len];
        for (int i = 0; i < len; i++) res[i] = min + i;
        return res;
    }

    private static float Sum(float[] a)
    {
        float s = 0f;
        for (int i = 0; i < a.Length; i++) s += a[i];
        return s;
    }

    private static float SymmetricSumHalfPlusCenter(float[] half)
    {
        float s = half[0];
        for (int i = 1; i < half.Length; i++) s += 2f * half[i];
        return s;
    }

    private static float SymmetricSumOptimized(float[] weights)
    {
        float s = weights[0];
        for (int i = 1; i < weights.Length; i++) s += 2f * weights[i];
        return s;
    }
}

public static class GaussianKernelArrayExtensions
{
    public static float[] ExpandHalfPlusCenterToFull(this float[] half)
    {
        int radius = half.Length - 1;
        int fullLen = 2 * radius + 1;

        var full = new float[fullLen];

        full[radius] = half[0];
        for (int i = 1; i <= radius; i++)
        {
            full[radius + i] = half[i];
            full[radius - i] = half[i];
        }

        return full;
    }

    public static void NormalizeHalfPlusCenterInPlace(this float[] half)
    {
        float sum = half[0];
        for (int i = 1; i < half.Length; i++) sum += 2f * half[i];
        if (sum <= 0f) return;

        float inv = 1f / sum;
        for (int i = 0; i < half.Length; i++) half[i] *= inv;
    }

    public static void NormalizeFullInPlace(this float[] full)
    {
        float sum = 0f;
        for (int i = 0; i < full.Length; i++) sum += full[i];
        if (sum <= 0f) return;

        float inv = 1f / sum;
        for (int i = 0; i < full.Length; i++) full[i] *= inv;
    }

    public static void NormalizeHalfPlusCenterOptimizedInPlace(this float[] weights)
    {
        float sum = weights[0];
        for (int i = 1; i < weights.Length; i++) sum += 2f * weights[i];
        if (sum <= 0f) return;

        float inv = 1f / sum;
        for (int i = 0; i < weights.Length; i++) weights[i] *= inv;
    }
}