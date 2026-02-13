using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleThirdPersonController : MonoBehaviour
{
    [SerializeField] InputActionAsset inputActions;
    [SerializeField] Transform cameraRoot;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float turnSpeed = 10f;
    [SerializeField] float lookSensitivity = 0.1f;
    [SerializeField] float pitchMin = -30f;
    [SerializeField] float pitchMax = 60f;
    [SerializeField] Vector3 cameraOffset = new(0f, 2f, -5f);

    InputAction moveAction;
    InputAction lookAction;
    float cameraYaw;
    float cameraPitch;

    void Awake()
    {
        if (inputActions == null) return;
        var playerMap = inputActions.FindActionMap("Player");
        moveAction = playerMap.FindAction("Move");
        lookAction = playerMap.FindAction("Look");
    }

    void OnEnable()
    {
        moveAction?.Enable();
        lookAction?.Enable();
    }

    void OnDisable()
    {
        moveAction?.Disable();
        lookAction?.Disable();
    }

    void Start()
    {
        if (cameraRoot == null) cameraRoot = Camera.main?.transform;
        if (cameraRoot != null)
        {
            var e = cameraRoot.eulerAngles;
            cameraYaw = e.y;
            cameraPitch = e.x > 180f ? e.x - 360f : e.x;
        }
    }

    void Update()
    {
        if (inputActions == null) return;

        var move = moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        if (move.sqrMagnitude > 0f)
        {
            var cam = cameraRoot != null ? cameraRoot : transform;
            var flatForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
            var flatRight = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
            var dir = (flatForward * move.y + flatRight * move.x).normalized;
            if (dir.sqrMagnitude < 0.01f) dir = flatForward;
            transform.position += dir * moveSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir),
                turnSpeed * Time.deltaTime);
        }

        var look = lookAction?.ReadValue<Vector2>() ?? Vector2.zero;
        if (look.sqrMagnitude > 0f)
        {
            cameraYaw += look.x * lookSensitivity;
            cameraPitch = Mathf.Clamp(cameraPitch - look.y * lookSensitivity, pitchMin, pitchMax);
        }

        if (cameraRoot != null)
        {
            var rot = Quaternion.Euler(cameraPitch, cameraYaw, 0f);
            cameraRoot.position = transform.position + rot * cameraOffset;
            cameraRoot.rotation = rot;
        }
    }
}