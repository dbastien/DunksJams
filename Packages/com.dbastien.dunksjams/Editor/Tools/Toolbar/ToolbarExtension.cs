using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ToolbarExtension
{
    static readonly int _toolCount;
    static GUIStyle _commandButtonStyle;

    public static event Action<double> OnUpdate;
    public static event Action OnLeftToolbarGUI;
    public static event Action OnRightToolbarGUI;

    static double _lastUpdateTime;

    static ToolbarExtension()
    {
        var toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
        var toolbarIcons = toolbarType.GetField("s_ShownToolIcons",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        _toolCount = toolbarIcons != null ? ((Array)toolbarIcons.GetValue(null)).Length : 7;

        ToolbarCallback.ToolbarGUI -= OnGUI;
        ToolbarCallback.ToolbarGUI += OnGUI;

        EditorApplication.update -= Update;
        EditorApplication.update += Update;
    }

    static void Update()
    {
        var currentTime = EditorApplication.timeSinceStartup;
        var deltaTime = currentTime - _lastUpdateTime;
        _lastUpdateTime = currentTime;

        OnUpdate?.Invoke(deltaTime);
    }

    static void OnGUI()
    {
        _commandButtonStyle ??= new GUIStyle("Command")
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter,
            imagePosition = ImagePosition.ImageLeft,
            fixedHeight = 22
        };

        var screenWidth = EditorGUIUtility.currentViewWidth;

        // Approximate positions of Unity's default controls
        var playButtonsPosition = screenWidth / 2 - 70;

        // Left side area (after tools, before play buttons)
        var leftRect = new Rect(0, 5, screenWidth, 24);
        leftRect.xMin += 32 * _toolCount; // Skip tools
        leftRect.xMin += 10; // Margin
        leftRect.xMin += 64 * 2; // Skip pivot/handle tools
        leftRect.xMax = playButtonsPosition - 10;

        if (leftRect.width > 10)
        {
            GUILayout.BeginArea(leftRect);
            GUILayout.BeginHorizontal();
            OnLeftToolbarGUI?.Invoke();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        // Right side area (after play buttons, before layout)
        Rect rightRect = new(0, 5, screenWidth, 24);
        rightRect.xMin = playButtonsPosition + 140; // After play/pause/step
        rightRect.xMax = screenWidth - 350; // Before layout/layers/account

        if (rightRect.width > 10)
        {
            GUILayout.BeginArea(rightRect);
            GUILayout.BeginHorizontal();
            OnRightToolbarGUI?.Invoke();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }

    static void DrawLeftToolbarItems()
    {
    }

    static void DrawRightToolbarItems()
    {
    }
}