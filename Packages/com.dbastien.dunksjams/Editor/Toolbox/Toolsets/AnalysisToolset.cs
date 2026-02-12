using UnityEditor;
using UnityEngine;

[ToolsetProvider(displayName = "Analysis Toolset", description = "Quick access to various profiling and debugging tools.")]
public class AnalysisToolset : IToolset
{
    GUIContent profilerContent;
    GUIContent frameDebuggerContent;
    GUIContent physicsDebuggerContent;
    GUIContent imguiDebuggerContent;

    public void Setup()
    {
        profilerContent = EditorGUIUtils.IconContentSafe("d_UnityEditor.ProfilerWindow", "UnityEditor.ProfilerWindow", "Profiler");
        frameDebuggerContent = EditorGUIUtils.IconContentSafe("animationdopesheetkeyframe", "Animation.AddKeyframe", "Frame Debugger");
        physicsDebuggerContent = EditorGUIUtils.IconContentSafe("d_Profiler.Physics", "Profiler.Physics", "Physics Debugger");
        imguiDebuggerContent = EditorGUIUtils.IconContentSafe("d_UnityEditor.ConsoleWindow", "UnityEditor.ConsoleWindow", "IMGUI Debugger");
    }

    public void Teardown() => _ = 0;

    public void Draw()
    {
        if (GUILayout.Button(profilerContent, ToolbarStyles.ToolbarButtonStyle))
            EditorApplication.ExecuteMenuItem("‽/Analysis/Profiler");
        if (GUILayout.Button(frameDebuggerContent, ToolbarStyles.ToolbarButtonStyle))
            EditorApplication.ExecuteMenuItem("‽/Analysis/Frame Debugger");
        if (GUILayout.Button(physicsDebuggerContent, ToolbarStyles.ToolbarButtonStyle))
            EditorApplication.ExecuteMenuItem("‽/Analysis/Physics Debugger");
        if (GUILayout.Button(imguiDebuggerContent, ToolbarStyles.ToolbarButtonStyle))
            EditorApplication.ExecuteMenuItem("‽/Analysis/IMGUI Debugger");
    }
}
