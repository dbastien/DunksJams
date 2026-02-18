using UnityEditor;
using UnityEngine;

/// <summary>Editor GUI utility methods. Use for shared icon loading and fallbacks.</summary>
public static class EditorGUIUtils
{
    /// <summary>Load icon content with optional fallback. Returns empty GUIContent if both fail.</summary>
    public static GUIContent IconContentSafe(string primary, string fallback = null)
    {
        GUIContent c = EditorGUIUtility.IconContent(primary);
        if (c?.image == null && !string.IsNullOrEmpty(fallback))
            c = EditorGUIUtility.IconContent(fallback);
        return c ?? new GUIContent();
    }

    /// <summary>Load the icon with a tooltip. Tries fallback icon name if primary fails.</summary>
    public static GUIContent IconContentSafe(string primary, string fallback, string tooltip)
    {
        GUIContent c = IconContentSafe(primary, fallback);
        return new GUIContent(c.image, tooltip);
    }
}