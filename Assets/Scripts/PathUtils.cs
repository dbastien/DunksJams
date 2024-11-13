using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class PathUtils
{
    public static string ProjectRootFolder => Application.dataPath.Remove("/Assets");
    
    public static string GetConsoleClickableLink(string filePath, int lineNumber) =>
        $"{filePath.Replace("\\", "/").Remove(ProjectRootFolder + "/")}:{lineNumber}";
    
    public static List<string> GetFiles(string dir, bool recurse = false)
    {
        if (dir == null) throw new ArgumentNullException(nameof(dir));
        List<string> files = new();
        if (recurse) GetFilesRecursively(dir, files);
        else files.AddRange(Directory.GetFiles(dir));
        return files;
    }

    static void GetFilesRecursively(string dir, List<string> files)
    {
        foreach (string file in Directory.GetFiles(dir))
            files.Add(file);
        foreach (string d in Directory.GetDirectories(dir))
            GetFilesRecursively(d, files);
    }
}