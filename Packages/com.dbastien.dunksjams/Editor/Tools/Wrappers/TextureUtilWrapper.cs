using System;
using System.Reflection;
using UnityEngine;

public static class TextureUtilWrapper
{
    private static readonly Type type = Type.GetType("UnityEditor.TextureUtil, UnityEditor.dll");

    private static MethodInfo GetMethod(string name) =>
        type?.GetMethod(name, BindingFlags.Static | BindingFlags.Public);

    private static readonly MethodInfo GetStorageMemorySizeLongMethod = GetMethod("GetStorageMemorySizeLong");
    private static readonly MethodInfo GetRuntimeMemorySizeLongMethod = GetMethod("GetRuntimeMemorySizeLong");
    private static readonly MethodInfo GetStorageMemorySizeMethod = GetMethod("GetStorageMemorySize");
    private static readonly MethodInfo GetRuntimeMemorySizeMethod = GetMethod("GetRuntimeMemorySize");
    private static readonly MethodInfo GetTextureFormatStringMethod = GetMethod("GetTextureFormatString");

    public static long GetStorageMemorySizeLong(Texture t) => GetStorageMemorySizeLongMethod != null
        ? (long)GetStorageMemorySizeLongMethod.Invoke(null, new object[] { t })
        : 0;

    public static long GetRuntimeMemorySizeLong(Texture t) => GetRuntimeMemorySizeLongMethod != null
        ? (long)GetRuntimeMemorySizeLongMethod.Invoke(null, new object[] { t })
        : 0;

    public static int GetStorageMemorySize(Texture t) => GetStorageMemorySizeMethod != null
        ? (int)GetStorageMemorySizeMethod.Invoke(null, new object[] { t })
        : 0;

    public static int GetRuntimeMemorySize(Texture t) => GetRuntimeMemorySizeMethod != null
        ? (int)GetRuntimeMemorySizeMethod.Invoke(null, new object[] { t })
        : 0;

    public static string GetTextureFormatString(TextureFormat t) => GetTextureFormatStringMethod != null
        ? (string)GetTextureFormatStringMethod.Invoke(null, new object[] { t })
        : t.ToString();
}