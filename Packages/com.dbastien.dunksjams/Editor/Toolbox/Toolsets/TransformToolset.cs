using UnityEditor;
using UnityEngine;

[ToolsetProvider(displayName = "Transform Toolset", description = "Transform-related tools: create empty at origin, reset transform.")]
public class TransformToolset : IToolset
{
    GUIContent _createGOContent;
    GUIContent _zeroTransformContent;

    public void Setup()
    {
        _createGOContent = EditorGUIUtils.IconContentSafe("d_GameObject Icon", "GameObject Icon", "Create empty GameObject at origin");
        _zeroTransformContent = EditorGUIUtils.IconContentSafe("d_Refresh", "Refresh", "Reset selected transforms");
    }

    public void Teardown() => _ = 0;

    public void Draw()
    {
        if (GUILayout.Button(_createGOContent, ToolbarStyles.ToolbarButtonStyle))
            CreateGameObjectAtOrigin();
        if (GUILayout.Button(_zeroTransformContent, ToolbarStyles.ToolbarButtonStyle))
            ResetTransformForSelectedObjects();
    }

    public static void CreateGameObjectAtOrigin()
    {
        var go = new GameObject();
        Undo.RegisterCreatedObjectUndo(go, "Created Empty Game Object");
        go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        go.transform.localScale = Vector3.one;
        if (Selection.activeGameObject != null)
            go.transform.SetParent(Selection.activeGameObject.transform);
    }

    public static void ResetTransformForSelectedObjects()
    {
        var selections = Selection.transforms;
        if (selections == null || selections.Length == 0) return;
        Undo.RecordObjects(selections, "Reset Selected Transforms");
        foreach (var t in selections)
        {
            t.position = Vector3.zero;
            t.rotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }
    }
}
