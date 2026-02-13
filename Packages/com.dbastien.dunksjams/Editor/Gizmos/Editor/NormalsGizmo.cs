using UnityEditor;
using UnityEngine;

/// <summary>
/// Scene editor gizmo which displays object normals
/// </summary>
public static class NormalsGizmo
{
    static bool on;

    public static int MaxNormalsToDraw = 30;

    //will show up in gizmo list as "MeshFilter"
    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.Active | GizmoType.Pickable)]
    public static void DrawGizmo(MeshFilter m, GizmoType type)
    {
        if (!on) return;

        Gizmos.color = Color.yellow;

        var vertexCount = m.sharedMesh.vertices.Length;
        var vertexIncrement = (int)((float)vertexCount / MaxNormalsToDraw + 0.5f);

        if (vertexIncrement < 1) vertexIncrement = 1;

        for (var i = 0; i < m.sharedMesh.vertices.Length; i += vertexIncrement)
        {
            var v = m.transform.TransformPoint(m.sharedMesh.vertices[i]);
            var n = m.transform.TransformDirection(m.sharedMesh.normals[i].normalized);
            GizmoUtils.DrawArrow(v, n);
        }
    }

    [MenuItem("‽/Gizmos/Toggle Normals Gizmo")]
    public static void ToggleShowNormals()
    {
        on = !on;
        SceneView.RepaintAll();
    }
}