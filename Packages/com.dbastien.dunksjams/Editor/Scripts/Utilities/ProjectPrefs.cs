#if UNITY_EDITOR
using UnityEditor;

public static class ProjectPrefs
{
    public static int GetInt
        (string key, int defaultValue = 0) => EditorPrefsCached.GetInt(key + projectId, defaultValue);

    public static bool GetBool
        (string key, bool defaultValue = false) => EditorPrefsCached.GetBool(key + projectId, defaultValue);

    public static float GetFloat
        (string key, float defaultValue = 0) => EditorPrefsCached.GetFloat(key + projectId, defaultValue);

    public static string GetString
        (string key, string defaultValue = "") => EditorPrefsCached.GetString(key + projectId, defaultValue);

    public static void SetInt(string key, int value) => EditorPrefsCached.SetInt(key + projectId, value);
    public static void SetBool(string key, bool value) => EditorPrefsCached.SetBool(key + projectId, value);
    public static void SetFloat(string key, float value) => EditorPrefsCached.SetFloat(key + projectId, value);
    public static void SetString(string key, string value) => EditorPrefsCached.SetString(key + projectId, value);

    public static bool HasKey(string key) => EditorPrefs.HasKey(key + projectId);
    public static void DeleteKey(string key) => EditorPrefs.DeleteKey(key + projectId);

    public static int projectId => PlayerSettings.productGUID.GetHashCode();
}
#endif