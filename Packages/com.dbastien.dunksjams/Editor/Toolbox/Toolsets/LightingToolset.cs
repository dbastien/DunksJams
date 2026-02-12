using UnityEditor;
using UnityEngine;

[ToolsetProvider(displayName = "Lighting Toolset", description = "Access Lighting and Light Explorer windows.")]
public class LightingToolset : IToolset
{
    GUIContent _lightingContent;
    GUIContent _lightExplorerContent;

    public void Setup()
    {
        _lightingContent = EditorGUIUtils.IconContentSafe("d_SceneViewLighting", "SceneviewLighting", "Lighting Settings");
        _lightExplorerContent = EditorGUIUtils.IconContentSafe("LightExplorer", "PreMatLight0", "Light Explorer");
    }

    public void Teardown() => _ = 0;

    public void Draw()
    {
        if (GUILayout.Button(_lightingContent, ToolbarStyles.ToolbarButtonStyle))
            EditorApplication.ExecuteMenuItem("‽/Rendering/Lighting");
        if (GUILayout.Button(_lightExplorerContent, ToolbarStyles.ToolbarButtonStyle))
            EditorApplication.ExecuteMenuItem("‽/Rendering/Light Explorer");
    }
}
