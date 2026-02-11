using UnityEditor;
using UnityEngine;

/// <summary>
/// Scene editor gizmo which displays object facing information
/// </summary>
public class FacingGizmo : MonoBehaviour
{
    private static bool on;

    //will show up in gizmo list as "MeshFilter"
    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.Active | GizmoType.Pickable)]
    public static void DrawGizmo(Transform t, GizmoType type)
    {
        if (!on)
        {
            return;
        }

        Gizmos.color = Color.blue;
        GizmoUtils.DrawArrow(t.position, t.forward);

        Gizmos.color = Color.green;
        GizmoUtils.DrawArrow(t.position, t.up);
    }

    [MenuItem("Gizmos/Toggle Facing Gizmo")]
    public static void ToggleShowFacing()
    {
        FacingGizmo.on = !FacingGizmo.on;
        SceneView.RepaintAll();
    }
}
