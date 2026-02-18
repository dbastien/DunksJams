using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleFirstPersonController : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private Transform camera;
    [SerializeField] private Vector2 speed = Vector2.one;
    [SerializeField] private float lookSensitivity = 0.1f;
    [SerializeField] private float pitchMin = -89f;
    [SerializeField] private float pitchMax = 89f;

    private InputAction moveAction;
    private InputAction lookAction;
    private float pitch;

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
        if (camera == null)
            camera = GetComponentInChildren<Camera>()?.transform;
        if (camera != null)
        {
            float x = camera.localEulerAngles.x;
            pitch = x > 180f ? x - 360f : x;
        }
    }

    private void Update()
    {
        if (inputActions == null) return;

        Vector2 move = moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        if (move.sqrMagnitude > 0f)
        {
            Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            Vector3 flatRight = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
            Vector3 delta = (flatForward * (move.y * speed.y) + flatRight * (move.x * speed.x)) * Time.deltaTime;
            transform.position += delta;
        }

        Vector2 look = lookAction?.ReadValue<Vector2>() ?? Vector2.zero;
        if (look.sqrMagnitude > 0f)
        {
            transform.Rotate(0f, look.x * lookSensitivity, 0f);
            pitch = Mathf.Clamp(pitch - look.y * lookSensitivity, pitchMin, pitchMax);
            if (camera != null)
                camera.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }
}