using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class IOUtils
{
    private static readonly string _projectRoot = Application.dataPath[..^7];
    public static string ProjectRootFolder => _projectRoot;

    public static string ToRelativePath(string path)
    {
        if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
        if (!path.StartsWith(_projectRoot)) return path;
        return path[(_projectRoot.Length + 1)..].Replace("\\", "/");
    }

    public static string ToAbsolutePath(string path)
    {
        if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
        return Path.Combine(_projectRoot, path.Replace("/", "\\"));
    }

    public static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
        return Path.GetFullPath(path).Replace("\\", "/").TrimEnd('/');
    }

    public static string GetConsoleClickableLink(string path, int lineNumber = 0)
    {
        if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
        string relativePath = ToRelativePath(path);
        return $"{relativePath}:{lineNumber}";
    }

    public static List<string> GetFiles(string dir, bool recurse = false, Func<string, bool> filter = null)
    {
        if (string.IsNullOrEmpty(dir)) throw new ArgumentNullException(nameof(dir));
        if (!Directory.Exists(dir)) throw new DirectoryNotFoundException($"Directory not found: {dir}");

        List<string> files = new();
        if (recurse)
            GetFilesRecursively(dir, files, filter);
        else
            foreach (string file in Directory.GetFiles(dir))
                if (filter == null || filter(file))
                    files.Add(file);

        return files;
    }

    private static void GetFilesRecursively(string dir, List<string> files, Func<string, bool> filter)
    {
        foreach (string file in Directory.GetFiles(dir))
            if (filter == null || filter(file))
                files.Add(file);

        foreach (string subDir in Directory.GetDirectories(dir)) GetFilesRecursively(subDir, files, filter);
    }

    public static List<string> GetDirectories(string dir, bool recurse = false, Func<string, bool> filter = null)
    {
        if (string.IsNullOrEmpty(dir)) throw new ArgumentNullException(nameof(dir));
        if (!Directory.Exists(dir)) throw new DirectoryNotFoundException($"Directory not found: {dir}");

        List<string> directories = new();
        if (recurse)
            GetDirectoriesRecursively(dir, directories, filter);
        else
            foreach (string subDir in Directory.GetDirectories(dir))
                if (filter == null || filter(subDir))
                    directories.Add(subDir);

        return directories;
    }

    private static void GetDirectoriesRecursively(string dir, List<string> dirs, Func<string, bool> filter)
    {
        foreach (string subDir in Directory.GetDirectories(dir))
        {
            if (filter == null || filter(subDir)) dirs.Add(subDir);
            GetDirectoriesRecursively(subDir, dirs, filter);
        }
    }

    public static bool IsPathValid(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        foreach (char c in Path.GetInvalidPathChars())
            if (path.Contains(c))
                return false;

        return true;
    }

    public static bool IsWritable(string fullPath)
    {
        //todo: do also need to check locked?
        try
        {
            if (!File.Exists(fullPath)) return false;
            return (File.GetAttributes(fullPath) & FileAttributes.ReadOnly) == 0;
        }
        catch { return false; }
    }

    //todo: ugh on the error handling
    public static string ReadAllTextSafe(string fullPath, out string error)
    {
        error = null;
        try { return File.ReadAllText(fullPath); }
        catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
        {
            error = e.Message;
            return string.Empty;
        }
    }

    public static bool IsDirectoryEmpty(string dir)
    {
        if (string.IsNullOrEmpty(dir)) throw new ArgumentNullException(nameof(dir));
        if (!Directory.Exists(dir)) throw new DirectoryNotFoundException($"Directory not found: {dir}");
        using IEnumerator<string> enumerator = Directory.EnumerateFileSystemEntries(dir).GetEnumerator();
        return !enumerator.MoveNext();
    }

    public static void CreateDirectoryIfNotExists(string dir)
    {
        if (string.IsNullOrEmpty(dir)) throw new ArgumentNullException(nameof(dir));
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
    }

    public static void DeleteDirectory(string dir, bool recursive = true)
    {
        if (string.IsNullOrEmpty(dir)) throw new ArgumentNullException(nameof(dir));
        if (Directory.Exists(dir)) Directory.Delete(dir, recursive);
    }

    public static void DeleteFileIfExists(string file)
    {
        if (string.IsNullOrEmpty(file)) throw new ArgumentNullException(nameof(file));
        if (File.Exists(file)) File.Delete(file);
    }

    public static string GetUniqueFileName(string dir, string baseName, string extension = "")
    {
        if (string.IsNullOrEmpty(dir)) throw new ArgumentNullException(nameof(dir));
        if (string.IsNullOrEmpty(baseName)) throw new ArgumentNullException(nameof(baseName));
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var index = 1;
        string fileName;
        do
        {
            fileName = Path.Combine(dir, $"{baseName}{(index > 1 ? $"_{index}" : "")}{extension}");
            ++index;
        }
        while (File.Exists(fileName));

        return fileName;
    }
}