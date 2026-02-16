#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class CreateNewPaletteWindow : EditorWindow
{
    public static void ShowWindow() => GetWindow<CreateNewPaletteWindow>("Create Palette");

    string _paletteName = "NewPalette";
    int _creationMode = 0; // 0=blank, 1=from base color, 2=from image
    Color _baseColor = Color.white;
    int _schemeIndex = 0; // 0=complementary, 1=triadic, 2=analogous, 3=spectrum
    int _colorCount = 5;
    Texture2D _sourceImage;
    int _extractColorCount = 8;
    string _savePath = "Assets/Palettes";

    void OnGUI()
    {
        GUILayout.Label("Create New Palette", EditorStyles.boldLabel);

        _paletteName = EditorGUILayout.TextField("Palette Name", _paletteName);

        GUILayout.Space(10);

        var modes = new[] { "Blank", "From Base Color", "From Image" };
        _creationMode = GUILayout.Toolbar(_creationMode, modes);

        GUILayout.Space(10);

        switch (_creationMode)
        {
            case 0: // Blank
                EditorGUILayout.HelpBox("Create a blank palette with manual colors.", MessageType.Info);
                _colorCount = EditorGUILayout.IntSlider("Color Count", _colorCount, 1, 16);
                break;

            case 1: // From base color
                EditorGUILayout.HelpBox("Generate a palette using color theory from a base color.", MessageType.Info);
                _baseColor = EditorGUILayout.ColorField("Base Color", _baseColor);
                var schemeModes = new[] { "Complementary", "Triadic", "Analogous", "Spectrum" };
                _schemeIndex = GUILayout.Toolbar(_schemeIndex, schemeModes);
                _colorCount = EditorGUILayout.IntSlider("Color Count", _colorCount, 1, 16);
                break;

            case 2: // From image
                EditorGUILayout.HelpBox("Extract dominant colors from an image using k-means clustering.", MessageType.Info);
                _sourceImage = (Texture2D)EditorGUILayout.ObjectField("Source Image", _sourceImage, typeof(Texture2D), false);
                _extractColorCount = EditorGUILayout.IntSlider("Extract Count", _extractColorCount, 2, 16);
                break;
        }

        GUILayout.Space(10);
        _savePath = EditorGUILayout.TextField("Save Path", _savePath);

        GUILayout.Space(20);

        if (GUILayout.Button("Create Palette", GUILayout.Height(40)))
        {
            CreatePalette();
        }
    }

    void CreatePalette()
    {
        if (string.IsNullOrEmpty(_paletteName))
        {
            EditorUtility.DisplayDialog("Error", "Palette name is required.", "OK");
            return;
        }

        Color[] colors = _creationMode switch
        {
            0 => CreateBlankPalette(),
            1 => CreateFromBaseColor(),
            2 => CreateFromImage(),
            _ => new Color[0]
        };

        if (colors.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "Failed to create palette colors.", "OK");
            return;
        }

        SavePalette(colors);
    }

    Color[] CreateBlankPalette()
    {
        var colors = new Color[_colorCount];
        for (int i = 0; i < _colorCount; i++)
            colors[i] = new Color(Random.value, Random.value, Random.value);
        return colors;
    }

    Color[] CreateFromBaseColor()
    {
        return _schemeIndex switch
        {
            0 => GenerateComplementaryPalette(_baseColor),
            1 => ColorTheory.Triadic(_baseColor),
            2 => ColorTheory.Analogous(_baseColor, _colorCount, 30f),
            3 => ColorTheory.EvenHuePalette(_colorCount),
            _ => new Color[0]
        };
    }

    Color[] GenerateComplementaryPalette(Color c)
    {
        var colors = new Color[_colorCount];
        colors[0] = c;
        for (int i = 1; i < _colorCount; i++)
        {
            var t = (float)i / (_colorCount - 1);
            colors[i] = Color.Lerp(c, ColorTheory.Complementary(c), t);
        }
        return colors;
    }

    Color[] CreateFromImage()
    {
        if (_sourceImage == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a source image.", "OK");
            return new Color[0];
        }

        // Extract dominant colors via k-means
        return PaletteExtraction.ExtractColors(_sourceImage, _extractColorCount);
    }

    void SavePalette(Color[] colors)
    {
        if (!AssetDatabase.IsValidFolder(_savePath))
            AssetDatabase.CreateFolder("Assets", "Palettes");

        var palette = ScriptableObject.CreateInstance<ColorPalette>();
        palette.paletteName = _paletteName;
        palette.colors = colors;
        ApplyGenerationMetadata(palette, colors);

        var assetPath = $"{_savePath}/{_paletteName}.asset";
        AssetDatabase.CreateAsset(palette, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        PaletteDatabase.Refresh();

        EditorUtility.DisplayDialog("Success", $"Palette created: {assetPath}", "OK");
        Close();
    }

    void ApplyGenerationMetadata(ColorPalette palette, Color[] colors)
    {
        if (palette == null) return;

        palette.BaseColor = colors != null && colors.Length > 0 ? colors[0] : Color.white;
        palette.Shades = 1;
        palette.Saturation = 1f;
        palette.Value = 1f;
        palette.MinBrightness = 0f;
        palette.MaxBrightness = 1f;
        palette.AnalogousStepDegrees = 30f;
        palette.SplitComplementaryDegrees = 30f;

        switch (_creationMode)
        {
            case 0: // Blank
                palette.GenerationMode = PaletteGenerationMode.Manual;
                palette.Scheme = PaletteScheme.Custom;
                palette.HueCount = Mathf.Max(1, colors?.Length ?? 1);
                break;
            case 1: // From base color
                palette.GenerationMode = PaletteGenerationMode.Generated;
                palette.BaseColor = _baseColor;
                palette.HueCount = Mathf.Max(1, _colorCount);
                palette.Scheme = _schemeIndex switch
                {
                    0 => PaletteScheme.Complementary,
                    1 => PaletteScheme.Triadic,
                    2 => PaletteScheme.Analogous,
                    3 => PaletteScheme.Spectrum,
                    _ => PaletteScheme.Custom
                };
                Color.RGBToHSV(_baseColor, out _, out var s, out var v);
                palette.Saturation = s;
                palette.Value = v;
                break;
            case 2: // From image
                palette.GenerationMode = PaletteGenerationMode.Manual;
                palette.Scheme = PaletteScheme.Custom;
                palette.HueCount = Mathf.Max(1, colors?.Length ?? 1);
                break;
        }
    }
}
#endif
