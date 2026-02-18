using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor menu utility for exporting meshes to OBJ files.
/// </summary>
public static class MeshExportMenu
{
    public static void ExportSelectedToObj()
    {
        GameObject go = Selection.activeGameObject;
        if (go == null)
        {
            EditorUtility.DisplayDialog("Export Mesh", "No GameObject selected.", "OK");
            return;
        }

        var mf = go.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
        {
            EditorUtility.DisplayDialog("Export Mesh", "Selected object has no MeshFilter with a valid mesh.", "OK");
            return;
        }

        string path = EditorUtility.SaveFilePanel(
            "Export Mesh to OBJ",
            Application.dataPath,
            go.name + ".obj",
            "obj");

        if (string.IsNullOrEmpty(path)) return;

        ObjExporter.MeshToFile(mf, path, true);
        DLog.Log($"Mesh exported to {path}");
    }
}