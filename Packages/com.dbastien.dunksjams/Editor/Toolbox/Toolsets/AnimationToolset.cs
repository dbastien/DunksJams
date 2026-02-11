using UnityEditor;
using UnityEngine;

[ToolsetProvider(displayName = "Animation Toolset", description = "Quick access to Animation and Animator windows.")]
public class AnimationToolset : IToolset
{
    GUIContent animationContent;
    GUIContent animatorContent;

    public void Setup()
    {
        animationContent = EditorGUIUtility.TrIconContent("UnityEditor.AnimationWindow", "Animation window");
        animatorContent = EditorGUIUtility.TrIconContent("UnityEditor.Graphs.AnimatorControllerTool", "Animator window");
    }

    public void Teardown() { }

    public void Draw()
    {
        if (GUILayout.Button(animationContent, ToolbarStyles.ToolbarButtonStyle))
            EditorApplication.ExecuteMenuItem("‽/Animation/Animation");
        if (GUILayout.Button(animatorContent, ToolbarStyles.ToolbarButtonStyle))
            EditorApplication.ExecuteMenuItem("‽/Animation/Animator");
    }
}
