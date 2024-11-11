using System;
using UnityEngine;

public struct GUIScope : IDisposable
{
    readonly Color _prevColor, _prevBgColor, _prevContentColor;
    readonly bool _prevEnabled;
    readonly int _prevFontSize;

    public GUIScope(Color? color = null, Color? backgroundColor = null, Color? contentColor = null, bool? enabled = null, int? fontSize = null)
    {
        _prevColor = GUI.color;
        _prevBgColor = GUI.backgroundColor;
        _prevContentColor = GUI.contentColor;
        _prevEnabled = GUI.enabled;
        _prevFontSize = GUI.skin.label.fontSize;

        if (color.HasValue) GUI.color = color.Value;
        if (backgroundColor.HasValue) GUI.backgroundColor = backgroundColor.Value;
        if (contentColor.HasValue) GUI.contentColor = contentColor.Value;
        if (enabled.HasValue) GUI.enabled = enabled.Value;
        if (fontSize.HasValue) GUI.skin.label.fontSize = fontSize.Value;
    }

    public void Dispose()
    {
        GUI.color = _prevColor;
        GUI.backgroundColor = _prevBgColor;
        GUI.contentColor = _prevContentColor;
        GUI.enabled = _prevEnabled;
        GUI.skin.label.fontSize = _prevFontSize;
    }
}

public struct GUIColorScope : IDisposable
{
    readonly Color _prev;
    public GUIColorScope(Color color)
    {
        _prev = GUI.color;
        if (_prev != color) GUI.color = color;
    }
    public void Dispose() => GUI.color = _prev;
}

public struct GUIFontSizeScope : IDisposable
{
    readonly int _prev;
    public GUIFontSizeScope(int size)
    {
        _prev = GUI.skin.label.fontSize;
        GUI.skin.label.fontSize = size;
    }
    public void Dispose() => GUI.skin.label.fontSize = _prev;
}

public struct GUIEnabledScope : IDisposable
{
    readonly bool _prev;
    public GUIEnabledScope(bool enabled)
    {
        _prev = GUI.enabled;
        GUI.enabled = enabled;
    }
    public void Dispose() => GUI.enabled = _prev;
}

public struct GUIBackgroundColorScope : IDisposable
{
    readonly Color _prev;
    public GUIBackgroundColorScope(Color color)
    {
        _prev = GUI.backgroundColor;
        if (_prev != color) GUI.backgroundColor = color;
    }
    public void Dispose() => GUI.backgroundColor = _prev;
}

public struct GUIContentColorScope : IDisposable
{
    readonly Color _prev;
    public GUIContentColorScope(Color color)
    {
        _prev = GUI.contentColor;
        if (_prev != color) GUI.contentColor = color;
    }
    public void Dispose() => GUI.contentColor = _prev;
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