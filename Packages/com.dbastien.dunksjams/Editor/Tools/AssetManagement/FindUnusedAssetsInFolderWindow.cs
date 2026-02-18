using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class FindUnusedAssetsInFolderWindow : EditorWindow
{
    private const string ProgressBarTitle = "Searching for References";

    public void OnGUI()
    {
        if (GUILayout.Button("Write Shader Usages")) FindAllShaderReferences();

        if (GUILayout.Button("Write Material Usages")) FindAllMaterialReferences();

        if (GUILayout.Button("Write All"))
        {
            DateTime t1 = DateTime.Now;
            FindAllShaderReferences();
            FindAllMaterialReferences();
            TimeSpan td = DateTime.Now - t1;
            DLog.Log("Completed in: " + td.TotalSeconds);
        }
    }

    [MenuItem("‽/Asset Management/Find Shader and Material References")]
    public static void ShowWindow() => GetWindow<FindUnusedAssetsInFolderWindow>().Show();

    public void FindAllShaderReferences()
    {
        const string SearchString = "  m_Shader";
        EditorUtility.DisplayProgressBar(ProgressBarTitle, string.Empty, 0f);

        string[] sourcePaths = Directory.GetFiles(Application.dataPath, "*.shader", SearchOption.AllDirectories);
        var sources = new Dictionary<string, string>(sourcePaths.Length);
        var references = new Dictionary<string, List<string>>(sources.Count);

        foreach (string sourcePath in sourcePaths)
        {
            string assetPath = "Assets" + sourcePath.Replace(Application.dataPath, string.Empty);
            sources.Add(AssetDatabase.AssetPathToGUID(assetPath), assetPath);
            references.Add(assetPath, new List<string>(1));
        }

        string[] targetPaths = Directory.GetFiles(Application.dataPath, "*.mat", SearchOption.AllDirectories);
        for (var j = 0; j < targetPaths.Length; ++j)
        {
            string path = targetPaths[j];
            string pathShort = path.Replace(Application.dataPath, string.Empty);

            EditorUtility.DisplayProgressBar(ProgressBarTitle, pathShort, j / (float)targetPaths.Length);

            var guidStrings = new List<string>(2);
            using (StreamReader fs = File.OpenText(path))
            {
                while (fs.Peek() != -1)
                {
                    string line = fs.ReadLine();
                    if (line.StartsWith(SearchString)) guidStrings.Add(line[SearchString.Length..]);
                }
            }

            foreach (KeyValuePair<string, string> kvp in sources)
            {
                string k = kvp.Key;
                foreach (string gs in guidStrings)
                    if (gs.Contains(k))
                    {
                        references[kvp.Value].Add(pathShort);
                        break;
                    }
            }
        }

        EditorUtility.ClearProgressBar();
        WriteRefsToFile(references, Application.dataPath + "/ShaderUse.csv");
    }

    public void FindAllMaterialReferences()
    {
        const string SearchString = "  m_Material";
        EditorUtility.DisplayProgressBar(ProgressBarTitle, string.Empty, 0f);

        string[] sourcePaths = Directory.GetFiles(Application.dataPath, "*.mat", SearchOption.AllDirectories);
        var sources = new Dictionary<string, string>(sourcePaths.Length);
        var references = new Dictionary<string, List<string>>(sources.Count);

        foreach (string sourcePath in sourcePaths)
        {
            string assetPath = "Assets" + sourcePath.Replace(Application.dataPath, string.Empty);
            sources.Add(AssetDatabase.AssetPathToGUID(assetPath), assetPath);
            references.Add(assetPath, new List<string>(1));
        }

        var targetPaths = new List<string>(256);
        targetPaths.AddRange(Directory.GetFiles(Application.dataPath, "*.unity", SearchOption.AllDirectories));
        targetPaths.AddRange(Directory.GetFiles(Application.dataPath, "*.prefab", SearchOption.AllDirectories));

        for (var j = 0; j < targetPaths.Count; ++j)
        {
            string path = targetPaths[j];
            string pathShort = path.Replace(Application.dataPath, string.Empty);

            EditorUtility.DisplayProgressBar(ProgressBarTitle, pathShort, j / (float)targetPaths.Count);

            var guidStrings = new List<string>(2);
            using (StreamReader fs = File.OpenText(path))
            {
                while (fs.Peek() != -1)
                    if (fs.ReadLine().StartsWith(SearchString))
                        guidStrings.Add(fs.ReadLine());
            }

            foreach (KeyValuePair<string, string> kvp in sources)
            {
                string k = kvp.Key;
                foreach (string gs in guidStrings)
                    if (gs.Contains(k))
                    {
                        references[kvp.Value].Add(pathShort);
                        break;
                    }
            }
        }

        EditorUtility.ClearProgressBar();
        WriteRefsToFile(references, Application.dataPath + "/MaterialUse.csv");
    }

    private void WriteRefsToFile(Dictionary<string, List<string>> refs, string logPath)
    {
        using (var file = new StreamWriter(logPath, false))
        {
            foreach (KeyValuePair<string, List<string>> kvp in refs)
            {
                string line = kvp.Key;
                foreach (string s in kvp.Value) line += ',' + s;
                file.WriteLine(line);
            }
        }
    }
}