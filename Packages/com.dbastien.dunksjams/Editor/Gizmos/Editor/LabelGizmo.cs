using UnityEditor;
using UnityEngine;

/// <summary>
/// Scene editor gizmo which displays useful object information via labels
/// </summary>
public static class LabelGizmo
{
    //todo: make menu option to toggle stuff
    private static bool showGameObjectName = true;
    private static bool showVertexCount = true;
    private static bool showMaterialName;
    private static bool showShaderName = true;
    private static bool showShaderKeywords;

    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.Active | GizmoType.Pickable)]
    public static void GizmoLabels(GameObject go, GizmoType type)
    {
        string label = "";

        var renderer = go.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        if (showGameObjectName)
        {
            label += go.name;
        }

        if (showVertexCount)
        {
            var meshFilter = go.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                label += "\nverts: " + meshFilter.sharedMesh.vertexCount;
            }
        }

        var mat = renderer.sharedMaterial;
        if (mat != null)
        {
            if (showMaterialName)
            {
                label += "\nmat: " + mat.name;
            }

            var shader = mat.shader;
            if (shader != null)
            {
                if (showShaderName)
                {
                    label += "\nshader: " + shader.name;
                }

                if (showShaderKeywords)
                {
                    label += "\nshader keywords:\n" + string.Join("\n", mat.shaderKeywords);
                }
            }
        }

        Handles.Label(go.transform.position, label);
    }

    [MenuItem("‽/Gizmos/Label/Toggle Show Game Object Name")]
    public static void ToggleShowGameObjectName()
    {
        LabelGizmo.showGameObjectName = !LabelGizmo.showGameObjectName;
        SceneView.RepaintAll();
    }

    [MenuItem("‽/Gizmos/Label/Toggle Show Vertex Count")]
    public static void ToggleShowVertexCount()
    {
        LabelGizmo.showVertexCount = !LabelGizmo.showVertexCount;
        SceneView.RepaintAll();
    }

    [MenuItem("‽/Gizmos/Label/Toggle Show Material Name")]
    public static void ToggleShowMaterialName()
    {
        LabelGizmo.showMaterialName = !LabelGizmo.showMaterialName;
        SceneView.RepaintAll();
    }

    [MenuItem("‽/Gizmos/Label/Toggle Show Shader Name")]
    public static void ToggleShowShaderName()
    {
        LabelGizmo.showShaderName = !LabelGizmo.showShaderName;
        SceneView.RepaintAll();
    }

    [MenuItem("‽/Gizmos/Label/Toggle Show Shader Keywords")]
    public static void ToggleShowShaderKeywords()
    {
        LabelGizmo.showShaderKeywords = !LabelGizmo.showShaderKeywords;
        SceneView.RepaintAll();
    }
}
