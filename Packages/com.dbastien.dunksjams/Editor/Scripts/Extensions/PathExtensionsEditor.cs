#if UNITY_EDITOR
using UnityEditor;

public static class PathExtensionsEditor
{
    public static string EnsurePathIsUnique(this string path)
    {
        if (!path.DirectoryExists()) return path;
        string s = AssetDatabase.GenerateUniqueAssetPath(path);
        return s == "" ? path : s;
    }

    public static void EnsureDirExistsAndRevealInFinder(string dir)
    {
        dir.EnsureDirExists();
        EditorUtility.OpenWithDefaultApp(dir);
    }
}
#endif