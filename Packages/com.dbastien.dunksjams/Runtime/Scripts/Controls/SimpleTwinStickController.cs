using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Twin-stick: left stick/WASD move, right stick aim. With camera, mouse aims at cursor.</summary>
public class SimpleTwinStickController : MonoBehaviour
{
    [SerializeField] InputActionAsset inputActions;
    [SerializeField] Camera aimCamera;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float aimSmoothing = 15f;

    InputAction moveAction;
    InputAction lookAction;
    Vector3 aimDirection;

    void Awake()
    {
        if (inputActions == null) return;
        var m = inputActions.FindActionMap("Player");
        moveAction = m.FindAction("Move");
        lookAction = m.FindAction("Look");
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
        aimDirection = transform.forward;
        if (aimCamera == null) aimCamera = Camera.main;
    }

    void Update()
    {
        if (inputActions == null) return;

        var move = moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        if (move.sqrMagnitude > 0f)
        {
            var dir = new Vector3(move.x, 0f, move.y).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;
        }

        Vector3 targetAim = aimDirection;
        if (aimCamera != null && Mouse.current != null)
        {
            var ray = aimCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (new Plane(Vector3.up, transform.position).Raycast(ray, out var dist))
            {
                targetAim = (ray.GetPoint(dist) - transform.position).normalized;
                targetAim.y = 0f;
                if (targetAim.sqrMagnitude < 0.01f) targetAim = aimDirection;
            }
        }
        else
        {
            var look = lookAction?.ReadValue<Vector2>() ?? Vector2.zero;
            if (look.sqrMagnitude > 0.1f)
                targetAim = new Vector3(look.x, 0f, look.y).normalized;
        }

        if (targetAim.sqrMagnitude > 0.01f)
        {
            aimDirection = Vector3.Slerp(aimDirection, targetAim, aimSmoothing * Time.deltaTime).normalized;
            if (aimDirection.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(aimDirection);
        }
    }
}
