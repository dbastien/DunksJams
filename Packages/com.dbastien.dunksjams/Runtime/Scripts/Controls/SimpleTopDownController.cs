using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Top-down movement. WASD moves in XZ, optional camera-relative or world-axial.</summary>
public class SimpleTopDownController : MonoBehaviour
{
    [SerializeField] InputActionAsset inputActions;
    [SerializeField] Transform cameraRoot;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float turnSpeed = 10f;
    [SerializeField] bool cameraRelative = true;

    InputAction moveAction;

    void Awake()
    {
        if (inputActions == null) return;
        moveAction = inputActions.FindActionMap("Player").FindAction("Move");
    }

    void OnEnable() => moveAction?.Enable();
    void OnDisable() => moveAction?.Disable();

    void Update()
    {
        var move = moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        if (move.sqrMagnitude < 0.01f) return;

        Vector3 dir;
        if (cameraRelative && cameraRoot != null)
        {
            var f = Vector3.ProjectOnPlane(cameraRoot.forward, Vector3.up).normalized;
            var r = Vector3.ProjectOnPlane(cameraRoot.right, Vector3.up).normalized;
            dir = (f * move.y + r * move.x).normalized;
        }
        else
            dir = new Vector3(move.x, 0f, move.y).normalized;

        if (dir.sqrMagnitude < 0.01f) return;
        transform.position += dir * moveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), turnSpeed * Time.deltaTime);
    }
}
