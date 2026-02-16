using UnityEngine;

public enum PaletteGenerationMode
{
    Manual,
    Generated
}

public enum PaletteScheme
{
    Custom,
    Monochromatic,
    Complementary,
    SplitComplementary,
    Analogous,
    Triadic,
    Tetradic,
    Square,
    Spectrum
}

[CreateAssetMenu(fileName = "ColorPalette", menuName = "DunksJams/Color Palette")]
public class ColorPalette : ScriptableObject
{
    public string paletteName;
    [Tooltip("Tags for organization: e.g., 'NES', 'GameBoy', 'Retro', 'UI'.")]
    public string[] tags = new string[0];
    public Color[] colors = new Color[0];

    [Header("Generation")]
    [SerializeField] PaletteGenerationMode _generationMode = PaletteGenerationMode.Manual;
    [SerializeField] PaletteScheme _scheme = PaletteScheme.Custom;
    [SerializeField] Color _baseColor = Color.white;
    [SerializeField, Min(1)] int _hueCount = 5;
    [SerializeField, Min(1)] int _shades = 3;
    [SerializeField, Range(0f, 1f)] float _saturation = 1f;
    [SerializeField, Range(0f, 1f)] float _value = 0.9f;
    [SerializeField, Range(0f, 1f)] float _minBrightness = 0f;
    [SerializeField, Range(0f, 1f)] float _maxBrightness = 1f;
    [SerializeField, Range(1f, 180f)] float _analogousStepDegrees = 30f;
    [SerializeField, Range(1f, 180f)] float _splitComplementaryDegrees = 30f;
    [SerializeField] bool _spectrumIncludeBlackWhite = false;

    public int Count => colors?.Length ?? 0;

    public PaletteGenerationMode GenerationMode
    {
        get => _generationMode;
        set => _generationMode = value;
    }

    public PaletteScheme Scheme
    {
        get => _scheme;
        set => _scheme = value;
    }

    public Color BaseColor
    {
        get => _baseColor;
        set => _baseColor = value;
    }

    public int HueCount
    {
        get => _hueCount;
        set => _hueCount = Mathf.Max(1, value);
    }

    public int Shades
    {
        get => _shades;
        set => _shades = Mathf.Max(1, value);
    }

    public float Saturation
    {
        get => _saturation;
        set => _saturation = Mathf.Clamp01(value);
    }

    public float Value
    {
        get => _value;
        set => _value = Mathf.Clamp01(value);
    }

    public float MinBrightness
    {
        get => _minBrightness;
        set => _minBrightness = Mathf.Clamp01(value);
    }

    public float MaxBrightness
    {
        get => _maxBrightness;
        set => _maxBrightness = Mathf.Clamp01(value);
    }

    public float AnalogousStepDegrees
    {
        get => _analogousStepDegrees;
        set => _analogousStepDegrees = Mathf.Clamp(value, 1f, 180f);
    }

    public float SplitComplementaryDegrees
    {
        get => _splitComplementaryDegrees;
        set => _splitComplementaryDegrees = Mathf.Clamp(value, 1f, 180f);
    }

    public bool SpectrumIncludeBlackWhite
    {
        get => _spectrumIncludeBlackWhite;
        set => _spectrumIncludeBlackWhite = value;
    }


    public bool HasTag(string tag) => tags != null && System.Array.Exists(tags, t => t == tag);

    public void AddTag(string tag)
    {
        if (!HasTag(tag))
        {
            var list = new System.Collections.Generic.List<string>(tags ?? new string[0]);
            list.Add(tag);
            tags = list.ToArray();
        }
    }

    public Color GetColor(int index, Color fallback)
    {
        if (colors == null || colors.Length == 0) return fallback;
        return colors[Mathf.Clamp(index, 0, colors.Length - 1)];
    }

    public Color[] ToArray() => colors ?? new Color[0];
}
