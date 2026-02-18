using System;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ToolbarExtension
{
    public static event Action<double> OnUpdate;
    public static event Action OnLeftToolbarGUI;
    public static event Action OnRightToolbarGUI;

    private static double _lastUpdateTime;

    static ToolbarExtension()
    {
        ToolbarCallback.OnLeftToolbarGUI -= HandleLeftGUI;
        ToolbarCallback.OnLeftToolbarGUI += HandleLeftGUI;
        ToolbarCallback.OnRightToolbarGUI -= HandleRightGUI;
        ToolbarCallback.OnRightToolbarGUI += HandleRightGUI;

        EditorApplication.update -= Update;
        EditorApplication.update += Update;
    }

    private static void Update()
    {
        double currentTime = EditorApplication.timeSinceStartup;
        double deltaTime = currentTime - _lastUpdateTime;
        _lastUpdateTime = currentTime;
        OnUpdate?.Invoke(deltaTime);
    }

    private static void HandleLeftGUI()
    {
        GUILayout.BeginHorizontal();
        OnLeftToolbarGUI?.Invoke();
        GUILayout.EndHorizontal();
    }

    private static void HandleRightGUI()
    {
        GUILayout.BeginHorizontal();
        OnRightToolbarGUI?.Invoke();
        GUILayout.EndHorizontal();
    }
}