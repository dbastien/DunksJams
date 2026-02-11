using UnityEditor;
using UnityEngine;

[ToolsetProvider(displayName = "Transform Toolset", description = "Transform-related tools: create empty at origin, reset transform.")]
public class TransformToolset : IToolset
{
    const string TexturePath = "Packages/com.dbastien.dunksjams/Editor/Toolbox/Textures/";
    GUIContent createGOContent;
    GUIContent zeroTransformContent;

    public void Setup()
    {
        var createIcon = EditorGUIUtility.Load(TexturePath + "TB_Icon_NewEmpty.png") as Texture2D;
        var zeroIcon = EditorGUIUtility.Load(TexturePath + "TB_Icon_ResetTransform.png") as Texture2D;
        createGOContent = new GUIContent(createIcon, "Create empty GameObject at origin");
        zeroTransformContent = new GUIContent(zeroIcon, "Reset selected transforms");
    }

    public void Teardown() { }

    public void Draw()
    {
        if (GUILayout.Button(createGOContent, ToolbarStyles.ToolbarButtonStyle))
            CreateGameObjectAtOrigin();
        if (GUILayout.Button(zeroTransformContent, ToolbarStyles.ToolbarButtonStyle))
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
