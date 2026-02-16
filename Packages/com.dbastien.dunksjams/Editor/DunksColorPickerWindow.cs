#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

public class DunksColorPickerWindow : EditorWindow
{
    [MenuItem("‽/Advanced Color Picker")]
    public static void ShowWindow() => PaletteStudioWindow.ShowPicker(null, Color.white);

    public static void ShowWindow(Action<Color> onColorChanged, Color initialColor) =>
        PaletteStudioWindow.ShowPicker(onColorChanged, initialColor);
}
#endif
