using UnityEngine;

[RequireComponent(typeof(VehicleController))]
public class VehicleTelemetry : MonoBehaviour
{
    [SerializeField] bool showTelemetry = true;
    [SerializeField] bool showGizmos = true;
    [SerializeField] KeyCode toggleKey = KeyCode.F3;

    VehicleController _vehicle;
    GUIStyle _style;
    GUIStyle _headerStyle;

    void Awake() => _vehicle = GetComponent<VehicleController>();

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            showTelemetry = !showTelemetry;
    }

    void OnGUI()
    {
        if (!showTelemetry || _vehicle == null) return;

        _style ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            normal = { textColor = Color.white }
        };

        _headerStyle ??= new GUIStyle(_style)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 14
        };

        float x = 10, y = 10;
        float lineHeight = 18;

        Label(ref y, _headerStyle, $"Vehicle Telemetry [{_vehicle.name}]");
        Label(ref y, _style, $"Speed: {_vehicle.ForwardSpeed:F1} m/s | {_vehicle.SpeedKmh:F0} km/h | {_vehicle.SpeedMph:F0} mph");
        Label(ref y, _style, $"Gear: {GearString(_vehicle.CurrentGear)} | RPM: {_vehicle.EngineRPM:F0}{(_vehicle.IsShifting ? " [SHIFTING]" : "")}");
        Label(ref y, _style, $"Input: T={_vehicle.ThrottleInput:F2} S={_vehicle.SteerInput:F2} B={_vehicle.BrakeInput:F2}{(_vehicle.HandbrakeInput ? " [HB]" : "")}");
        Label(ref y, _style, $"Grounded: {_vehicle.IsGrounded}");

        y += 4;
        Label(ref y, _headerStyle, "Wheels");

        foreach (var w in _vehicle.Wheels)
        {
            string ground = w.IsGrounded ? "Y" : "N";
            string surface = w.CurrentSurface != null ? w.CurrentSurface.name : "none";
            Label(ref y, _style,
                $"  {w.name}: G={ground} C={w.Compression:F2} LS={w.LongitudinalSlip:F3} LA={w.LateralSlipAngle:F1}° CS={w.CombinedSlip:F2} SF={w.SuspensionForce:F0}N [{surface}]");
        }

        void Label(ref float yPos, GUIStyle s, string text)
        {
            GUI.Label(new Rect(x, yPos, 800, lineHeight), text, s);
            yPos += lineHeight;
        }
    }

    static string GearString(int gear) => gear < 0 ? "R" : gear == 0 ? "1" : (gear + 1).ToString();

    void OnDrawGizmos()
    {
        if (!showGizmos || _vehicle == null) return;

        foreach (var w in _vehicle.Wheels)
        {
            if (!w.IsGrounded) continue;

            // Suspension force
            Gizmos.color = Color.yellow;
            float forceScale = w.SuspensionForce / (_vehicle.Rb.mass * Physics.gravity.magnitude);
            Gizmos.DrawRay(w.transform.position, w.transform.up * forceScale * 0.5f);

            // Tire force
            if (w.TireForceWorld.sqrMagnitude > 1f)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(w.ContactPoint, w.TireForceWorld.normalized * 0.3f);
            }

            // Slip indicator
            Gizmos.color = w.CombinedSlip > 0.3f ? Color.red : Color.green;
            Gizmos.DrawSphere(w.ContactPoint, 0.05f);
        }

        // Center of mass
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(_vehicle.transform.TransformPoint(_vehicle.Rb.centerOfMass), 0.1f);
    }
}
