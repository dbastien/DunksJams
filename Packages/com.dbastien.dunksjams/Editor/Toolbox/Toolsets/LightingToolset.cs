using UnityEditor;
using UnityEngine;

[ToolsetProvider(displayName = "Lighting Toolset", description = "Access Lighting and Light Explorer windows.")]
public class LightingToolset : IToolset
{
    const string TexturePath = "Packages/com.dbastien.dunksjams/Editor/Toolbox/Textures/";
    GUIContent lightingContent;
    GUIContent lightExplorerContent;

    public void Setup()
    {
        var lightIcon = EditorGUIUtility.Load(TexturePath + "TB_Icon_LightSettings.png") as Texture2D;
        var explorerIcon = EditorGUIUtility.Load(TexturePath + "TB_Icon_LightExplorer.png") as Texture2D;
        if (lightIcon != null) lightIcon.hideFlags = HideFlags.HideAndDontSave;
        if (explorerIcon != null) explorerIcon.hideFlags = HideFlags.HideAndDontSave;
        lightingContent = new GUIContent(lightIcon, "Lighting Settings");
        lightExplorerContent = new GUIContent(explorerIcon, "Light Explorer");
    }

    public void Teardown() { }

    public void Draw()
    {
        if (GUILayout.Button(lightingContent, ToolbarStyles.ToolbarButtonStyle))
            EditorApplication.ExecuteMenuItem("Window/Rendering/Lighting");
        if (GUILayout.Button(lightExplorerContent, ToolbarStyles.ToolbarButtonStyle))
            EditorApplication.ExecuteMenuItem("Window/Rendering/Light Explorer");
    }
}
