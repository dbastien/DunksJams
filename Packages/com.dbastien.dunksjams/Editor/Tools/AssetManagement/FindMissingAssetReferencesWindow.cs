using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class FindMissingAssetReferencesWindow : EditorWindow
{
    [MenuItem("‽/Asset Management/Find Missing References")]
    public static void ShowWindow() => GetWindow<FindMissingAssetReferencesWindow>().Show();

    private bool checkAssetDatabase = false;
    private bool checkScene = true;
    private string results;

    public void OnGUI()
    {
        checkAssetDatabase = GUILayout.Toggle(checkAssetDatabase, "Search Asset Database");
        checkScene = GUILayout.Toggle(checkScene, "Search Scene");

        if (GUILayout.Button("Search"))
        {
            results = string.Empty;
            FindAllMissingReferences();
        }

        EditorGUILayout.SelectableLabel(results, GUILayout.ExpandHeight(true));
    }

    public void FindAllMissingReferences()
    {
        if (checkAssetDatabase)
        {
            List<GameObject> gameObjects = AssetDatabaseUtils.FindAndLoadAssets<GameObject>();
            results += string.Format("Searching {0} AssetDatabase GameObjects for missing references\n",
                gameObjects.Count);
            int missingCount = gameObjects.Sum(go => FindMissingReferences(go));
            results += string.Format("{0} AssetDatabase missing references found\n", missingCount);
        }

        if (checkScene)
        {
            GameObject[] sceneGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            results += string.Format("Searching {0} Scene GameObjects for missing references\n",
                sceneGameObjects.Length);
            int missingCountScene = sceneGameObjects.Sum(go => FindMissingReferences(go));
            results += string.Format("{0} Scene missing references found\n", missingCountScene);
        }
    }

    private int FindMissingReferences(GameObject go)
    {
        var missingCount = 0;
        Component[] components = go.GetComponents<Component>();
        foreach (Component component in components)
        {
            if (!component)
            {
                results += string.Format("Missing component for: {0}\n", go.GetFullPath());
                ++missingCount;
                continue;
            }

            var so = new SerializedObject(component);

            SerializedProperty sp = so.GetIterator();
            while (sp.NextVisible(true))
                if (sp.propertyType == SerializedPropertyType.ObjectReference &&
                    sp.objectReferenceInstanceIDValue != 0 &&
                    sp.objectReferenceValue == null)
                {
                    results += string.Format("Missing asset for: {0}, Component: {1}, Property: {2}\n",
                        go.GetFullPath(),
                        ObjectNames.GetClassName(component),
                        ObjectNames.NicifyVariableName(sp.name));
                    ++missingCount;
                }
        }

        return missingCount;
    }
}