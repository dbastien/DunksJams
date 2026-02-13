using UnityEditor;
using UnityEngine;

public class FacingGizmo : MonoBehaviour
{
    static bool on;

    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.Active | GizmoType.Pickable)]
    public static void DrawGizmo(Transform t, GizmoType type)
    {
        if (!on) return;

        Gizmos.color = Color.blue;
        GizmoUtils.DrawArrow(t.position, t.forward);

        Gizmos.color = Color.green;
        GizmoUtils.DrawArrow(t.position, t.up);
    }

    [MenuItem("‽/Gizmos/Toggle Facing Gizmo")]
    public static void ToggleShowFacing()
    {
        on = !on;
        SceneView.RepaintAll();
    }
}