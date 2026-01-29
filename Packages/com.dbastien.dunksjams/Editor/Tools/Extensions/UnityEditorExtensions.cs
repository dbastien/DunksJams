using UnityEngine;
using UnityEditor;

public static class UnityEditorExtensions
{
    public static void Ping(this Object obj) => EditorGUIUtility.PingObject(obj);
    
    public static object GetDockArea(this EditorWindow win) =>
        typeof(EditorWindow).GetField("m_Parent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.GetValue(win);
    
    public static T FindOrCreateAsset<T>(this string path) where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset) return asset;
        asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        return asset;
    }
    
    public static void LogChildPropertyNames(this SerializedObject so) => so.GetIterator().LogPropertyHierarchy();

    public static void LogPropertyHierarchy(this SerializedProperty sp)
    {
        var rootLen = sp.propertyPath.Length;
        var propCopy = sp.Copy();
        var depth = 0;

        while (propCopy.Next(true))
        {
            var newDepth = propCopy.depth;
            if (newDepth != depth)
            {
                DLog.Log($"Level {newDepth}:");
                depth = newDepth;
            }

            var childPath = propCopy.propertyPath[rootLen..];
            if (!string.IsNullOrWhiteSpace(childPath))
                DLog.Log($"  {childPath}");
        }
    }
    
    public static Color GetTypeColor(this Object obj) =>
        obj switch
        {
            AnimationClip => new(1f, 0.5f, 0.5f),
            AudioClip => new(1f, 0.7f, 0.4f),
            Font => new(0.8f, 0.8f, 1f),
            GameObject => new(0.3f, 0.7f, 1f),
            Material => new(0.6f, 1f, 0.6f),
            MonoScript => new(1f, 1f, 0.6f),
            ScriptableObject => new(1f, 0.92f, 0.6f),
            Shader => new(0.6f, 0.8f, 1f),
            Texture => new(1f, 0.5f, 1f),
            _ => GUI.color
        };
}