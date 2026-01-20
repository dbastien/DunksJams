using System.Linq;
using UnityEditor;
using UnityEngine;

public class FindReferencesInAssetsWindow : EditorWindow
{
    [MenuItem("‽/Asset Management/Find References")]
    public static void ShowWindow() => EditorWindow.GetWindow<FindReferencesInAssetsWindow>().Show();

    private Object target;
    bool checkAssetDatabase = false;
    bool checkScene = true;
    private string results;    

    public void OnGUI()
    {
        if (target == null && Selection.objects != null)
        {
            target = Selection.objects[0];
        }

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
        var path = AssetDatabase.GetAssetOrScenePath(target);

        var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
        if (asset == null)
        {
            Debug.LogError("Couldn't load asset!");
            return;
        }

        if (this.checkAssetDatabase)
        {
            var gameObjects = AssetDatabaseUtils.FindAndLoadAssets<GameObject>();
            results += string.Format("Searching {0} AssetDatabase GameObjects\n", gameObjects.Count);
            int countFound = gameObjects.Sum(go => FindReferences(asset, go));
            results += string.Format("{0} AssetDatabase references found\n", countFound);
        }

        if (this.checkScene)
        {
            var sceneGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            results += string.Format("Searching {0} Scene GameObjects\n", sceneGameObjects.Length);
            int sceneCountFound = sceneGameObjects.Sum(go => FindReferences(asset, go));
            results += string.Format("{0} Scene references found\n", sceneCountFound);            
        }
    }
    
    private  int FindReferences(Object asset, GameObject go)
    {
        int countFound = 0;
        if (PrefabUtility.GetPrefabType(go) == PrefabType.PrefabInstance)
        {
            if (PrefabUtility.GetPrefabParent(go) == asset)
            {
                results += string.Format("{0}\n", go.GetFullPath());
                ++countFound;
            }
        }

        var components = go.GetComponents<Component>();
        foreach (var component in components)
        {
            if (!component)
            {
                continue;
            }

            var so = new SerializedObject(component);

            var sp = so.GetIterator();
            while (sp.NextVisible(true))
            {
                if ((sp.propertyType == SerializedPropertyType.ObjectReference) && (sp.objectReferenceValue == asset))
                {
                    results += string.Format("{0}, Component {1}, Property {2}\n",
                                              go.GetFullPath(),
                                              ObjectNames.GetClassName(component),
                                              ObjectNames.NicifyVariableName(sp.name));
                    ++countFound;
                }
            }
        }

        return countFound;
    }
}
