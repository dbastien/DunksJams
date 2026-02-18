using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Free-flight debug camera. WASD move, mouse look, Space/C for up/down, Shift for speed boost.</summary>
public class DebugFlyController : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private float baseSpeed = 10f;
    [SerializeField] private float sprintMultiplier = 3f;
    [SerializeField] private float lookSensitivity = 0.1f;
    [SerializeField] private float pitchMin = -89f;
    [SerializeField] private float pitchMax = 89f;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction crouchAction;
    private InputAction sprintAction;
    private float pitch;
    private float yaw;

    private void Awake()
    {
        if (inputActions == null) return;
        InputActionMap m = inputActions.FindActionMap("Player");
        moveAction = m.FindAction("Move");
        lookAction = m.FindAction("Look");
        jumpAction = m.FindAction("Jump");
        crouchAction = m.FindAction("Crouch");
        sprintAction = m.FindAction("Sprint");
    }

    private void OnEnable()
    {
        moveAction?.Enable();
        lookAction?.Enable();
        jumpAction?.Enable();
        crouchAction?.Enable();
        sprintAction?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        lookAction?.Disable();
        jumpAction?.Disable();
        crouchAction?.Disable();
        sprintAction?.Disable();
    }

    private void Start()
    {
        Vector3 e = transform.eulerAngles;
        yaw = e.y;
        pitch = e.x > 180f ? e.x - 360f : e.x;
    }

    private void Update()
    {
        if (inputActions == null) return;

        Vector2 look = lookAction?.ReadValue<Vector2>() ?? Vector2.zero;
        if (look.sqrMagnitude > 0f)
        {
            yaw += look.x * lookSensitivity;
            pitch = Mathf.Clamp(pitch - look.y * lookSensitivity, pitchMin, pitchMax);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        Vector2 move = moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        float up = (jumpAction?.IsPressed() == true ? 1f : 0f) - (crouchAction?.IsPressed() == true ? 1f : 0f);
        float sprint = sprintAction?.IsPressed() == true ? sprintMultiplier : 1f;
        float speed = baseSpeed * sprint * Time.deltaTime;

        Vector3 delta = transform.forward * (move.y * speed) +
                        transform.right * (move.x * speed) +
                        Vector3.up * (up * speed);
        if (delta.sqrMagnitude > 0f) transform.position += delta;
    }
}