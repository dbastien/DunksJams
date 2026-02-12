using UnityEditor;
using UnityEngine;

[ToolsetProvider(displayName = "Quick Search Toolset", description = "Opens Quick Search (Alt+').")]
public class QuickSearchToolset : IToolset
{
    GUIContent _quickSearchContent;

    public void Setup() =>
        _quickSearchContent = EditorGUIUtils.IconContentSafe("d_Search Icon", "Search Icon", "Open Quick Search menu");

    public void Teardown() => _ = 0;

    public void Draw()
    {
        if (GUILayout.Button(_quickSearchContent, ToolbarStyles.ToolbarButtonStyle))
            EditorApplication.ExecuteMenuItem("Edit/Search All...");
    }
}
