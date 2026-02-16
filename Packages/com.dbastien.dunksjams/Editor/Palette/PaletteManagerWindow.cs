#if UNITY_EDITOR
using UnityEditor;

public class PaletteManagerWindow : EditorWindow
{
    [MenuItem("‽/Palette Studio")]
    public static void ShowWindow() => PaletteStudioWindow.ShowWindow();
}
#endif
