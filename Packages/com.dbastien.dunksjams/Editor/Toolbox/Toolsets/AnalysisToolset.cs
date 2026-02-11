using UnityEditor;
using UnityEngine;

[ToolsetProvider(displayName = "Analysis Toolset", description = "Quick access to Profiler, Frame Debugger, Physics Debugger, IMGUI Debugger.")]
public class AnalysisToolset : IToolset
{
    GUIContent profilerContent;
    GUIContent frameDebuggerContent;
    GUIContent physicsDebuggerContent;
    GUIContent imguiDebuggerContent;

    public void Setup()
    {
        profilerContent = EditorGUIUtility.TrIconContent("d_UnityEditor.ProfilerWindow", "Profiler");
        frameDebuggerContent = EditorGUIUtility.TrIconContent("animationdopesheetkeyframe", "Frame Debugger");
        physicsDebuggerContent = EditorGUIUtility.TrIconContent("d_Profiler.Physics", "Physics Debugger");
        imguiDebuggerContent = EditorGUIUtility.TrIconContent("UnityEditor.ConsoleWindow", "IMGUI Debugger");
    }

    public void Teardown() { }

    public void Draw()
    {
        if (GUILayout.Button(profilerContent, ToolbarStyles.ToolbarButtonStyle))
            EditorApplication.ExecuteMenuItem("Window/Analysis/Profiler");
        if (GUILayout.Button(frameDebuggerContent, ToolbarStyles.ToolbarButtonStyle))
            EditorApplication.ExecuteMenuItem("Window/Analysis/Frame Debugger");
        if (GUILayout.Button(physicsDebuggerContent, ToolbarStyles.ToolbarButtonStyle))
            EditorApplication.ExecuteMenuItem("Window/Analysis/Physics Debugger");
        if (GUILayout.Button(imguiDebuggerContent, ToolbarStyles.ToolbarButtonStyle))
            EditorApplication.ExecuteMenuItem("Window/Analysis/IMGUI Debugger");
    }
}
