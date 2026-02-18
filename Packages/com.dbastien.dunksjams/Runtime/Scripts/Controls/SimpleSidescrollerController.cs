using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>2D sidescroller. WASD horizontal, Space to jump. Simple gravity and ground check.</summary>
[RequireComponent(typeof(CharacterController))]
public class SimpleSidescrollerController : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpSpeed = 8f;
    [SerializeField] private float gravity = 20f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private Vector3 horizontalAxis = Vector3.right;

    private InputAction moveAction;
    private InputAction jumpAction;
    private CharacterController controller;
    private float velocityY;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (inputActions == null) return;
        InputActionMap m = inputActions.FindActionMap("Player");
        moveAction = m.FindAction("Move");
        jumpAction = m.FindAction("Jump");
    }

    private void OnEnable()
    {
        moveAction?.Enable();
        jumpAction?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        jumpAction?.Disable();
    }

    private void Update()
    {
        Vector2 move = moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        Vector3 axis = horizontalAxis.y == 0f
            ? horizontalAxis.normalized
            : Vector3.ProjectOnPlane(horizontalAxis, Vector3.up).normalized;
        float horizontal = axis.x * move.x + axis.z * move.y;
        bool grounded = controller.isGrounded || Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);

        if (grounded && jumpAction?.WasPressedThisFrame() == true)
            velocityY = jumpSpeed;
        velocityY -= gravity * Time.deltaTime;

        Vector3 delta = (axis * horizontal * moveSpeed + Vector3.up * velocityY) * Time.deltaTime;
        controller.Move(delta);
    }
}