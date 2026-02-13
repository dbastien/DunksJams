using System;
using System.Reflection;
using UnityEngine;

public static class ColorPresetLibraryWrapper
{
    static readonly Type type = Type.GetType("UnityEditor.ColorPresetLibrary, UnityEditor");

    static readonly MethodInfo AddMethod = type.GetMethod("Add");
    static readonly MethodInfo CountMethod = type.GetMethod("Count");
    static readonly MethodInfo DrawMethod = type.GetMethod("Draw", new[] { typeof(Rect), typeof(int) });
    static readonly MethodInfo GetPresetMethod = type.GetMethod("GetPreset");
    static readonly MethodInfo RemoveMethod = type.GetMethod("Remove");
    static readonly MethodInfo ReplaceMethod = type.GetMethod("Replace");

    public static ScriptableObject CreateLibrary() => ScriptableObject.CreateInstance(type);

    public static void Add(ScriptableObject library, Color color, string name) =>
        AddMethod.Invoke(library, new object[] { color, name });

    public static int Count(ScriptableObject library) => (int)CountMethod.Invoke(library, new object[] { });

    public static void Draw(ScriptableObject library, Rect rect, int index) =>
        DrawMethod.Invoke(library, new object[] { rect, index });

    public static Color GetPreset(ScriptableObject library, int index) =>
        (Color)GetPresetMethod.Invoke(library, new object[] { index });

    public static void Remove(ScriptableObject library, int index) =>
        RemoveMethod.Invoke(library, new object[] { index });

    public static void Replace(ScriptableObject library, int index, Color newObject) =>
        ReplaceMethod.Invoke(library, new object[] { index, newObject });
}