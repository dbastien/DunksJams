using UnityEditor;
using UnityEngine;

public static class LabelGizmo
{
    private static bool showGameObjectName = true;
    private static bool showVertexCount = true;
    private static bool showMaterialName;
    private static bool showShaderName = true;
    private static bool showShaderKeywords;

    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.Active | GizmoType.Pickable)]
    public static void GizmoLabels(GameObject go, GizmoType type)
    {
        var label = "";

        var renderer = go.GetComponent<Renderer>();
        if (renderer == null) return;

        if (showGameObjectName) label += go.name;

        if (showVertexCount)
        {
            var meshFilter = go.GetComponent<MeshFilter>();
            if (meshFilter != null) label += "\nverts: " + meshFilter.sharedMesh.vertexCount;
        }

        Material mat = renderer.sharedMaterial;
        if (mat != null)
        {
            if (showMaterialName) label += "\nmat: " + mat.name;

            Shader shader = mat.shader;
            if (shader != null)
            {
                if (showShaderName) label += "\nshader: " + shader.name;

                if (showShaderKeywords) label += "\nshader keywords:\n" + string.Join("\n", mat.shaderKeywords);
            }
        }

        Handles.Label(go.transform.position, label);
    }

    [MenuItem("‽/Gizmos/Label/Toggle Show Game Object Name")]
    public static void ToggleShowGameObjectName()
    {
        showGameObjectName = !showGameObjectName;
        SceneView.RepaintAll();
    }

    [MenuItem("‽/Gizmos/Label/Toggle Show Vertex Count")]
    public static void ToggleShowVertexCount()
    {
        showVertexCount = !showVertexCount;
        SceneView.RepaintAll();
    }

    [MenuItem("‽/Gizmos/Label/Toggle Show Material Name")]
    public static void ToggleShowMaterialName()
    {
        showMaterialName = !showMaterialName;
        SceneView.RepaintAll();
    }

    [MenuItem("‽/Gizmos/Label/Toggle Show Shader Name")]
    public static void ToggleShowShaderName()
    {
        showShaderName = !showShaderName;
        SceneView.RepaintAll();
    }

    [MenuItem("‽/Gizmos/Label/Toggle Show Shader Keywords")]
    public static void ToggleShowShaderKeywords()
    {
        showShaderKeywords = !showShaderKeywords;
        SceneView.RepaintAll();
    }
}