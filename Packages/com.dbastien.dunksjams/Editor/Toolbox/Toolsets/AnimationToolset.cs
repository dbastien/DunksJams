using UnityEditor;
using UnityEngine;

[ToolsetProvider(displayName = "Animation Toolset", description = "Quick access to Animation and Animator windows.")]
public class AnimationToolset : IToolset
{
    GUIContent animationContent;
    GUIContent animatorContent;

    public void Setup()
    {
        animationContent = EditorGUIUtils.IconContentSafe("d_UnityEditor.AnimationWindow", "UnityEditor.AnimationWindow", "Animation window");
        animatorContent = EditorGUIUtils.IconContentSafe("d_UnityEditor.Graphs.AnimatorControllerTool", "UnityEditor.Graphs.AnimatorControllerTool", "Animator window");
    }

    public void Teardown() => _ = 0;

    public void Draw()
    {
        if (GUILayout.Button(animationContent, ToolbarStyles.ToolbarButtonStyle))
            EditorApplication.ExecuteMenuItem("‽/Animation/Animation");
        if (GUILayout.Button(animatorContent, ToolbarStyles.ToolbarButtonStyle))
            EditorApplication.ExecuteMenuItem("‽/Animation/Animator");
    }
}
