using UnityEditor;
using UnityEngine;
using System;

public static class ShaderGUIUtils
{
    public static int IndentAmount = 1;

    //re-implements MaterialEditor internal
    public static Rect GetControlRectForSingleLine() =>
        EditorGUILayout.GetControlRect(true, 18f, EditorStyles.layerMaskField, Array.Empty<GUILayoutOption>());
    
    //re-implements EditorGUI internal
    public static void GetRectsForMiniThumbnailField(Rect rect, out Rect thumbRect, out Rect labelRect)
    {
        thumbRect = EditorGUI.IndentedRect(rect);
        thumbRect.y -= 1f;
        thumbRect.height = 18f;
        thumbRect.width = 32f;
        var num = thumbRect.x + 30f;
        labelRect = new(num, rect.y, thumbRect.x + EditorGUIUtility.labelWidth - num, rect.height);
    }

    public static void HeaderSection(string text, Action func)
    {
        BeginHeader(text);
        func();
        EndHeader();
    }

    public static void HeaderAutoSection(MaterialEditor editor, string text, MaterialProperty prop, Action func)
    {
        if (BeginHeaderAutoProperty(editor, text, prop)) func();
        EndHeader();
    }

    public static bool BeginHeaderAutoProperty(MaterialEditor editor, string text, MaterialProperty prop)
    {
        BeginHeaderProperty(editor, text, prop);
        return prop.floatValue > 0f;
    }

    public static void BeginHeaderProperty(MaterialEditor editor, string text, MaterialProperty prop)
    {
        editor.ShaderProperty(prop, GUIContent.none);
        var rect = GUILayoutUtility.GetLastRect();
        EditorGUI.indentLevel += IndentAmount;
        EditorGUI.LabelField(rect, text, EditorStyles.boldLabel);
    }

    public static void BeginHeader(string text)
    {
        EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
        EditorGUI.indentLevel += IndentAmount;
    }

    public static void EndHeader() => EditorGUI.indentLevel -= IndentAmount;

    public static void HeaderSeparator() => GUILayout.Space(12f);
}