using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class FindUnusedAssetsInFolderWindow : EditorWindow
{
    const string ProgressBarTitle = "Searching for References";

    [MenuItem("‽/Asset Management/Find Shader and Material References")]
    public static void ShowWindow() => EditorWindow.GetWindow<FindUnusedAssetsInFolderWindow>().Show();

    public void OnGUI()
    {
        if (GUILayout.Button("Write Shader Usages"))
        {
            FindAllShaderReferences();
        }

        if (GUILayout.Button("Write Material Usages"))
        {
            FindAllMaterialReferences();
        }

        if (GUILayout.Button("Write All"))
        {
            var t1 = System.DateTime.Now;
            FindAllShaderReferences();
            FindAllMaterialReferences();
            var td = System.DateTime.Now - t1; 
            Debug.Log("Completed in: " + td.TotalSeconds);
        }        
    }

    public void FindAllShaderReferences()
    {
        const string SearchString = "  m_Shader";       
        EditorUtility.DisplayProgressBar(ProgressBarTitle, string.Empty, 0f);

        var sourcePaths = Directory.GetFiles(Application.dataPath, "*.shader", SearchOption.AllDirectories);
        var sources = new Dictionary<string, string>(sourcePaths.Length);
        var references = new Dictionary<string, List<string>>(sources.Count);
       
        foreach (var sourcePath in sourcePaths)
        {
            string assetPath = "Assets" + sourcePath.Replace(Application.dataPath, string.Empty);
            sources.Add(AssetDatabase.AssetPathToGUID(assetPath), assetPath);
            references.Add(assetPath, new List<string>(1));           
        }

		var targetPaths = Directory.GetFiles(Application.dataPath, "*.mat", SearchOption.AllDirectories);
        for (var j = 0; j < targetPaths.Length; ++j)
        {
            var path = targetPaths[j];        
            var pathShort = path.Replace(Application.dataPath, string.Empty);
	        
            EditorUtility.DisplayProgressBar(ProgressBarTitle, pathShort, (j / (float)targetPaths.Length));

            var guidStrings = new List<string>(2);
            using (var fs = File.OpenText(path))
            {
                while (fs.Peek() != -1)
                {
                    var line = fs.ReadLine();
                    if (line.StartsWith(SearchString))
                    {
                        guidStrings.Add(line.Substring(SearchString.Length));
                    }
                }
            }

            foreach(var kvp in sources)
            {
                var k = kvp.Key;               
                foreach(var gs in guidStrings)
                {
                    if (gs.Contains(k))
                    {
                        references[kvp.Value].Add(pathShort);
                        break;
                    }
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

        var sourcePaths = Directory.GetFiles(Application.dataPath, "*.mat", SearchOption.AllDirectories);
        var sources = new Dictionary<string, string>(sourcePaths.Length);
        var references = new Dictionary<string, List<string>>(sources.Count);

        foreach (var sourcePath in sourcePaths)
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
            var path = targetPaths[j];        
            var pathShort = path.Replace(Application.dataPath, string.Empty);
	        
            EditorUtility.DisplayProgressBar(ProgressBarTitle, pathShort, (j / (float)targetPaths.Count));

            var guidStrings = new List<string>(2);
            using (var fs = File.OpenText(path))
            {
                while (fs.Peek() != -1)
                {
                    if (fs.ReadLine().StartsWith(SearchString))
                    {
                        guidStrings.Add(fs.ReadLine());
                    }
                }
            }

            foreach(var kvp in sources)
            {
                var k = kvp.Key;
                foreach(var gs in guidStrings)
                {
                    if (gs.Contains(k))
                    {
                        references[kvp.Value].Add(pathShort);
                        break;
                    }
                }
            }
        }

        EditorUtility.ClearProgressBar();
        WriteRefsToFile(references, Application.dataPath + "/MaterialUse.csv");   
     }

    private void WriteRefsToFile(Dictionary<string, List<string>> refs, string logPath)
    {
        using (var file = new System.IO.StreamWriter(logPath, false))
        {
            foreach (var kvp in refs)
            {
                var line = kvp.Key;
                foreach (var s in kvp.Value)
                {
                    line += ',' + s;
                }
                file.WriteLine(line);
            }
        }
    }    
}
