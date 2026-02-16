#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;

public static class PaletteDatabase
{
    static List<ColorPalette> _palettes;

    public static IReadOnlyList<ColorPalette> Palettes
    {
        get
        {
            if (_palettes == null) Refresh();
            return _palettes;
        }
    }

    public static void Refresh()
    {
        _palettes = new List<ColorPalette>();
        var guids = AssetDatabase.FindAssets("t:ColorPalette");
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var pal = AssetDatabase.LoadAssetAtPath<ColorPalette>(path);
            if (pal != null) _palettes.Add(pal);
        }
    }
}
#endif