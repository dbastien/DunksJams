using UnityEditor;
using UnityEngine;

[ToolsetProvider(displayName = "Quick Search Toolset", description = "Opens Quick Search (Alt+').")]
public class QuickSearchToolset : IToolset
{
    const string TexturePath = "Packages/com.dbastien.dunksjams/Editor/Toolbox/Textures/TB_Icon_Search.png";
    GUIContent quickSearchContent;

    public void Setup()
    {
        var icon = EditorGUIUtility.Load(TexturePath) as Texture2D;
        if (icon != null) icon.hideFlags = HideFlags.HideAndDontSave;
        quickSearchContent = new GUIContent(icon, "Open Quick Search menu");
    }

    public void Teardown() { }

    public void Draw()
    {
        if (GUILayout.Button(quickSearchContent, ToolbarStyles.ToolbarButtonStyle))
            EditorApplication.ExecuteMenuItem("Edit/Search All...");
    }
}
