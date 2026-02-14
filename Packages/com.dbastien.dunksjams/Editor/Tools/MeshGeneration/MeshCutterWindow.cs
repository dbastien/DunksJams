using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor window that lets you cut a selected mesh along a plane,
/// creating two new GameObjects from the halves.
/// </summary>
public class MeshCutterWindow : EditorWindow
{
    Vector3 _planePoint = Vector3.zero;
    Vector3 _planeNormal = Vector3.up;

    readonly MeshCutter _cutter = new();

    void OnGUI()
    {
        EditorGUILayout.LabelField("Mesh Cutter", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        EditorGUILayout.HelpBox(
            "Select a GameObject with a MeshFilter, configure the cutting plane, then press Cut.",
            MessageType.Info);
        EditorGUILayout.Space(4);

        _planePoint = EditorGUILayout.Vector3Field("Plane Point (local)", _planePoint);
        _planeNormal = EditorGUILayout.Vector3Field("Plane Normal", _planeNormal);

        if (_planeNormal == Vector3.zero) _planeNormal = Vector3.up;

        EditorGUILayout.Space(4);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("X")) _planeNormal = Vector3.right;
        if (GUILayout.Button("Y")) _planeNormal = Vector3.up;
        if (GUILayout.Button("Z")) _planeNormal = Vector3.forward;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8);

        var go = Selection.activeGameObject;
        bool valid = go != null && go.GetComponent<MeshFilter>()?.sharedMesh != null;

        using (new EditorGUI.DisabledScope(!valid))
        {
            if (GUILayout.Button("Cut Selected Mesh", GUILayout.Height(30)))
                DoCut(go);
        }

        if (!valid)
        {
            EditorGUILayout.HelpBox("No valid MeshFilter selected.", MessageType.Warning);
        }
    }

    void DoCut(GameObject go)
    {
        var mf = go.GetComponent<MeshFilter>();
        var mesh = mf.sharedMesh;

        // Build plane in the object's local space
        var plane = new Plane(_planeNormal.normalized, _planePoint);

        if (!_cutter.Cut(mesh, plane, out var frontMesh, out var backMesh))
        {
            EditorUtility.DisplayDialog("Mesh Cutter", "The cutting plane does not intersect the mesh.", "OK");
            return;
        }

        var material = go.GetComponent<Renderer>()?.sharedMaterial;

        var front = CreateHalf(go.name + "_front", frontMesh, material, go.transform);
        var back = CreateHalf(go.name + "_back", backMesh, material, go.transform);

        Undo.RegisterCreatedObjectUndo(front, "Mesh Cut");
        Undo.RegisterCreatedObjectUndo(back, "Mesh Cut");

        go.SetActive(false);
        Undo.RecordObject(go, "Mesh Cut (hide original)");

        Selection.objects = new Object[] { front, back };
    }

    static GameObject CreateHalf(string name, Mesh mesh, Material mat, Transform source)
    {
        var halfGo = new GameObject(name);
        halfGo.AddComponent<MeshFilter>().sharedMesh = mesh;
        var mr = halfGo.AddComponent<MeshRenderer>();
        if (mat != null) mr.sharedMaterial = mat;

        halfGo.transform.position = source.position;
        halfGo.transform.rotation = source.rotation;
        halfGo.transform.localScale = source.localScale;

        return halfGo;
    }
}
