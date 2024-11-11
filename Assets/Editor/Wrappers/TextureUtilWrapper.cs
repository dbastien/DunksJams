using System;
using System.Reflection;
using UnityEngine;

public static class TextureUtilWrapper
{
    static readonly Type type = Type.GetType("UnityEditor.TextureUtil, UnityEditor.dll");
    static MethodInfo GetMethod(string name) => type?.GetMethod(name, BindingFlags.Static | BindingFlags.Public);
    
    static readonly MethodInfo GetStorageMemorySizeLongMethod = GetMethod("GetStorageMemorySizeLong");
    static readonly MethodInfo GetRuntimeMemorySizeLongMethod = GetMethod("GetRuntimeMemorySizeLong");
    static readonly MethodInfo GetTextureFormatStringMethod = GetMethod("GetTextureFormatString");

    public static long GetStorageMemorySizeLong(Texture t) => (long)GetStorageMemorySizeLongMethod.Invoke(null, new object[] {t});
    public static long GetRuntimeMemorySizeLong(Texture t) => (long)GetRuntimeMemorySizeLongMethod.Invoke(null, new object[] {t});
    public static string GetStorageMemorySizeLong(TextureFormat t) => (string)GetTextureFormatStringMethod.Invoke(null, new object[] {t});
}