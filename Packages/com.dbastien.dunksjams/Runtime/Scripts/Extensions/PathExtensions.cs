using System.IO;
using UnityEngine;

public static class PathExtensions
{
    public static bool HasParentPath(this string path) => path.LastIndexOf('/') > 0;

    public static string GetParentPath
        (this string path) => path.HasParentPath() ? path[..path.LastIndexOf('/')] : "";

    public static string GetFilename
        (this string path, bool withExtension = false) =>
        withExtension ? Path.GetFileName(path) : Path.GetFileNameWithoutExtension(path);

    public static string GetExtension(this string path) => Path.GetExtension(path);

    public static string ToGlobalPath
        (this string localPath) => Application.dataPath + "/" + localPath[..^1];

    public static string ToLocalPath(this string globalPath) => "Assets" + globalPath.Replace(Application.dataPath, "");
    public static string CombinePath(this string p, string p2) => Path.Combine(p, p2);
    public static bool IsSubpathOf(this string path, string of) => path.StartsWith(of + "/") || of == "";

    public static string GetDirectory(this string pathOrDirectory)
    {
        string directory = pathOrDirectory.Contains('.')
            ? pathOrDirectory[..pathOrDirectory.LastIndexOf('/')]
            : pathOrDirectory;
        if (directory.Contains('.'))
            directory = directory[..directory.LastIndexOf('/')];
        return directory;
    }

    public static bool DirectoryExists(this string pathOrDirectory) => Directory.Exists(pathOrDirectory.GetDirectory());

    public static string EnsureDirExists(this string pathOrDirectory)
    {
        string directory = pathOrDirectory.GetDirectory();
        if (directory.HasParentPath() && !Directory.Exists(directory.GetParentPath()))
            EnsureDirExists(directory.GetParentPath());
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        return pathOrDirectory;
    }

    public static string ClearDir(this string dir)
    {
        if (!Directory.Exists(dir)) return dir;
        var diri = new DirectoryInfo(dir);
        foreach (FileInfo r in diri.EnumerateFiles()) r.Delete();
        foreach (DirectoryInfo r in diri.EnumerateDirectories()) r.Delete(true);
        return dir;
    }
}