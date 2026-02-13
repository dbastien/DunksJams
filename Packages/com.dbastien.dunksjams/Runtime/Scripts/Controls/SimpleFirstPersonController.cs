using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleFirstPersonController : MonoBehaviour
{
    [SerializeField] InputActionAsset inputActions;
    [SerializeField] Transform camera;
    [SerializeField] Vector2 speed = Vector2.one;
    [SerializeField] float lookSensitivity = 0.1f;
    [SerializeField] float pitchMin = -89f;
    [SerializeField] float pitchMax = 89f;

    InputAction moveAction;
    InputAction lookAction;
    float pitch;

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
        if (camera == null)
            camera = GetComponentInChildren<Camera>()?.transform;
        if (camera != null)
        {
            var x = camera.localEulerAngles.x;
            pitch = x > 180f ? x - 360f : x;
        }
    }

    void Update()
    {
        if (inputActions == null) return;

        var move = moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        if (move.sqrMagnitude > 0f)
        {
            var flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            var flatRight = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
            var delta = (flatForward * (move.y * speed.y) + flatRight * (move.x * speed.x)) * Time.deltaTime;
            transform.position += delta;
        }

        var look = lookAction?.ReadValue<Vector2>() ?? Vector2.zero;
        if (look.sqrMagnitude > 0f)
        {
            transform.Rotate(0f, look.x * lookSensitivity, 0f);
            pitch = Mathf.Clamp(pitch - look.y * lookSensitivity, pitchMin, pitchMax);
            if (camera != null)
                camera.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }
}
