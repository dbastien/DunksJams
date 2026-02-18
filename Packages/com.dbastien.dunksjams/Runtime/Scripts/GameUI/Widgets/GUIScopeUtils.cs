using System;
using UnityEngine;

public struct GUIScope : IDisposable
{
    private readonly Color prevColor, prevBgColor, prevContentColor;
    private readonly bool prevEnabled;
    private readonly int prevFontSize;

    public GUIScope
    (
        Color? color = null, Color? backgroundColor = null, Color? contentColor = null,
        bool? enabled = null, int? fontSize = null
    )
    {
        prevColor = GUI.color;
        prevBgColor = GUI.backgroundColor;
        prevContentColor = GUI.contentColor;
        prevEnabled = GUI.enabled;
        prevFontSize = GUI.skin.label.fontSize;

        if (color.HasValue) GUI.color = color.Value;
        if (backgroundColor.HasValue) GUI.backgroundColor = backgroundColor.Value;
        if (contentColor.HasValue) GUI.contentColor = contentColor.Value;
        if (enabled.HasValue) GUI.enabled = enabled.Value;
        if (fontSize.HasValue) GUI.skin.label.fontSize = fontSize.Value;
    }

    public void Dispose()
    {
        GUI.color = prevColor;
        GUI.backgroundColor = prevBgColor;
        GUI.contentColor = prevContentColor;
        GUI.enabled = prevEnabled;
        GUI.skin.label.fontSize = prevFontSize;
    }
}

public struct GUIColorScope : IDisposable
{
    private readonly Color prev;

    public GUIColorScope(Color color)
    {
        prev = GUI.color;
        if (prev != color) GUI.color = color;
    }

    public void Dispose() => GUI.color = prev;
}

public struct GuiChangedScope : IDisposable
{
    private bool saved;

    public GuiChangedScope(bool? setChangedTo = null)
    {
        saved = GUI.changed;
        if (setChangedTo.HasValue) GUI.changed = setChangedTo.Value;
    }

    public void Dispose() => GUI.changed = saved;
}

public struct GUIFontSizeScope : IDisposable
{
    private readonly int prev;

    public GUIFontSizeScope(int size)
    {
        prev = GUI.skin.label.fontSize;
        GUI.skin.label.fontSize = size;
    }

    public void Dispose() => GUI.skin.label.fontSize = prev;
}

public struct GUIEnabledScope : IDisposable
{
    private readonly bool prev;

    public GUIEnabledScope(bool enabled)
    {
        prev = GUI.enabled;
        GUI.enabled = enabled;
    }

    public void Dispose() => GUI.enabled = prev;
}

public struct GUIBackgroundColorScope : IDisposable
{
    private readonly Color prev;

    public GUIBackgroundColorScope(Color color)
    {
        prev = GUI.backgroundColor;
        if (prev != color) GUI.backgroundColor = color;
    }

    public void Dispose() => GUI.backgroundColor = prev;
}

public struct GUIContentColorScope : IDisposable
{
    private readonly Color prev;

    public GUIContentColorScope(Color color)
    {
        prev = GUI.contentColor;
        if (prev != color) GUI.contentColor = color;
    }

    public void Dispose() => GUI.contentColor = prev;
}

public struct GUIHorizontalScope : IDisposable
{
    public GUIHorizontalScope(params GUILayoutOption[] options) => GUILayout.BeginHorizontal(options);
    public void Dispose() => GUILayout.EndHorizontal();
}

public struct GUIVerticalScope : IDisposable
{
    public GUIVerticalScope(params GUILayoutOption[] options) => GUILayout.BeginVertical(options);
    public void Dispose() => GUILayout.EndVertical();
}