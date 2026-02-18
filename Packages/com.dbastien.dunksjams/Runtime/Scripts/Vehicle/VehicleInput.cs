using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(VehicleController))]
public class VehicleInput : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string actionMapName = "Vehicle";
    [SerializeField] private float steerSmoothing = 8f;

    private InputAction _throttleAction;
    private InputAction _brakeAction;
    private InputAction _steerAction;
    private InputAction _handbrakeAction;

    private VehicleController _vehicle;
    private float _smoothedSteer;

    private void Awake()
    {
        _vehicle = GetComponent<VehicleController>();

        if (inputActions == null) return;
        InputActionMap map = inputActions.FindActionMap(actionMapName);
        if (map == null)
        {
            DLog.LogW($"VehicleInput: Action map '{actionMapName}' not found, trying 'Player'.");
            map = inputActions.FindActionMap("Player");
        }

        if (map == null) return;

        _throttleAction = map.FindAction("Throttle") ?? map.FindAction("Vertical");
        _brakeAction = map.FindAction("Brake");
        _steerAction = map.FindAction("Steer") ?? map.FindAction("Horizontal") ?? map.FindAction("Move");
        _handbrakeAction = map.FindAction("Handbrake") ?? map.FindAction("Jump");
    }

    private void OnEnable()
    {
        _throttleAction?.Enable();
        _brakeAction?.Enable();
        _steerAction?.Enable();
        _handbrakeAction?.Enable();
    }

    private void OnDisable()
    {
        _throttleAction?.Disable();
        _brakeAction?.Disable();
        _steerAction?.Disable();
        _handbrakeAction?.Disable();
    }

    private void Update()
    {
        if (_vehicle == null) return;

        float rawThrottle = _throttleAction?.ReadValue<float>() ?? 0f;
        float rawBrake = _brakeAction?.ReadValue<float>() ?? 0f;

        // Steer can be float or Vector2 (if reusing Move action)
        var rawSteer = 0f;
        if (_steerAction != null)
        {
            if (_steerAction.expectedControlType == "Vector2")
                rawSteer = _steerAction.ReadValue<Vector2>().x;
            else
                rawSteer = _steerAction.ReadValue<float>();
        }

        bool handbrake = _handbrakeAction?.IsPressed() ?? false;

        // If no separate brake action, negative throttle = brake
        float throttle = rawThrottle;
        float brake = rawBrake;
        if (_brakeAction == null && rawThrottle < 0f)
        {
            throttle = rawThrottle; // will go negative for reverse
            brake = Mathf.Abs(Mathf.Min(rawThrottle, 0f));

            // Only brake if moving forward, otherwise reverse
            if (_vehicle.ForwardSpeed > 1f)
                throttle = 0f;
            else
                brake = 0f;
        }

        // Smooth steering
        _smoothedSteer = Mathf.MoveTowards(_smoothedSteer, rawSteer, steerSmoothing * Time.deltaTime);

        _vehicle.SetInput(throttle, _smoothedSteer, brake, handbrake);
    }
}