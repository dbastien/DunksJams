#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;

public static class EditorPrefsCached
{
    private static Dictionary<string, int> ints_byKey = new();
    private static Dictionary<string, bool> bools_byKey = new();
    private static Dictionary<string, float> floats_byKey = new();
    private static Dictionary<string, string> strings_byKey = new();

    public static int GetInt(string key, int defaultValue = 0)
    {
        if (ints_byKey.ContainsKey(key))
            return ints_byKey[key];
        return ints_byKey[key] = EditorPrefs.GetInt(key, defaultValue);
    }

    public static bool GetBool(string key, bool defaultValue = false)
    {
        if (bools_byKey.ContainsKey(key))
            return bools_byKey[key];
        return bools_byKey[key] = EditorPrefs.GetBool(key, defaultValue);
    }

    public static float GetFloat(string key, float defaultValue = 0)
    {
        if (floats_byKey.ContainsKey(key))
            return floats_byKey[key];
        return floats_byKey[key] = EditorPrefs.GetFloat(key, defaultValue);
    }

    public static string GetString(string key, string defaultValue = "")
    {
        if (strings_byKey.ContainsKey(key))
            return strings_byKey[key];
        return strings_byKey[key] = EditorPrefs.GetString(key, defaultValue);
    }

    public static void SetInt(string key, int value)
    {
        ints_byKey[key] = value;
        EditorPrefs.SetInt(key, value);
    }

    public static void SetBool(string key, bool value)
    {
        bools_byKey[key] = value;
        EditorPrefs.SetBool(key, value);
    }

    public static void SetFloat(string key, float value)
    {
        floats_byKey[key] = value;
        EditorPrefs.SetFloat(key, value);
    }

    public static void SetString(string key, string value)
    {
        strings_byKey[key] = value;
        EditorPrefs.SetString(key, value);
    }
}
#endif