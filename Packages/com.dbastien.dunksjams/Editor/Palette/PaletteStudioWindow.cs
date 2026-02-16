#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PaletteStudioWindow : EditorWindow
{
    enum Tab
    {
        Picker,
        Library
    }

    [SerializeField] Tab _tab = Tab.Picker;
    [SerializeField] ColorPalette _selectedPalette;
    [SerializeField] Vector2 _paletteListScroll;
    [SerializeField] Vector2 _rightScroll;
    [SerializeField] Vector2 _colorTheoryScroll;
    [SerializeField] bool _showTagFilter;
    [SerializeField] string _tagFilter = "";
    [SerializeField] int _colorTheoryMode;
    [SerializeField] bool _showPaletteOverview = false;
    [SerializeField] bool _showColorBlindPreview;
    [SerializeField] int _colorBlindMode;
    [SerializeField] int _selectedPaletteColorIndex = -1;

    [SerializeField] Color _selectedColor = Color.white;
    [SerializeField] float _h, _s, _v, _a = 1f;
    [SerializeField] PaletteScheme _schemePreview = PaletteScheme.Custom;
    [SerializeField] Texture2D _lutSourceTexture;
    [SerializeField, Range(2, 128)] int _texturePaletteCount = 16;
    [SerializeField] int _palettePickerId = -1;

    System.Action<Color> _onColorChanged;

    Texture2D _hueWheel;
    Texture2D _centerPie;
    Texture2D _lightnessBar;

    const int WheelSize = 200;
    const float SwatchMinSize = 14f;
    const float WheelCenterRadiusRatio = 0.32f;
    const float HarmonyLineWidth = 3f;
    const float PalettePreviewHeight = 34f;
    const int UnityPaletteSlotCount = 16;
    const int CenterPieSize = 128;

    readonly HashSet<string> _discoveredTags = new();

    static readonly string[] TabLabels = { "Picker", "Library" };
    static readonly string[] ColorTheoryTabs = { "Complementary", "Triadic", "Analogous", "Contrast Matrix" };
    static readonly string[] ColorBlindTabs = { "Normal", "Deuteranopia", "Protanopia", "Tritanopia" };

    public static PaletteStudioWindow ShowWindow()
    {
        var window = GetWindow<PaletteStudioWindow>("Palette Studio");
        window.Show();
        return window;
    }

    public static PaletteStudioWindow ShowPicker(System.Action<Color> onColorChanged, Color initialColor)
    {
        var window = GetWindow<PaletteStudioWindow>("Palette Studio");
        window._tab = Tab.Picker;
        window._onColorChanged = onColorChanged;
        window._selectedColor = initialColor;
        window.UpdateHSV();
        window.Show();
        return window;
    }

    void OnEnable()
    {
        PaletteDatabase.Refresh();
        RefreshDiscoveredTags();
        EnsureSelectedPalette();
        UpdateHSV();
        GenerateTextures();
    }

    void OnDisable()
    {
        DestroyTextures();
    }

    void OnGUI()
    {
        HandleObjectPicker();
        DrawToolbar();
        if (_tab == Tab.Library)
        {
            DrawLibraryLayout();
        }
        else
        {
            DrawFullWidthContent();
        }
    }

    void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        var tabIndex = _tab == Tab.Library ? 1 : 0;
        tabIndex = GUILayout.Toolbar(tabIndex, TabLabels, EditorStyles.toolbarButton, GUILayout.Height(22));
        _tab = tabIndex == 0 ? Tab.Picker : Tab.Library;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    void DrawPaletteSidebar()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(220));
        GUILayout.Label("Palettes", EditorStyles.boldLabel);

        if (GUILayout.Button("Create New Palette", GUILayout.Height(22)))
            CreateNewPaletteWindow.ShowWindow();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh", GUILayout.Width(80)))
        {
            PaletteDatabase.Refresh();
            RefreshDiscoveredTags();
            EnsureSelectedPalette();
        }
        if (_selectedPalette != null && GUILayout.Button("Ping", GUILayout.Width(60)))
        {
            Selection.activeObject = _selectedPalette;
            EditorGUIUtility.PingObject(_selectedPalette);
        }
        EditorGUILayout.EndHorizontal();

        _showTagFilter = EditorGUILayout.Foldout(_showTagFilter, "Filter by Tag");
        if (_showTagFilter)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("All", GUILayout.Width(50)))
                _tagFilter = "";
            foreach (var tag in _discoveredTags)
            {
                if (GUILayout.Button(tag, GUILayout.Width(80)))
                    _tagFilter = (_tagFilter == tag) ? "" : tag;
            }
            EditorGUILayout.EndHorizontal();
            if (!string.IsNullOrEmpty(_tagFilter))
                EditorGUILayout.LabelField($"Filtered by: {_tagFilter}", EditorStyles.miniLabel);
        }

        _paletteListScroll = EditorGUILayout.BeginScrollView(_paletteListScroll, GUILayout.ExpandHeight(true));

        var palettes = PaletteDatabase.Palettes;
        var filtered = FilterPalettesByTag(palettes, _tagFilter);

        if (palettes.Count == 0)
        {
            EditorGUILayout.HelpBox("No palettes found. Create one to get started.", MessageType.Info);
        }
        else if (filtered.Count == 0)
        {
            EditorGUILayout.HelpBox($"No palettes with tag '{_tagFilter}'.", MessageType.Info);
        }
        else
        {
            foreach (var pal in filtered)
            {
                var isSelected = _selectedPalette == pal;
                var style = isSelected ? EditorStyles.whiteBoldLabel : EditorStyles.label;
                var name = GetPaletteDisplayName(pal);
                var tagStr = pal.tags != null && pal.tags.Length > 0 ? $" [{string.Join(", ", pal.tags)}]" : "";
                if (GUILayout.Button(name + $" ({pal.Count}){tagStr}", style, GUILayout.Height(22)))
                    SelectPalette(pal);
            }
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }
    void DrawFullWidthContent()
    {
        _rightScroll = EditorGUILayout.BeginScrollView(_rightScroll);
        DrawPickerTab();
        EditorGUILayout.EndScrollView();
    }

    void DrawLibraryLayout()
    {
        EditorGUILayout.BeginHorizontal();
        DrawPaletteSidebar();
        GUILayout.Space(8);
        _rightScroll = EditorGUILayout.BeginScrollView(_rightScroll);
        DrawLibraryTab();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndHorizontal();
    }

    void DrawPickerTab()
    {
        DrawCompactPaletteSelector();

        var colorChanged = false;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical(GUILayout.Width(WheelSize + 20));
        DrawVisualPickers(ref colorChanged);
        GUILayout.Space(6);
        DrawLightnessSlider(ref colorChanged);
        EditorGUILayout.EndVertical();

        GUILayout.Space(12);

        EditorGUILayout.BeginVertical();
        DrawColorControls(ref colorChanged);
        GUILayout.Space(6);
        DrawSchemeControls();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        if (colorChanged)
        {
            NotifyColorChanged();
            Repaint();
        }

        GUILayout.Space(8);
        DrawPickerPaletteSection();
    }

    void DrawColorControls(ref bool colorChanged)
    {
        EditorGUILayout.LabelField("Selected Color", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        _selectedColor = EditorGUILayout.ColorField(GUIContent.none, _selectedColor, true, true, false, GUILayout.Height(40));
        if (EditorGUI.EndChangeCheck())
        {
            UpdateHSV();
            colorChanged = true;
        }

        GUILayout.Space(6);
        var prevLabelWidth = EditorGUIUtility.labelWidth;
        var prevFieldWidth = EditorGUIUtility.fieldWidth;
        EditorGUIUtility.labelWidth = 18f;
        EditorGUIUtility.fieldWidth = 42f;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical(GUILayout.MinWidth(150));
        EditorGUILayout.LabelField("HSVA", EditorStyles.miniBoldLabel);
        EditorGUI.BeginChangeCheck();
        var h = EditorGUILayout.Slider("H", _h * 360f, 0, 360f) / 360f;
        var s = EditorGUILayout.Slider("S", _s * 100f, 0, 100f) / 100f;
        var v = EditorGUILayout.Slider("V", _v * 100f, 0, 100f) / 100f;
        var a = EditorGUILayout.Slider("A", _a * 100f, 0, 100f) / 100f;
        if (EditorGUI.EndChangeCheck())
        {
            _h = h;
            _s = s;
            _v = v;
            _a = a;
            UpdateSelectedColorFromHSV();
            colorChanged = true;
        }
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);

        EditorGUILayout.BeginVertical(GUILayout.MinWidth(150));
        EditorGUILayout.LabelField("RGBA", EditorStyles.miniBoldLabel);
        EditorGUI.BeginChangeCheck();
        var r = EditorGUILayout.Slider("R", _selectedColor.r * 255f, 0, 255f) / 255f;
        var g = EditorGUILayout.Slider("G", _selectedColor.g * 255f, 0, 255f) / 255f;
        var b = EditorGUILayout.Slider("B", _selectedColor.b * 255f, 0, 255f) / 255f;
        if (EditorGUI.EndChangeCheck())
        {
            _selectedColor = new Color(r, g, b, _a);
            UpdateHSV();
            colorChanged = true;
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        EditorGUIUtility.labelWidth = prevLabelWidth;
        EditorGUIUtility.fieldWidth = prevFieldWidth;

        GUILayout.Space(4);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Hex", GUILayout.Width(30));
        var hex = ColorUtility.ToHtmlStringRGBA(_selectedColor);
        var hexInput = EditorGUILayout.TextField(hex);
        if (!string.Equals(hexInput, hex, StringComparison.OrdinalIgnoreCase) &&
            ColorUtility.TryParseHtmlString("#" + hexInput, out var parsed))
        {
            _selectedColor = parsed;
            UpdateHSV();
            colorChanged = true;
        }
        EditorGUILayout.EndHorizontal();
    }

    void DrawLightnessSlider(ref bool colorChanged)
    {
        EditorGUILayout.LabelField("Lightness", EditorStyles.miniLabel);
        var rect = EditorGUILayout.GetControlRect(GUILayout.Height(18));
        
        if (_lightnessBar == null) GenerateTextures();
        GUI.DrawTexture(rect, _lightnessBar);

        // Marker for current V
        var markerX = rect.x + _v * rect.width;
        var markerRect = new Rect(markerX - 2, rect.y - 2, 4, rect.height + 4);
        EditorGUI.DrawRect(markerRect, Color.white);
        DrawRectOutline(markerRect, Color.black, 1);

        var e = Event.current;
        if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && rect.Contains(e.mousePosition))
        {
            _v = Mathf.Clamp01((e.mousePosition.x - rect.x) / rect.width);
            UpdateSelectedColorFromHSV();
            colorChanged = true;
            if (_selectedPalette != null && _selectedPalette.GenerationMode == PaletteGenerationMode.Generated)
                ApplyPickerSvToPalette();
            e.Use();
        }
    }

    void DrawVisualPickers(ref bool colorChanged)
    {
        if (_hueWheel == null)
            GenerateTextures();

        var wheelRect = GUILayoutUtility.GetRect(WheelSize, WheelSize, GUILayout.Width(WheelSize), GUILayout.Height(WheelSize));
        GUI.DrawTexture(wheelRect, _hueWheel);

        var angle = _h * Mathf.PI * 2f;
        var center = wheelRect.center;
        var centerRadius = WheelSize * WheelCenterRadiusRatio;
        var centerRect = new Rect(center.x - centerRadius, center.y - centerRadius, centerRadius * 2f, centerRadius * 2f);

        DrawWheelCenterPreview(centerRect);

        var markerPos = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (WheelSize * 0.45f);
        Handles.color = Color.white;
        Handles.DrawWireDisc(markerPos, Vector3.forward, 5f);
        Handles.color = Color.black;
        Handles.DrawWireDisc(markerPos, Vector3.forward, 6f);

        var e = Event.current;

        if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && wheelRect.Contains(e.mousePosition))
        {
            var dir = e.mousePosition - center;
            var dist = dir.magnitude / (WheelSize / 2f);
            if (dist >= 0.7f)
            {
                var a = Mathf.Atan2(dir.y, dir.x);
                if (a < 0) a += Mathf.PI * 2f;
                _h = a / (Mathf.PI * 2f);
                UpdateSelectedColorFromHSV();
                colorChanged = true;
                e.Use();
            }
        }

        DrawHarmonyMarkers(center);
    }

    void DrawSchemeControls()
    {
        var scheme = GetActiveScheme();
        EditorGUI.BeginChangeCheck();
        scheme = (PaletteScheme)EditorGUILayout.EnumPopup("Scheme", scheme);
        if (EditorGUI.EndChangeCheck())
            SetActiveScheme(scheme);
    }

    void DrawPickerPaletteSection()
    {
        if (_selectedPalette == null)
        {
            EditorGUILayout.HelpBox("Select a palette to edit.", MessageType.Info);
            return;
        }

        DrawPaletteGenerationControls(_selectedPalette);
        GUILayout.Space(6);
        DrawPalettePreview(_selectedPalette);
        GUILayout.Space(8);
        DrawPaletteActions(_selectedPalette);
        GUILayout.Space(10);
        DrawPaletteBottomButtons(_selectedPalette);
    }

    void ShowImportMenu(ColorPalette palette)
    {
        var menu = new GenericMenu();
        menu.AddItem(new GUIContent("Image..."), false, () => {
            var path = EditorUtility.OpenFilePanel("Import Palette from Image", "Assets", "png,jpg,jpeg");
            if (!string.IsNullOrEmpty(path))
            {
                var bytes = File.ReadAllBytes(path);
                var tex = new Texture2D(2, 2);
                if (tex.LoadImage(bytes))
                {
                    CreatePaletteFromTexture(tex, _texturePaletteCount);
                }
                DestroyImmediate(tex);
            }
        });
        menu.AddItem(new GUIContent("JSON..."), false, () => ImportJson(palette));
        menu.AddItem(new GUIContent("Unity Palette (.colors)..."), false, () => ImportUnityPalette(palette));
        menu.AddItem(new GUIContent("HEX List..."), false, () => ImportHex(palette));
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Tools/GPL (GIMP)..."), false, () => ImportGpl(palette));
        menu.AddItem(new GUIContent("Tools/ASE (Adobe)..."), false, () => ImportAse(palette));
        menu.AddItem(new GUIContent("Tools/ACO..."), false, () => ImportAco(palette));
        menu.ShowAsContext();
    }

    void ShowExportMenu(ColorPalette palette)
    {
        var menu = new GenericMenu();
        menu.AddItem(new GUIContent("Texture (PNG)..."), false, () => ExportPaletteTexture(palette));
        menu.AddItem(new GUIContent("LUT/16..."), false, () => ExportPaletteLut(palette, 16));
        menu.AddItem(new GUIContent("LUT/32..."), false, () => ExportPaletteLut(palette, 32));
        menu.AddItem(new GUIContent("Unity Palette (.colors)..."), false, () => ExportUnityPalette(palette));
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Data/JSON..."), false, () => ExportJson(palette));
        menu.AddItem(new GUIContent("Data/HEX List..."), false, () => ExportHex(palette));
        menu.AddItem(new GUIContent("Code/C# Array"), false, () => ExportCSharp(palette));
        menu.AddItem(new GUIContent("Code/CSS Variables"), false, () => ExportCss(palette));
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Tools/GPL (GIMP)"), false, () => ExportGpl(palette));
        menu.AddItem(new GUIContent("Tools/ASE (Adobe)"), false, () => ExportAse(palette));
        menu.AddItem(new GUIContent("Tools/ACO"), false, () => ExportAco(palette));
        menu.AddItem(new GUIContent("Tools/SVG"), false, () => ExportSvg(palette));
        menu.ShowAsContext();
    }

    void ImportJson(ColorPalette palette)
    {
        var path = EditorUtility.OpenFilePanel("Import JSON Palette", "Assets", "json");
        if (string.IsNullOrEmpty(path)) return;
        var json = File.ReadAllText(path);

        if (palette == null)
        {
            var assetPath = EditorUtility.SaveFilePanelInProject("Save Imported Palette", "NewPalette", "asset", "");
            if (string.IsNullOrEmpty(assetPath)) return;
            palette = ScriptableObject.CreateInstance<ColorPalette>();
            JsonUtility.FromJsonOverwrite(json, palette);
            AssetDatabase.CreateAsset(palette, assetPath);
        }
        else
        {
            Undo.RecordObject(palette, "Import JSON Palette");
            JsonUtility.FromJsonOverwrite(json, palette);
        }

        EditorUtility.SetDirty(palette);
        AssetDatabase.SaveAssets();
        PaletteDatabase.Refresh();
        SelectPalette(palette);
    }

    void ImportUnityPalette(ColorPalette palette)
    {
        var path = EditorUtility.OpenFilePanel("Import Unity Palette", "Assets", "colors");
        if (string.IsNullOrEmpty(path)) return;

        var relativePath = path;
        if (path.StartsWith(Application.dataPath))
            relativePath = "Assets" + path.Substring(Application.dataPath.Length - 6);

        var lib = AssetDatabase.LoadAssetAtPath<ScriptableObject>(relativePath);
        if (lib == null) return;

        var count = ColorPresetLibraryWrapper.Count(lib);
        var colors = new List<Color>();
        for (int i = 0; i < count; i++)
            colors.Add(ColorPresetLibraryWrapper.GetPreset(lib, i));

        if (colors.Count == 0) return;

        if (palette == null)
        {
            var assetPath = EditorUtility.SaveFilePanelInProject("Save Imported Palette", Path.GetFileNameWithoutExtension(path), "asset", "");
            if (string.IsNullOrEmpty(assetPath)) return;
            palette = ScriptableObject.CreateInstance<ColorPalette>();
            palette.paletteName = Path.GetFileNameWithoutExtension(assetPath);
            palette.colors = colors.ToArray();
            palette.GenerationMode = PaletteGenerationMode.Manual;
            AssetDatabase.CreateAsset(palette, assetPath);
        }
        else
        {
            Undo.RecordObject(palette, "Import Unity Palette");
            palette.colors = colors.ToArray();
            palette.GenerationMode = PaletteGenerationMode.Manual;
        }

        EditorUtility.SetDirty(palette);
        AssetDatabase.SaveAssets();
        PaletteDatabase.Refresh();
        SelectPalette(palette);
    }

    void ImportHex(ColorPalette palette)
    {
        var path = EditorUtility.OpenFilePanel("Import HEX List", "Assets", "txt");
        if (string.IsNullOrEmpty(path)) return;

        var lines = File.ReadAllLines(path);
        var colors = new List<Color>();
        foreach (var line in lines)
        {
            if (ColorUtility.TryParseHtmlString(line.Trim(), out var c))
                colors.Add(c);
        }

        if (colors.Count == 0) return;

        if (palette == null)
        {
            var assetPath = EditorUtility.SaveFilePanelInProject("Save Imported Palette", Path.GetFileNameWithoutExtension(path), "asset", "");
            if (string.IsNullOrEmpty(assetPath)) return;
            palette = ScriptableObject.CreateInstance<ColorPalette>();
            palette.paletteName = Path.GetFileNameWithoutExtension(assetPath);
            palette.colors = colors.ToArray();
            palette.GenerationMode = PaletteGenerationMode.Manual;
            AssetDatabase.CreateAsset(palette, assetPath);
        }
        else
        {
            Undo.RecordObject(palette, "Import HEX List");
            palette.colors = colors.ToArray();
            palette.GenerationMode = PaletteGenerationMode.Manual;
        }

        EditorUtility.SetDirty(palette);
        AssetDatabase.SaveAssets();
        PaletteDatabase.Refresh();
        SelectPalette(palette);
    }

    void ImportGpl(ColorPalette palette) => Debug.Log("GPL Import not yet implemented.");
    void ImportAse(ColorPalette palette) => Debug.Log("ASE Import not yet implemented.");
    void ImportAco(ColorPalette palette) => Debug.Log("ACO Import not yet implemented.");

    void ExportJson(ColorPalette palette)
    {
        if (palette == null) return;
        var path = EditorUtility.SaveFilePanel("Export JSON", "Assets", GetPaletteDisplayName(palette), "json");
        if (string.IsNullOrEmpty(path)) return;
        File.WriteAllText(path, JsonUtility.ToJson(palette, true));
        AssetDatabase.Refresh();
    }

    void ExportHex(ColorPalette palette)
    {
        if (palette == null) return;
        var path = EditorUtility.SaveFilePanel("Export HEX List", "Assets", GetPaletteDisplayName(palette), "txt");
        if (string.IsNullOrEmpty(path)) return;
        var lines = palette.colors.Select(c => "#" + ColorUtility.ToHtmlStringRGBA(c)).ToArray();
        File.WriteAllLines(path, lines);
        AssetDatabase.Refresh();
    }

    void ExportCSharp(ColorPalette palette)
    {
        if (palette == null) return;
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"public static readonly Color[] {GetPaletteDisplayName(palette).Replace(" ", "_")} = new Color[] {{");
        foreach (var c in palette.colors)
            sb.AppendLine($"    new Color({c.r}f, {c.g}f, {c.b}f, {c.a}f),");
        sb.AppendLine("};");
        EditorGUIUtility.systemCopyBuffer = sb.ToString();
        EditorUtility.DisplayDialog("Export C#", "C# array code copied to clipboard.", "OK");
    }

    void ExportCss(ColorPalette palette)
    {
        if (palette == null) return;
        var sb = new System.Text.StringBuilder();
        sb.AppendLine(":root {");
        var name = GetPaletteDisplayName(palette).ToLower().Replace(" ", "-");
        for (int i = 0; i < palette.colors.Length; i++)
            sb.AppendLine($"    --{name}-{i}: #{ColorUtility.ToHtmlStringRGBA(palette.colors[i])};");
        sb.AppendLine("}");
        EditorGUIUtility.systemCopyBuffer = sb.ToString();
        EditorUtility.DisplayDialog("Export CSS", "CSS variables copied to clipboard.", "OK");
    }

    void ExportGpl(ColorPalette palette) => Debug.Log("GPL Export not yet implemented.");
    void ExportAse(ColorPalette palette) => Debug.Log("ASE Export not yet implemented.");
    void ExportSvg(ColorPalette palette) => Debug.Log("SVG Export not yet implemented.");
    void ExportAco(ColorPalette palette) => Debug.Log("ACO Export not yet implemented.");

    void DrawPalettePreview(ColorPalette palette)
    {
        var colors = palette.ToArray();
        if (colors == null || colors.Length == 0)
        {
            EditorGUILayout.HelpBox("Palette is empty.", MessageType.Info);
            return;
        }

        var rect = EditorGUILayout.GetControlRect(GUILayout.Height(PalettePreviewHeight));
        var count = colors.Length;
        var swatchWidth = rect.width / Mathf.Max(1, count);

        for (var i = 0; i < count; i++)
        {
            var r = new Rect(rect.x + i * swatchWidth, rect.y, swatchWidth, rect.height);
            var c = colors[i];
            if (c.a < 1f) c = c.WithAlpha(1f);
            EditorGUI.DrawRect(r, c);

            if (i == _selectedPaletteColorIndex)
                DrawRectOutline(r, Color.white, 2f);
        }

        var e = Event.current;
        if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
        {
            var idx = Mathf.Clamp(Mathf.FloorToInt((e.mousePosition.x - rect.x) / swatchWidth), 0, count - 1);
            _selectedPaletteColorIndex = idx;
            _selectedColor = colors[idx];
            UpdateHSV();
            NotifyColorChanged();
            e.Use();
        }
    }

    void DrawPaletteGenerationControls(ColorPalette palette)
    {
        EditorGUI.BeginChangeCheck();
        var generationMode = (PaletteGenerationMode)EditorGUILayout.EnumPopup("Mode", palette.GenerationMode);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(palette, "Change Palette Mode");
            palette.GenerationMode = generationMode;
            if (palette.GenerationMode == PaletteGenerationMode.Generated)
                RegeneratePalette(palette, recordUndo: false);
            EditorUtility.SetDirty(palette);
        }

        if (palette.GenerationMode != PaletteGenerationMode.Generated) return;

        EditorGUI.BeginChangeCheck();
        var scheme = palette.Scheme;
        var baseColor = EditorGUILayout.ColorField("Base Color", palette.BaseColor);
        var shades = EditorGUILayout.IntSlider("Shades", palette.Shades, 1, 16);

        var hueCount = palette.HueCount;
        if (scheme == PaletteScheme.Custom || scheme == PaletteScheme.Monochromatic)
        {
            EditorGUILayout.LabelField($"Hues: {PaletteGenerator.GetEffectiveHueCount(palette)}");
        }
        else
        {
            var minHues = PaletteGenerator.GetMinHueCount(scheme);
            hueCount = EditorGUILayout.IntSlider("Hues", palette.HueCount, minHues, 128);
        }

        var saturation = EditorGUILayout.Slider("Saturation", palette.Saturation, 0f, 1f);
        var value = EditorGUILayout.Slider("Value", palette.Value, 0f, 1f);
        var minB = palette.MinBrightness;
        var maxB = palette.MaxBrightness;
        EditorGUILayout.MinMaxSlider("Brightness", ref minB, ref maxB, 0f, 1f);

        var analogousStep = palette.AnalogousStepDegrees;
        var splitComplementary = palette.SplitComplementaryDegrees;
        var spectrumEndpoints = palette.SpectrumIncludeBlackWhite;

        if (scheme == PaletteScheme.Analogous)
            analogousStep = EditorGUILayout.Slider("Analogous Step", palette.AnalogousStepDegrees, 5f, 90f);
        if (scheme == PaletteScheme.SplitComplementary)
            splitComplementary = EditorGUILayout.Slider("Split Angle", palette.SplitComplementaryDegrees, 5f, 90f);
        if (scheme == PaletteScheme.Spectrum)
            spectrumEndpoints = EditorGUILayout.Toggle("Spectrum Endpoints", palette.SpectrumIncludeBlackWhite);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(palette, "Update Palette Settings");
            palette.BaseColor = baseColor;
            palette.Shades = shades;
            palette.HueCount = hueCount;
            palette.Saturation = saturation;
            palette.Value = value;
            palette.MinBrightness = minB;
            palette.MaxBrightness = maxB;
            palette.AnalogousStepDegrees = analogousStep;
            palette.SplitComplementaryDegrees = splitComplementary;
            palette.SpectrumIncludeBlackWhite = spectrumEndpoints;

            RegeneratePalette(palette, recordUndo: false);
            EditorUtility.SetDirty(palette);
        }
    }

    void DrawPaletteActions(ColorPalette palette)
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Current"))
            AddColorToPalette(palette, _selectedColor);
        GUI.enabled = _selectedPaletteColorIndex >= 0;
        if (GUILayout.Button("Replace Selected"))
            ReplaceColorAt(palette, _selectedPaletteColorIndex, _selectedColor);
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        if (palette.GenerationMode == PaletteGenerationMode.Manual)
        {
            EditorGUILayout.BeginHorizontal();
            var scheme = GetActiveScheme();
            GUI.enabled = scheme != PaletteScheme.Custom && scheme != PaletteScheme.Monochromatic;
            if (GUILayout.Button("Add Scheme Colors"))
                AddSchemeColorsToPalette();
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUI.enabled = _selectedPaletteColorIndex >= 0;
            if (GUILayout.Button("Remove Selected"))
                RemoveColorAt(palette, _selectedPaletteColorIndex);
            GUI.enabled = true;
            if (GUILayout.Button("Clear"))
                ClearPaletteColors(palette);
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Bake To Manual"))
                BakeToManual(palette);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Texture", GUILayout.Width(52));
        _lutSourceTexture = (Texture2D)EditorGUILayout.ObjectField(_lutSourceTexture, typeof(Texture2D), false);
        GUI.enabled = _lutSourceTexture != null;
        if (GUILayout.Button(new GUIContent("Make Palette", "Create a new palette from a texture"), GUILayout.Width(120)))
            CreatePaletteFromTexture(_lutSourceTexture, _texturePaletteCount);
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Colors", GUILayout.Width(52));
        _texturePaletteCount = EditorGUILayout.IntSlider(_texturePaletteCount, 2, 128);
        EditorGUILayout.EndHorizontal();
    }

    void DrawPaletteBottomButtons(ColorPalette palette)
    {
        var width = Mathf.Max(20, (EditorGUIUtility.currentViewWidth - 40) / 3f);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("New", GUILayout.Width(width))) CreateNewPaletteWindow.ShowWindow();
        if (GUILayout.Button("Load", GUILayout.Width(width))) OpenPalettePicker();
        
        GUI.enabled = palette != null;
        if (GUILayout.Button("Save", GUILayout.Width(width))) SavePaletteAsset(palette);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Save As", GUILayout.Width(width))) DuplicatePalette(palette);
        
        if (EditorGUILayout.DropdownButton(new GUIContent("Import"), FocusType.Passive, GUILayout.Width(width)))
            ShowImportMenu(palette);
            
        if (EditorGUILayout.DropdownButton(new GUIContent("Export"), FocusType.Passive, GUILayout.Width(width)))
            ShowExportMenu(palette);
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
    }

    void DrawLibraryTab()
    {
        if (_selectedPalette == null)
        {
            EditorGUILayout.HelpBox("Select a palette to view details.", MessageType.Info);
            return;
        }

        EditorGUILayout.LabelField($"Palette: {GetPaletteDisplayName(_selectedPalette)}", EditorStyles.boldLabel);

        DrawSwatchPreview(_selectedPalette.ToArray(), 40f);

        GUILayout.Space(8);
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Set as Global Default"))
            SetAsGlobalDefault(_selectedPalette);
        if (GUILayout.Button("Duplicate"))
            DuplicatePalette(_selectedPalette);
        if (GUILayout.Button("Delete"))
            DeletePalette(_selectedPalette);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(8);
        EditorGUILayout.LabelField("Color Theory", EditorStyles.boldLabel);
        _showPaletteOverview = EditorGUILayout.Foldout(_showPaletteOverview, "Palette Overview");
        if (_showPaletteOverview)
        {
            DrawPaletteOverview(_selectedPalette.ToArray());
            GUILayout.Space(6);
        }

        _colorTheoryMode = GUILayout.Toolbar(_colorTheoryMode, ColorTheoryTabs);
        _colorTheoryScroll = GUILayout.BeginScrollView(_colorTheoryScroll, GUILayout.Height(200));
        DrawColorTheory(_selectedPalette.ToArray());
        GUILayout.EndScrollView();

        GUILayout.Space(8);
        _showColorBlindPreview = EditorGUILayout.Foldout(_showColorBlindPreview, "Color Blindness Preview");
        if (_showColorBlindPreview)
        {
            _colorBlindMode = GUILayout.Toolbar(_colorBlindMode, ColorBlindTabs);
            var simulated = SimulateColorBlindness(_selectedPalette.ToArray(), _colorBlindMode);
            DrawSwatchPreview(simulated, 32f);
        }
    }

    void DrawPaletteGrid(ColorPalette palette, bool allowEdit)
    {
        var colors = palette.ToArray();
        if (colors == null || colors.Length == 0)
        {
            EditorGUILayout.HelpBox("Palette is empty.", MessageType.Info);
            return;
        }

        var columns = palette.GenerationMode == PaletteGenerationMode.Generated
            ? Mathf.Max(1, PaletteGenerator.GetEffectiveHueCount(palette))
            : Mathf.Max(1, Mathf.FloorToInt((EditorGUIUtility.currentViewWidth - 260f) / 26f));

        var rows = palette.GenerationMode == PaletteGenerationMode.Generated
            ? Mathf.Max(1, palette.Shades)
            : Mathf.CeilToInt(colors.Length / (float)columns);

        var swatchSize = Mathf.Clamp((EditorGUIUtility.currentViewWidth - 260f) / columns, SwatchMinSize, 32f);

        for (var row = 0; row < rows; row++)
        {
            EditorGUILayout.BeginHorizontal();
            for (var col = 0; col < columns; col++)
            {
                var idx = row * columns + col;
                var rect = GUILayoutUtility.GetRect(swatchSize, swatchSize, GUILayout.Width(swatchSize), GUILayout.Height(swatchSize));

                if (idx < colors.Length)
                {
                    Handles.DrawSolidRectangleWithOutline(rect, colors[idx], Color.black);
                    if (idx == _selectedPaletteColorIndex)
                        Handles.DrawSolidRectangleWithOutline(rect, colors[idx], Color.white);
                }
                else if (palette.GenerationMode != PaletteGenerationMode.Manual)
                {
                    EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.1f));
                }

                if (idx < colors.Length && Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
                {
                    if (Event.current.button == 0)
                    {
                        _selectedPaletteColorIndex = idx;
                        _selectedColor = colors[idx];
                        UpdateHSV();
                        NotifyColorChanged();
                        Event.current.Use();
                    }
                    else if (Event.current.button == 1 && allowEdit)
                    {
                        RemoveColorAt(palette, idx);
                        Event.current.Use();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
    void UpdateHSV()
    {
        Color.RGBToHSV(_selectedColor, out _h, out _s, out _v);
        _a = _selectedColor.a;
        UpdateLightnessBarTexture();
    }

    void UpdateSelectedColorFromHSV()
    {
        _selectedColor = Color.HSVToRGB(_h, _s, _v).WithAlpha(_a);
        UpdateLightnessBarTexture();
    }

    void GenerateTextures()
    {
        if (_hueWheel == null)
        {
            _hueWheel = new Texture2D(WheelSize, WheelSize);
            for (var y = 0; y < WheelSize; y++)
            {
                for (var x = 0; x < WheelSize; x++)
                {
                    var dx = (x - WheelSize / 2f) / (WheelSize / 2f);
                    var dy = (y - WheelSize / 2f) / (WheelSize / 2f);
                    var dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist >= 0.8f && dist <= 1f)
                    {
                        var angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                        if (angle < 0) angle += 360f;
                        _hueWheel.SetPixel(x, y, Color.HSVToRGB(angle / 360f, 1f, 1f));
                    }
                    else
                    {
                        _hueWheel.SetPixel(x, y, Color.clear);
                    }
                }
            }
            _hueWheel.Apply();
        }
        UpdateLightnessBarTexture();
    }

    void UpdateLightnessBarTexture()
    {
        if (_lightnessBar == null)
            _lightnessBar = new Texture2D(256, 1, TextureFormat.RGBA32, false);

        for (var i = 0; i < 256; i++)
        {
            var v = i / 255f;
            _lightnessBar.SetPixel(i, 0, Color.HSVToRGB(_h, _s, v));
        }
        _lightnessBar.Apply();
    }

    void DestroyTextures()
    {
        if (_hueWheel != null)
        {
            DestroyImmediate(_hueWheel);
            _hueWheel = null;
        }
        if (_centerPie != null)
        {
            DestroyImmediate(_centerPie);
            _centerPie = null;
        }
        if (_lightnessBar != null)
        {
            DestroyImmediate(_lightnessBar);
            _lightnessBar = null;
        }
    }

    void DrawHarmonyMarkers(Vector2 center)
    {
        var hues = new List<float>();
        GetSchemeHues(hues);
        if (hues.Count <= 1) return;

        hues.Sort();
        var points = new Vector2[hues.Count];
        for (var i = 0; i < hues.Count; i++)
            points[i] = center + GetPosOnWheel(hues[i]);

        Handles.color = new Color(1f, 1f, 1f, 0.35f);
        for (var i = 0; i < points.Length; i++)
            DrawThickLine(points[i], points[(i + 1) % points.Length], HarmonyLineWidth);

        for (var i = 0; i < points.Length; i++)
        {
            Handles.color = new Color(1f, 1f, 1f, 0.6f);
            Handles.DrawSolidDisc(points[i], Vector3.forward, 4f);
            Handles.color = Color.black;
            Handles.DrawWireDisc(points[i], Vector3.forward, 4f);
        }
    }

    void GetSchemeHues(List<float> hues)
    {
        hues.Clear();
        hues.Add(_h);

        var scheme = GetActiveScheme();
        if (scheme == PaletteScheme.Custom || scheme == PaletteScheme.Monochromatic)
            return;

        var analogStep = (_selectedPalette != null ? _selectedPalette.AnalogousStepDegrees : 30f) / 360f;
        var splitStep = (_selectedPalette != null ? _selectedPalette.SplitComplementaryDegrees : 30f) / 360f;

        switch (scheme)
        {
            case PaletteScheme.UI_Kit:
                hues.Add(Mathf.Repeat(_h + 0.1f, 1f));
                hues.Add(Mathf.Repeat(_h + 0.5f, 1f));
                break;
            case PaletteScheme.Complementary:
                hues.Add(Mathf.Repeat(_h + 0.5f, 1f));
                break;
            case PaletteScheme.Triadic:
                hues.Add(Mathf.Repeat(_h + 1f / 3f, 1f));
                hues.Add(Mathf.Repeat(_h + 2f / 3f, 1f));
                break;
            case PaletteScheme.Analogous:
                hues.Add(Mathf.Repeat(_h - analogStep, 1f));
                hues.Add(Mathf.Repeat(_h + analogStep, 1f));
                break;
            case PaletteScheme.SplitComplementary:
                hues.Add(Mathf.Repeat(_h + 0.5f - splitStep, 1f));
                hues.Add(Mathf.Repeat(_h + 0.5f + splitStep, 1f));
                break;
            case PaletteScheme.Tetradic:
            case PaletteScheme.Square:
                hues.Add(Mathf.Repeat(_h + 0.25f, 1f));
                hues.Add(Mathf.Repeat(_h + 0.5f, 1f));
                hues.Add(Mathf.Repeat(_h + 0.75f, 1f));
                break;
            case PaletteScheme.Spectrum:
                var count = Mathf.Clamp(_selectedPalette != null ? _selectedPalette.HueCount : 8, 3, 12);
                hues.Clear();
                for (var i = 0; i < count; i++)
                    hues.Add(Mathf.Repeat(_h + i / (float)count, 1f));
                break;
        }
    }

    Vector2 GetPosOnWheel(float hue)
    {
        var angle = hue * Mathf.PI * 2f;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (WheelSize * 0.45f);
    }

    void DrawWheelCenterPreview(Rect rect)
    {
        var center = rect.center;
        var radius = rect.width * 0.5f;

        UpdateCenterPieTexture();
        if (_centerPie != null)
            GUI.DrawTexture(rect, _centerPie, ScaleMode.StretchToFill, true);

        Handles.color = _selectedColor.SuggestedTextColor();
        Handles.DrawWireDisc(center, Vector3.forward, radius);
    }

    Color ResolveCenterColor(float hue)
    {
        if (_selectedPalette == null || _selectedPalette.Count == 0)
            return Color.HSVToRGB(hue, _s, _v);

        var source = _selectedPalette.GenerationMode == PaletteGenerationMode.Generated
            ? PaletteGenerator.BuildBaseColors(_selectedPalette)
            : new List<Color>(_selectedPalette.ToArray());

        if (source.Count == 0)
            return Color.HSVToRGB(hue, _s, _v);

        var best = source[0];
        var bestDist = float.MaxValue;
        var bestSat = source[0];
        var bestSatDist = float.MaxValue;
        var hasSaturated = false;
        for (var i = 0; i < source.Count; i++)
        {
            Color.RGBToHSV(source[i], out var h, out var s, out _);
            var dist = HueDistance(h, hue);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = source[i];
            }

            if (s >= 0.08f && dist < bestSatDist)
            {
                bestSatDist = dist;
                bestSat = source[i];
                hasSaturated = true;
            }
        }

        var result = hasSaturated ? bestSat : best;
        result.a = 1f;
        return result;
    }

    static float HueDistance(float a, float b) =>
        Mathf.Abs(Mathf.DeltaAngle(a * 360f, b * 360f));

    static float MidAngleDeg(float aHue, float bHue)
    {
        var a = Mathf.Repeat(aHue, 1f) * 360f;
        var b = Mathf.Repeat(bHue, 1f) * 360f;
        if (b < a) b += 360f;
        return Mathf.Repeat((a + b) * 0.5f, 360f);
    }

    void UpdateCenterPieTexture()
    {
        if (_centerPie == null || _centerPie.width != CenterPieSize)
            _centerPie = new Texture2D(CenterPieSize, CenterPieSize, TextureFormat.RGBA32, false);

        var hues = new List<float>();
        GetSchemeHues(hues);
        hues.Sort();

        var colors = new Color[hues.Count];
        for (var i = 0; i < hues.Count; i++)
            colors[i] = ResolveCenterColor(hues[i]);

        var boundaries = new (float start, float end)[hues.Count];
        for (var i = 0; i < hues.Count; i++)
        {
            var prev = hues[(i - 1 + hues.Count) % hues.Count];
            var next = hues[(i + 1) % hues.Count];
            boundaries[i] = (MidAngleDeg(prev, hues[i]), MidAngleDeg(hues[i], next));
        }

        var size = CenterPieSize;
        var center = (size - 1) * 0.5f;
        var radius = center;
        var r2 = radius * radius;

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var dx = x - center;
                var dy = y - center;
                var dist2 = dx * dx + dy * dy;
                if (dist2 > r2)
                {
                    _centerPie.SetPixel(x, y, Color.clear);
                    continue;
                }

                var angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                if (angle < 0f) angle += 360f;

                var idx = 0;
                for (var i = 0; i < boundaries.Length; i++)
                {
                    if (AngleInRange(angle, boundaries[i].start, boundaries[i].end))
                    {
                        idx = i;
                        break;
                    }
                }

                _centerPie.SetPixel(x, y, colors[idx]);
            }
        }

        _centerPie.Apply();
    }

    static bool AngleInRange(float angle, float start, float end)
    {
        angle = Mathf.Repeat(angle, 360f);
        start = Mathf.Repeat(start, 360f);
        end = Mathf.Repeat(end, 360f);
        if (start <= end) return angle >= start && angle < end;
        return angle >= start || angle < end;
    }


    static void DrawThickLine(Vector2 a, Vector2 b, float width)
    {
        var pts = new[] { (Vector3)a, (Vector3)b };
        Handles.DrawAAPolyLine(width, pts);
    }

    static void DrawRectOutline(Rect rect, Color color, float width)
    {
        var p0 = new Vector3(rect.xMin, rect.yMin);
        var p1 = new Vector3(rect.xMax, rect.yMin);
        var p2 = new Vector3(rect.xMax, rect.yMax);
        var p3 = new Vector3(rect.xMin, rect.yMax);
        Handles.color = color;
        Handles.DrawAAPolyLine(width, new[] { p0, p1, p2, p3, p0 });
    }

    void ApplyPickerSvToPalette()
    {
        if (_selectedPalette == null) return;

        if (_selectedPalette.GenerationMode == PaletteGenerationMode.Generated)
        {
            Undo.RecordObject(_selectedPalette, "Adjust Palette SV");
            _selectedPalette.Saturation = _s;
            _selectedPalette.Value = _v;
            _selectedPalette.BaseColor = Color.HSVToRGB(_h, _s, _v).WithAlpha(_a);
            RegeneratePalette(_selectedPalette, recordUndo: false);
            EditorUtility.SetDirty(_selectedPalette);
        }
        else if (_selectedPaletteColorIndex >= 0)
        {
            ReplaceColorAt(_selectedPalette, _selectedPaletteColorIndex, _selectedColor);
        }
    }

    void DrawCompactPaletteSelector()
    {
        var palettes = PaletteDatabase.Palettes;

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (palettes.Count == 0)
        {
            GUILayout.Label("No palettes found.", EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("New", EditorStyles.toolbarButton, GUILayout.Width(50)))
                CreateNewPaletteWindow.ShowWindow();
            EditorGUILayout.EndHorizontal();
            return;
        }

        var paletteNames = new string[palettes.Count];
        var currentIndex = 0;
        for (var i = 0; i < palettes.Count; i++)
        {
            var pal = palettes[i];
            paletteNames[i] = GetPaletteDisplayName(pal);
            if (pal == _selectedPalette) currentIndex = i;
        }

        GUILayout.Label("Palette", EditorStyles.miniLabel, GUILayout.Width(50));
        var newIndex = EditorGUILayout.Popup(currentIndex, paletteNames, EditorStyles.toolbarPopup, GUILayout.MinWidth(180));
        if (newIndex != currentIndex)
            SelectPalette(palettes[newIndex]);

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("New", EditorStyles.toolbarButton, GUILayout.Width(50)))
            CreateNewPaletteWindow.ShowWindow();
        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
        {
            PaletteDatabase.Refresh();
            RefreshDiscoveredTags();
            EnsureSelectedPalette();
        }
        GUI.enabled = _selectedPalette != null;
        if (GUILayout.Button("Ping", EditorStyles.toolbarButton, GUILayout.Width(50)))
        {
            Selection.activeObject = _selectedPalette;
            EditorGUIUtility.PingObject(_selectedPalette);
        }
        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();
    }

    void AddSchemeColorsToPalette()
    {
        if (_selectedPalette == null) return;

        var scheme = GetActiveScheme();
        if (scheme == PaletteScheme.Custom || scheme == PaletteScheme.Monochromatic) return;

        var hues = new List<float>();
        GetSchemeHues(hues);
        var list = new List<Color>(_selectedPalette.ToArray());
        foreach (var hue in hues)
            list.Add(Color.HSVToRGB(hue, _s, _v).WithAlpha(_a));

        Undo.RecordObject(_selectedPalette, "Add Scheme Colors");
        _selectedPalette.colors = list.ToArray();
        EditorUtility.SetDirty(_selectedPalette);
    }

    void RegeneratePalette(ColorPalette palette, bool recordUndo)
    {
        if (palette == null) return;
        if (recordUndo) Undo.RecordObject(palette, "Regenerate Palette");
        palette.colors = PaletteGenerator.Generate(palette);
        EditorUtility.SetDirty(palette);
    }

    void BakeToManual(ColorPalette palette)
    {
        if (palette == null) return;
        Undo.RecordObject(palette, "Bake Palette");
        palette.GenerationMode = PaletteGenerationMode.Manual;
        EditorUtility.SetDirty(palette);
    }

    void AddColorToPalette(ColorPalette palette, Color color)
    {
        if (palette == null) return;
        var list = new List<Color>(palette.ToArray()) { color };
        Undo.RecordObject(palette, "Add Palette Color");
        palette.colors = list.ToArray();
        EditorUtility.SetDirty(palette);
    }

    void ReplaceColorAt(ColorPalette palette, int index, Color color)
    {
        if (palette == null || index < 0) return;
        var list = new List<Color>(palette.ToArray());
        if (index >= list.Count) return;
        Undo.RecordObject(palette, "Replace Palette Color");
        list[index] = color;
        palette.colors = list.ToArray();
        EditorUtility.SetDirty(palette);
    }

    void RemoveColorAt(ColorPalette palette, int index)
    {
        if (palette == null || index < 0) return;
        var list = new List<Color>(palette.ToArray());
        if (index >= list.Count) return;
        Undo.RecordObject(palette, "Remove Palette Color");
        list.RemoveAt(index);
        palette.colors = list.ToArray();
        _selectedPaletteColorIndex = Mathf.Clamp(_selectedPaletteColorIndex, -1, palette.Count - 1);
        EditorUtility.SetDirty(palette);
    }

    void ClearPaletteColors(ColorPalette palette)
    {
        if (palette == null) return;
        Undo.RecordObject(palette, "Clear Palette");
        palette.colors = new Color[0];
        _selectedPaletteColorIndex = -1;
        EditorUtility.SetDirty(palette);
    }

    void ExportPaletteTexture(ColorPalette palette)
    {
        if (palette == null) return;
        var tex = PaletteUtils.PaletteToTexture(palette, false);
        var path = EditorUtility.SaveFilePanelInProject("Export Palette Texture", $"{GetPaletteDisplayName(palette)}_Palette", "png", "Export palette texture");
        if (string.IsNullOrEmpty(path))
        {
            DestroyImmediate(tex);
            return;
        }

        File.WriteAllBytes(path, tex.EncodeToPNG());
        DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path);

        if (AssetImporter.GetAtPath(path) is TextureImporter importer)
        {
            importer.isReadable = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }

    void ExportPaletteLut(ColorPalette palette, int size)
    {
        if (palette == null) return;
        var tex = PaletteUtils.PaletteToLut(palette, size);
        if (tex == null) return;

        var path = EditorUtility.SaveFilePanelInProject("Export Palette LUT", $"{GetPaletteDisplayName(palette)}_LUT_{size}", "png", "Export palette LUT");
        if (string.IsNullOrEmpty(path))
        {
            DestroyImmediate(tex);
            return;
        }

        File.WriteAllBytes(path, tex.EncodeToPNG());
        DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path);

        if (AssetImporter.GetAtPath(path) is TextureImporter importer)
        {
            importer.isReadable = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }

    void ExportUnityPalette(ColorPalette palette)
    {
        if (palette == null || palette.Count == 0)
        {
            EditorUtility.DisplayDialog("Export Unity Palette", "Palette is empty.", "OK");
            return;
        }

        var colors = palette.ToArray();
        if (colors.Length > UnityPaletteSlotCount)
        {
            EditorUtility.DisplayDialog(
                "Export Unity Palette",
                $"Unity palette export supports up to {UnityPaletteSlotCount} colors. This palette has {colors.Length}.",
                "OK");
            return;
        }

        var lib = ColorPresetLibraryWrapper.CreateLibrary();
        var paletteName = GetPaletteDisplayName(palette);
        for (var i = 0; i < UnityPaletteSlotCount; i++)
        {
            var c = i < colors.Length ? colors[i] : Color.black;
            ColorPresetLibraryWrapper.Add(lib, c, $"{paletteName} {i + 1:00}");
        }

        var path = EditorUtility.SaveFilePanelInProject(
            "Export Unity Palette",
            paletteName,
            "colors",
            "Export Unity color preset library");
        if (string.IsNullOrEmpty(path))
        {
            DestroyImmediate(lib);
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null)
        {
            if (!EditorUtility.DisplayDialog("Overwrite Palette?", $"Overwrite '{path}'?", "Overwrite", "Cancel"))
            {
                DestroyImmediate(lib);
                return;
            }

            AssetDatabase.DeleteAsset(path);
        }

        AssetDatabase.CreateAsset(lib, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorGUIUtility.PingObject(lib);
    }

    void CreatePaletteFromTexture(Texture2D tex, int colorCount)
    {
        if (!tex)
        {
            EditorUtility.DisplayDialog("Create Palette From Texture", "Select a texture.", "OK");
            return;
        }

        if (!tex.isReadable)
        {
            EditorUtility.DisplayDialog("Create Palette From Texture", "Texture must be Read/Write enabled in its importer.", "OK");
            return;
        }

        var extracted = PaletteExtraction.ExtractColors(tex, colorCount);
        if (extracted == null || extracted.Length == 0)
        {
            EditorUtility.DisplayDialog("Create Palette From Texture", "Failed to extract colors.", "OK");
            return;
        }

        var defaultName = $"{tex.name}_Palette";
        var path = EditorUtility.SaveFilePanelInProject(
            "Create Palette From Texture",
            defaultName,
            "asset",
            "Choose where to save the palette asset.");
        if (string.IsNullOrEmpty(path)) return;

        if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null)
        {
            if (!EditorUtility.DisplayDialog("Overwrite Palette?", $"Overwrite '{path}'?", "Overwrite", "Cancel"))
                return;
            AssetDatabase.DeleteAsset(path);
        }

        var newPalette = ScriptableObject.CreateInstance<ColorPalette>();
        newPalette.paletteName = Path.GetFileNameWithoutExtension(path);
        newPalette.colors = extracted;
        newPalette.GenerationMode = PaletteGenerationMode.Manual;
        newPalette.Scheme = PaletteScheme.Custom;
        newPalette.HueCount = Mathf.Max(1, extracted.Length);
        newPalette.Shades = 1;
        newPalette.Saturation = 1f;
        newPalette.Value = 1f;
        newPalette.MinBrightness = 0f;
        newPalette.MaxBrightness = 1f;
        newPalette.BaseColor = extracted.Length > 0 ? extracted[0] : Color.white;

        AssetDatabase.CreateAsset(newPalette, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        PaletteDatabase.Refresh();

        SelectPalette(newPalette);
        EditorGUIUtility.PingObject(newPalette);
        DLog.Log($"Created palette from texture '{tex.name}' at {path}");
    }

    void SavePaletteAsset(ColorPalette palette)
    {
        if (palette == null) return;
        EditorUtility.SetDirty(palette);
        AssetDatabase.SaveAssets();
        EditorGUIUtility.PingObject(palette);
    }

    void OpenPalettePicker()
    {
        _palettePickerId = GUIUtility.GetControlID(FocusType.Passive);
        EditorGUIUtility.ShowObjectPicker<ColorPalette>(_selectedPalette, false, string.Empty, _palettePickerId);
    }

    void HandleObjectPicker()
    {
        var e = Event.current;
        if (e == null) return;

        if (e.commandName != "ObjectSelectorUpdated" && e.commandName != "ObjectSelectorClosed") return;
        if (EditorGUIUtility.GetObjectPickerControlID() != _palettePickerId) return;

        var picked = EditorGUIUtility.GetObjectPickerObject() as ColorPalette;
        if (picked != null)
            SelectPalette(picked);

        if (e.commandName == "ObjectSelectorClosed")
            _palettePickerId = -1;
    }

    void SetAsGlobalDefault(ColorPalette palette)
    {
        if (palette == null) return;
        var path = "Assets/Resources";
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder("Assets", "Resources");

        var assetPath = "Assets/Resources/PaletteSettings.asset";
        var settings = AssetDatabase.LoadAssetAtPath<PaletteSettings>(assetPath);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<PaletteSettings>();
            AssetDatabase.CreateAsset(settings, assetPath);
        }

        settings.defaultPalette = palette;
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
        Selection.activeObject = settings;
        EditorGUIUtility.PingObject(settings);
    }

    void DuplicatePalette(ColorPalette palette)
    {
        if (palette == null) return;
        var path = AssetDatabase.GetAssetPath(palette);
        if (string.IsNullOrEmpty(path)) return;

        var newPath = AssetDatabase.GenerateUniqueAssetPath(path);
        AssetDatabase.CopyAsset(path, newPath);
        AssetDatabase.Refresh();
        PaletteDatabase.Refresh();
        RefreshDiscoveredTags();
        _selectedPalette = AssetDatabase.LoadAssetAtPath<ColorPalette>(newPath);
    }

    void DeletePalette(ColorPalette palette)
    {
        if (palette == null) return;
        var path = AssetDatabase.GetAssetPath(palette);
        if (string.IsNullOrEmpty(path)) return;
        if (!EditorUtility.DisplayDialog("Delete Palette", $"Delete '{GetPaletteDisplayName(palette)}'?", "Delete", "Cancel")) return;

        AssetDatabase.DeleteAsset(path);
        PaletteDatabase.Refresh();
        RefreshDiscoveredTags();
        EnsureSelectedPalette();
    }

    void DrawSwatchPreview(Color[] colors, float height)
    {
        var rect = EditorGUILayout.GetControlRect(GUILayout.Height(height));
        if (colors == null || colors.Length == 0) return;

        var sw = rect.width / colors.Length;
        for (var i = 0; i < colors.Length; i++)
        {
            var r = new Rect(rect.x + i * sw, rect.y, sw, rect.height);
            EditorGUI.DrawRect(r, colors[i]);
            EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, 1), Color.black);
        }
    }

    void DrawColorTheory(Color[] colors)
    {
        if (colors == null || colors.Length == 0) return;

        switch (_colorTheoryMode)
        {
            case 0:
                EditorGUILayout.LabelField("Complementary Colors");
                for (var i = 0; i < colors.Length; i++)
                {
                    var comp = ColorTheory.Complementary(colors[i]);
                    EditorGUILayout.LabelField($"Color {i} complement:");
                    var previewRect = EditorGUILayout.GetControlRect(GUILayout.Height(20));
                    EditorGUI.DrawRect(new Rect(previewRect.x, previewRect.y, previewRect.width / 2, previewRect.height), colors[i]);
                    EditorGUI.DrawRect(new Rect(previewRect.x + previewRect.width / 2, previewRect.y, previewRect.width / 2, previewRect.height), comp);
                }
                break;
            case 1:
                EditorGUILayout.LabelField("Triadic Colors");
                for (var i = 0; i < colors.Length; i++)
                {
                    var triadic = ColorTheory.Triadic(colors[i]);
                    EditorGUILayout.LabelField($"Color {i} triadic:");
                    DrawSwatchPreview(triadic, 20f);
                }
                break;
            case 2:
                EditorGUILayout.LabelField("Analogous Colors");
                for (var i = 0; i < colors.Length; i++)
                {
                    var analogous = ColorTheory.Analogous(colors[i], 5, 30f);
                    EditorGUILayout.LabelField($"Color {i} analogous:");
                    DrawSwatchPreview(analogous, 20f);
                }
                break;
            case 3:
                EditorGUILayout.LabelField("WCAG Contrast Ratios (foreground on white bg)");
                for (var i = 0; i < colors.Length; i++)
                {
                    var ratio = GetContrastRatio(Color.white, colors[i]);
                    var rating = ratio >= 4.5f ? "✓ Pass" : "✗ Fail";
                    EditorGUILayout.LabelField($"Color {i}: {ratio:F2}:1 [{rating}]");
                }
                break;
        }
    }

    void DrawPaletteOverview(Color[] colors)
    {
        if (colors == null || colors.Length == 0)
        {
            EditorGUILayout.HelpBox("Palette is empty.", MessageType.Info);
            return;
        }

        EditorGUILayout.LabelField($"Colors: {colors.Length}");
        for (var i = 0; i < colors.Length; i++)
        {
            Color.RGBToHSV(colors[i], out var h, out var s, out var v);
            EditorGUILayout.LabelField($"Color {i}: H={h * 360f:F1}° S={s * 100f:F1}% V={v * 100f:F1}%");
        }
    }

    Color[] SimulateColorBlindness(Color[] colors, int mode)
    {
        if (colors == null) return colors;
        var result = new Color[colors.Length];
        for (var i = 0; i < colors.Length; i++)
            result[i] = SimulateColorBlindnessForColor(colors[i], mode);
        return result;
    }

    Color SimulateColorBlindnessForColor(Color c, int mode)
    {
        var r = c.r;
        var g = c.g;
        var b = c.b;

        return mode switch
        {
            1 => new Color(0.625f * r + 0.375f * g, 0.7f * r + 0.3f * g, b),
            2 => new Color(0.567f * r + 0.433f * g, 0.558f * r + 0.442f * g, b),
            3 => new Color(r, 0.95f * g + 0.05f * b, 0.433f * g + 0.567f * b),
            _ => c
        };
    }

    float GetContrastRatio(Color a, Color b)
    {
        var l1 = GetRelativeLuminance(a);
        var l2 = GetRelativeLuminance(b);
        var lighter = Mathf.Max(l1, l2);
        var darker = Mathf.Min(l1, l2);
        return (lighter + 0.05f) / (darker + 0.05f);
    }

    float GetRelativeLuminance(Color c)
    {
        var r = Linearize(c.r);
        var g = Linearize(c.g);
        var b = Linearize(c.b);
        return 0.2126f * r + 0.7152f * g + 0.0722f * b;
    }

    float Linearize(float c) => (c <= 0.03928f) ? c / 12.92f : Mathf.Pow((c + 0.055f) / 1.055f, 2.4f);

    void EnsureSelectedPalette()
    {
        if (_selectedPalette != null) return;
        var palettes = PaletteDatabase.Palettes;
        if (palettes.Count > 0)
            SelectPalette(palettes[0]);
    }

    void SelectPalette(ColorPalette palette)
    {
        _selectedPalette = palette;
        _selectedPaletteColorIndex = -1;
        if (_selectedPalette != null && _selectedPalette.Count > 0)
        {
            _selectedColor = _selectedPalette.colors[0];
            UpdateHSV();
        }
    }

    void RefreshDiscoveredTags()
    {
        _discoveredTags.Clear();
        foreach (var pal in PaletteDatabase.Palettes)
        {
            if (pal.tags == null) continue;
            foreach (var tag in pal.tags)
                _discoveredTags.Add(tag);
        }
    }

    List<ColorPalette> FilterPalettesByTag(IReadOnlyList<ColorPalette> palettes, string tag)
    {
        var result = new List<ColorPalette>();
        if (string.IsNullOrEmpty(tag))
        {
            result.AddRange(palettes);
            return result;
        }

        for (var i = 0; i < palettes.Count; i++)
        {
            if (palettes[i].HasTag(tag))
                result.Add(palettes[i]);
        }

        return result;
    }

    string GetPaletteDisplayName(ColorPalette palette)
    {
        if (palette == null) return "(None)";
        return string.IsNullOrEmpty(palette.paletteName) ? palette.name : palette.paletteName;
    }

    PaletteScheme GetActiveScheme() => _selectedPalette != null ? _selectedPalette.Scheme : _schemePreview;

    void SetActiveScheme(PaletteScheme scheme)
    {
        if (_selectedPalette != null)
        {
            if (_selectedPalette.Scheme == scheme) return;
            Undo.RecordObject(_selectedPalette, "Change Scheme");
            _selectedPalette.Scheme = scheme;
            if (_selectedPalette.GenerationMode == PaletteGenerationMode.Generated)
                RegeneratePalette(_selectedPalette, recordUndo: false);
            EditorUtility.SetDirty(_selectedPalette);
        }
        else
        {
            _schemePreview = scheme;
        }

        Repaint();
    }

    void NotifyColorChanged() => _onColorChanged?.Invoke(_selectedColor);
}
#endif