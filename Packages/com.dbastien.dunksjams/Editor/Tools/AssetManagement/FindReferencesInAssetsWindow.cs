using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class FindReferencesInAssetsWindow : EditorWindow
{
    [MenuItem("‽/Asset Management/Find References")]
    public static void ShowWindow() => GetWindow<FindReferencesInAssetsWindow>().Show();

    private Object target;
    private bool checkAssetDatabase = false;
    private bool checkScene = true;
    private string results;

    public void OnGUI()
    {
        if (target == null && Selection.objects != null) target = Selection.objects[0];

        target = EditorGUILayout.ObjectField("Find references to:", target, typeof(Object), true);
        checkAssetDatabase = GUILayout.Toggle(checkAssetDatabase, "Search Asset Database");
        checkScene = GUILayout.Toggle(checkScene, "Search Scene");

        if (GUILayout.Button("Search"))
        {
            results = string.Empty;
            FindAllReferences();
        }

        EditorGUILayout.SelectableLabel(results, GUILayout.ExpandHeight(true));
    }

    public void FindAllReferences()
    {
        string path = AssetDatabase.GetAssetOrScenePath(target);

        var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
        if (asset == null)
        {
            DLog.LogE("Couldn't load asset!");
            return;
        }

        if (checkAssetDatabase)
        {
            List<GameObject> gameObjects = AssetDatabaseUtils.FindAndLoadAssets<GameObject>();
            results += $"Searching {gameObjects.Count} AssetDatabase GameObjects\n";
            int countFound = gameObjects.Sum(go => FindReferences(asset, go));
            results += $"{countFound} AssetDatabase references found\n";
        }

        if (checkScene)
        {
            GameObject[] sceneGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            results += $"Searching {sceneGameObjects.Length} Scene GameObjects\n";
            int sceneCountFound = sceneGameObjects.Sum(go => FindReferences(asset, go));
            results += $"{sceneCountFound} Scene references found\n";
        }
    }

    private int FindReferences(Object asset, GameObject go)
    {
        var countFound = 0;
        if (PrefabUtility.GetCorrespondingObjectFromOriginalSource(go) == asset)
        {
            results += $"{go.GetFullPath()}\n";
            ++countFound;
        }

        Component[] components = go.GetComponents<Component>();
        foreach (Component component in components)
        {
            if (!component) continue;

            var so = new SerializedObject(component);

            SerializedProperty sp = so.GetIterator();
            while (sp.NextVisible(true))
                if (sp.propertyType == SerializedPropertyType.ObjectReference && sp.objectReferenceValue == asset)
                {
                    results +=
                        $"{go.GetFullPath()}, Component {ObjectNames.GetClassName(component)}, Property {ObjectNames.NicifyVariableName(sp.name)}\n";
                    ++countFound;
                }
        }

        return countFound;
    }
}