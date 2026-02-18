#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class AssetDatabaseExtensions
{
    public static AssetImporter GetImporter(this Object t) => AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(t));

    public static string ToPath(this string guid) => AssetDatabase.GUIDToAssetPath(guid);
    public static List<string> ToPaths(this IEnumerable<string> guids) => guids.Select(r => r.ToPath()).ToList();

    public static string ToGuid(this string pathInProject) => AssetDatabase.AssetPathToGUID(pathInProject);

    public static List<string> ToGuids
        (this IEnumerable<string> pathsInProject) => pathsInProject.Select(r => r.ToGuid()).ToList();

    public static string GetPath(this Object o) => AssetDatabase.GetAssetPath(o);
    public static string GetGuid(this Object o) => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(o));

    public static string GetScriptPath
        (string scriptName) => AssetDatabase.FindAssets("t: script " + scriptName, null).FirstOrDefault()?.ToPath() ??
                               "script not found";

    public static bool IsValidGuid
        (this string guid) =>
        AssetDatabase.AssetPathToGUID(AssetDatabase.GUIDToAssetPath(guid), AssetPathToGUIDOptions.OnlyExistingAssets) !=
        "";

    public static T Reimport<T>(this T t) where T : Object
    {
        AssetDatabase.ImportAsset(t.GetPath(), ImportAssetOptions.ForceUpdate);
        return t;
    }

    public static Object LoadGuid(this string guid) => AssetDatabase.LoadAssetAtPath(guid.ToPath(), typeof(Object));
    public static T LoadGuid<T>(this string guid) where T : Object => AssetDatabase.LoadAssetAtPath<T>(guid.ToPath());
}
#endif