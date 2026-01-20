using System.Linq;
using UnityEditor;
using UnityEngine;

public class FindAssetDependenciesWindow : EditorWindow
{
    const string ProgressBarTitle = "Searching for Dependencies";

    [MenuItem("‽/Asset Management/Find Dependencies")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<FindAssetDependenciesWindow>().Show();
    }

    private Object target;
    private string results;

    public void OnGUI()
    {
        if (target == null && Selection.objects != null)
        {
            target = Selection.objects[0];
        }

        target = EditorGUILayout.ObjectField("Find dependencies of:", target, typeof(GameObject), true);

        if (GUILayout.Button("Search"))
        {
            results = string.Empty;

            var roots = new Object[] { target };
            var dependencies = EditorUtility.CollectDependencies(roots);
            Selection.objects = dependencies;

            for (var i = 0; i < dependencies.Length; ++i)
            {
                var o = dependencies[i];
                var go = o as GameObject;

                results += (go ? go.GetFullPath() : o.name) + System.Environment.NewLine;
            }
        }

        EditorGUILayout.SelectableLabel(results, GUILayout.ExpandHeight(true));
    }
}
