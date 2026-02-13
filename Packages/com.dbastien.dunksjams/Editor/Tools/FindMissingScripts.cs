using UnityEngine;
using UnityEditor;

public class FindMissingScripts : EditorWindow
{
    static int go_count = 0, components_count = 0, missing_count = 0;

    [MenuItem("‽/FindMissingScripts")]
    public static void ShowWindow()
    {
        GetWindow(typeof(FindMissingScripts));
    }

    public void OnGUI()
    {
        if (GUILayout.Button("Find Missing Scripts in selected GameObjects")) FindInSelected();
    }

    static void FindInSelected()
    {
        var go = Selection.gameObjects;
        go_count = 0;
        components_count = 0;
        missing_count = 0;
        foreach (var g in go) FindInGO(g);
        Debug.Log($"Searched {go_count} GameObjects, {components_count} components, found {missing_count} missing");
    }

    static void FindInGO(GameObject g)
    {
        go_count++;
        var components = g.GetComponents<Component>();
        for (var i = 0; i < components.Length; i++)
        {
            components_count++;
            if (components[i] == null)
            {
                missing_count++;
                var s = g.name;
                var t = g.transform;
                while (t.parent != null)
                {
                    s = t.parent.name + "/" + s;
                    t = t.parent;
                }

                Debug.Log(s + " has an empty script attached in position: " + i, g);
            }
        }

        // Now recurse through each child GO (if there are any):
        foreach (Transform childT in g.transform)
            //Debug.Log("Searching " + childT.name  + " " );
            FindInGO(childT.gameObject);
    }
}