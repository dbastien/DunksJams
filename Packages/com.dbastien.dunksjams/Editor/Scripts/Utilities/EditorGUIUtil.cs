#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class EditorGUIUtil
{
    public static Event curEvent => Event.current;
    public static Rect lastRect => GUILayoutUtility.GetLastRect();
    public static bool isDarkTheme => EditorGUIUtility.isProSkin;

    // Event extension properties
    public static bool isRepaint(this Event e) => e is { type: EventType.Repaint };
    public static bool isLayout(this Event e) => e is { type: EventType.Layout };
    public static bool isKeyDown(this Event e) => e is { type: EventType.KeyDown };
    public static bool isMouseDown(this Event e) => e is { type: EventType.MouseDown };
    public static bool isMouseUp(this Event e) => e is { type: EventType.MouseUp };
    public static bool isMouseLeaveWindow(this Event e) => e is { type: EventType.MouseLeaveWindow };
    public static bool isIgnore(this Event e) => e is { type: EventType.Ignore };
    public static bool holdingAlt(this Event e) => e is { alt: true };
    public static bool holdingCmdOrCtrl(this Event e) => e != null && (e.command || e.control);

    public static Vector2 mousePosition_screenSpace
        (this Event e) => e != null ? GUIUtility.GUIToScreenPoint(e.mousePosition) : Vector2.zero;

    public static float GetLabelWidth(this string s) => GUI.skin.label.CalcSize(new GUIContent(s)).x;

    public static float GetLabelWidth(this string s, int fontSize)
    {
        SetLabelFontSize(fontSize);
        float r = s.GetLabelWidth();
        ResetLabelStyle();
        return r;
    }

    public static float GetLabelWidth(this string s, bool isBold)
    {
        if (isBold) SetLabelBold();
        float r = s.GetLabelWidth();
        if (isBold) ResetLabelStyle();
        return r;
    }

    public static float GetLabelWidth(this string s, int fontSize, bool isBold)
    {
        if (isBold) SetLabelBold();
        SetLabelFontSize(fontSize);
        float r = s.GetLabelWidth();
        ResetLabelStyle();
        return r;
    }

    public static void SetGUIEnabled(bool enabled)
    {
        _prevGuiEnabled = GUI.enabled;
        GUI.enabled = enabled;
    }

    public static void ResetGUIEnabled() => GUI.enabled = _prevGuiEnabled;
    private static bool _prevGuiEnabled = true;

    public static void SetLabelFontSize(int size) => GUI.skin.label.fontSize = size;
    public static void SetLabelBold() => GUI.skin.label.fontStyle = FontStyle.Bold;
    public static void SetLabelAlignmentCenter() => GUI.skin.label.alignment = TextAnchor.MiddleCenter;

    public static void ResetLabelStyle()
    {
        GUI.skin.label.fontSize = 0;
        GUI.skin.label.fontStyle = FontStyle.Normal;
        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
        GUI.skin.label.wordWrap = false;
    }

    public static void SetGUIColor(Color c)
    {
        _guiColorStack.Push(GUI.color);
        GUI.color *= c;
    }

    public static void ResetGUIColor() { GUI.color = _guiColorStack.Pop(); }

    private static Stack<Color> _guiColorStack = new();

    public static float editorDeltaTime = .0166f;

    private static void EditorDeltaTime_Update()
    {
        editorDeltaTime = (float)(EditorApplication.timeSinceStartup - _lastUpdateTime);
        _lastUpdateTime = EditorApplication.timeSinceStartup;
    }

    private static double _lastUpdateTime;

    [InitializeOnLoadMethod]
    private static void EditorDeltaTime_Subscribe()
    {
        EditorApplication.update -= EditorDeltaTime_Update;
        EditorApplication.update += EditorDeltaTime_Update;
    }
}
#endif