using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Free-flight debug camera. WASD move, mouse look, Space/C for up/down, Shift for speed boost.</summary>
public class DebugFlyController : MonoBehaviour
{
    [SerializeField] InputActionAsset inputActions;
    [SerializeField] float baseSpeed = 10f;
    [SerializeField] float sprintMultiplier = 3f;
    [SerializeField] float lookSensitivity = 0.1f;
    [SerializeField] float pitchMin = -89f;
    [SerializeField] float pitchMax = 89f;

    InputAction moveAction;
    InputAction lookAction;
    InputAction jumpAction;
    InputAction crouchAction;
    InputAction sprintAction;
    float pitch;
    float yaw;

    void Awake()
    {
        if (inputActions == null) return;
        var m = inputActions.FindActionMap("Player");
        moveAction = m.FindAction("Move");
        lookAction = m.FindAction("Look");
        jumpAction = m.FindAction("Jump");
        crouchAction = m.FindAction("Crouch");
        sprintAction = m.FindAction("Sprint");
    }

    void OnEnable()
    {
        moveAction?.Enable();
        lookAction?.Enable();
        jumpAction?.Enable();
        crouchAction?.Enable();
        sprintAction?.Enable();
    }

    void OnDisable()
    {
        moveAction?.Disable();
        lookAction?.Disable();
        jumpAction?.Disable();
        crouchAction?.Disable();
        sprintAction?.Disable();
    }

    void Start()
    {
        var e = transform.eulerAngles;
        yaw = e.y;
        pitch = e.x > 180f ? e.x - 360f : e.x;
    }

    void Update()
    {
        if (inputActions == null) return;

        var look = lookAction?.ReadValue<Vector2>() ?? Vector2.zero;
        if (look.sqrMagnitude > 0f)
        {
            yaw += look.x * lookSensitivity;
            pitch = Mathf.Clamp(pitch - look.y * lookSensitivity, pitchMin, pitchMax);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        var move = moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        var up = (jumpAction?.IsPressed() == true ? 1f : 0f) - (crouchAction?.IsPressed() == true ? 1f : 0f);
        var sprint = sprintAction?.IsPressed() == true ? sprintMultiplier : 1f;
        var speed = baseSpeed * sprint * Time.deltaTime;

        var delta = transform.forward * (move.y * speed) + transform.right * (move.x * speed) + Vector3.up * (up * speed);
        if (delta.sqrMagnitude > 0f) transform.position += delta;
    }
}
