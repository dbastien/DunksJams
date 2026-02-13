using System;
using System.Reflection;
using UnityEngine;

public static class CurvePresetLibraryWrapper
{
    static readonly Type type = Type.GetType("UnityEditor.CurvePresetLibrary, UnityEditor");

    static readonly MethodInfo AddMethod = type.GetMethod("Add");
    static readonly MethodInfo CountMethod = type.GetMethod("Count");
    static readonly MethodInfo DrawMethod = type.GetMethod("Draw", new[] { typeof(Rect), typeof(int) });
    static readonly MethodInfo GetPresetMethod = type.GetMethod("GetPreset");
    static readonly MethodInfo GetNameMethod = type.GetMethod("GetName");
    static readonly MethodInfo RemoveMethod = type.GetMethod("Remove");
    static readonly MethodInfo ReplaceMethod = type.GetMethod("Replace");

    public static ScriptableObject CreateLibrary() => ScriptableObject.CreateInstance(type);

    public static void Add(ScriptableObject lib, AnimationCurve curve, string name) =>
        AddMethod.Invoke(lib, new object[] { curve, name });

    public static int Count(ScriptableObject lib) => (int)CountMethod.Invoke(lib, new object[] { });

    public static void Draw(ScriptableObject lib, Rect rect, int index) =>
        DrawMethod.Invoke(lib, new object[] { rect, index });

    public static AnimationCurve GetPreset(ScriptableObject lib, int index) =>
        (AnimationCurve)GetPresetMethod.Invoke(lib, new object[] { index });

    public static string GetName(ScriptableObject lib, int index) =>
        (string)GetNameMethod.Invoke(lib, new object[] { index });

    public static void Remove(ScriptableObject lib, int index) => RemoveMethod.Invoke(lib, new object[] { index });

    public static void Replace(ScriptableObject lib, int index, AnimationCurve newObject) =>
        ReplaceMethod.Invoke(lib, new object[] { index, newObject });
}