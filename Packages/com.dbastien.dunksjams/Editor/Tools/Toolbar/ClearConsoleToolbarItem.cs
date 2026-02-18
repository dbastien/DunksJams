using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class ClearConsoleToolbarItem : IToolbarItem
{
    private static GUIStyle _commandButtonStyle;

    public string Name => "Clear Console";
    public ToolbarItemPosition Position => ToolbarItemPosition.Left;
    public ToolbarItemAnchor Anchor => ToolbarItemAnchor.Left;
    public int Priority => -100;
    public bool Enabled => true;

    public void Init() { }

    public SettingsProvider GetSettingsProvider() => null;

    public void DrawInToolbar()
    {
        if (_commandButtonStyle == null)
            _commandButtonStyle = new GUIStyle("Command")
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                imagePosition = ImagePosition.ImageLeft,
                fixedHeight = 22
            };

        // UnityEngine.Debug.Log("[DEBUG_LOG] Drawing Clear Console Button");
        if (GUILayout.Button(new GUIContent("Clear Console", "Clears the Unity Console"), _commandButtonStyle,
                GUILayout.Width(100)))
        {
            var logEntries = Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
            MethodInfo clearMethod = logEntries?.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
            clearMethod?.Invoke(null, null);
        }
    }

    public void DrawInWindow()
    {
        if (GUILayout.Button("Clear Console"))
        {
            var logEntries = Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
            MethodInfo clearMethod = logEntries?.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
            clearMethod?.Invoke(null, null);
        }
    }
}