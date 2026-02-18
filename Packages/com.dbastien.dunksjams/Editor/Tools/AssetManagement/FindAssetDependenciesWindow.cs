using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class FindAssetDependenciesWindow : EditorWindow
{
    private const string ProgressBarTitle = "Searching for Dependencies";
    private string results;

    private Object target;

    public void OnGUI()
    {
        if (target == null && Selection.objects != null) target = Selection.objects[0];

        target = EditorGUILayout.ObjectField("Find dependencies of:", target, typeof(GameObject), true);

        if (GUILayout.Button("Search"))
        {
            results = string.Empty;

            Object[] roots = new[] { target };
            Object[] dependencies = EditorUtility.CollectDependencies(roots);
            Selection.objects = dependencies;

            for (var i = 0; i < dependencies.Length; ++i)
            {
                Object o = dependencies[i];
                var go = o as GameObject;

                results += (go ? go.GetFullPath() : o.name) + Environment.NewLine;
            }
        }

        EditorGUILayout.SelectableLabel(results, GUILayout.ExpandHeight(true));
    }

    [MenuItem("‽/Asset Management/Find Dependencies")]
    public static void ShowWindow() { GetWindow<FindAssetDependenciesWindow>().Show(); }
}