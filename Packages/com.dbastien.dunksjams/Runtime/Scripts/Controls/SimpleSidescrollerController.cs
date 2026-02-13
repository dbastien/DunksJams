using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>2D sidescroller. WASD horizontal, Space to jump. Simple gravity and ground check.</summary>
[RequireComponent(typeof(CharacterController))]
public class SimpleSidescrollerController : MonoBehaviour
{
    [SerializeField] InputActionAsset inputActions;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpSpeed = 8f;
    [SerializeField] float gravity = 20f;
    [SerializeField] float groundCheckDistance = 0.2f;
    [SerializeField] Vector3 horizontalAxis = Vector3.right;

    InputAction moveAction;
    InputAction jumpAction;
    CharacterController controller;
    float velocityY;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (inputActions == null) return;
        var m = inputActions.FindActionMap("Player");
        moveAction = m.FindAction("Move");
        jumpAction = m.FindAction("Jump");
    }

    void OnEnable()
    {
        moveAction?.Enable();
        jumpAction?.Enable();
    }

    void OnDisable()
    {
        moveAction?.Disable();
        jumpAction?.Disable();
    }

    void Update()
    {
        var move = moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        var axis = horizontalAxis.y == 0f ? horizontalAxis.normalized : Vector3.ProjectOnPlane(horizontalAxis, Vector3.up).normalized;
        var horizontal = axis.x * move.x + axis.z * move.y;
        var grounded = controller.isGrounded || Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);

        if (grounded && jumpAction?.WasPressedThisFrame() == true)
            velocityY = jumpSpeed;
        velocityY -= gravity * Time.deltaTime;

        var delta = (axis * horizontal * moveSpeed + Vector3.up * velocityY) * Time.deltaTime;
        controller.Move(delta);
    }
}
