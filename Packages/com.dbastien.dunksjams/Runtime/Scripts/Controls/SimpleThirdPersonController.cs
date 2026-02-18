using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleThirdPersonController : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private Transform cameraRoot;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private float lookSensitivity = 0.1f;
    [SerializeField] private float pitchMin = -30f;
    [SerializeField] private float pitchMax = 60f;
    [SerializeField] private Vector3 cameraOffset = new(0f, 2f, -5f);

    private InputAction moveAction;
    private InputAction lookAction;
    private float cameraYaw;
    private float cameraPitch;

    private void Awake()
    {
        if (inputActions == null) return;
        InputActionMap playerMap = inputActions.FindActionMap("Player");
        moveAction = playerMap.FindAction("Move");
        lookAction = playerMap.FindAction("Look");
    }

    private void OnEnable()
    {
        moveAction?.Enable();
        lookAction?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        lookAction?.Disable();
    }

    private void Start()
    {
        if (cameraRoot == null) cameraRoot = Camera.main?.transform;
        if (cameraRoot != null)
        {
            Vector3 e = cameraRoot.eulerAngles;
            cameraYaw = e.y;
            cameraPitch = e.x > 180f ? e.x - 360f : e.x;
        }
    }

    private void Update()
    {
        if (inputActions == null) return;

        Vector2 move = moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        if (move.sqrMagnitude > 0f)
        {
            Transform cam = cameraRoot != null ? cameraRoot : transform;
            Vector3 flatForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
            Vector3 flatRight = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
            Vector3 dir = (flatForward * move.y + flatRight * move.x).normalized;
            if (dir.sqrMagnitude < 0.01f) dir = flatForward;
            transform.position += dir * moveSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir),
                turnSpeed * Time.deltaTime);
        }

        Vector2 look = lookAction?.ReadValue<Vector2>() ?? Vector2.zero;
        if (look.sqrMagnitude > 0f)
        {
            cameraYaw += look.x * lookSensitivity;
            cameraPitch = Mathf.Clamp(cameraPitch - look.y * lookSensitivity, pitchMin, pitchMax);
        }

        if (cameraRoot != null)
        {
            Quaternion rot = Quaternion.Euler(cameraPitch, cameraYaw, 0f);
            cameraRoot.position = transform.position + rot * cameraOffset;
            cameraRoot.rotation = rot;
        }
    }
}