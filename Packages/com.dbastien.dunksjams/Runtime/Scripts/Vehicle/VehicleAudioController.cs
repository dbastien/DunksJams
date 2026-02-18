using UnityEngine;

[RequireComponent(typeof(VehicleController))]
public class VehicleAudioController : MonoBehaviour
{
    [Header("Engine")] [SerializeField] private AudioClipReference engineClip;
    [SerializeField] private float engineMinPitch = 0.6f;
    [SerializeField] private float engineMaxPitch = 2.5f;
    [SerializeField] [Range(0f, 1f)] private float engineBaseVolume = 0.5f;
    [SerializeField] private float engineVolumeThrottleBoost = 0.3f;

    [Header("Tire")] [SerializeField] private AudioClipReference tireSkidClip;
    [SerializeField] private float skidSlipThreshold = 0.3f;
    [SerializeField] [Range(0f, 1f)] private float skidMaxVolume = 0.4f;

    [Header("Wind")] [SerializeField] private AudioClipReference windClip;
    [SerializeField] private float windSpeedThreshold = 15f;
    [SerializeField] [Range(0f, 1f)] private float windMaxVolume = 0.3f;

    [Header("Impact")] [SerializeField] private AudioClipReference impactSound;

    private VehicleController _vehicle;
    private AudioSource _engineSource;
    private AudioSource _skidSource;
    private AudioSource _windSource;

    private void Awake() => _vehicle = GetComponent<VehicleController>();

    private void OnEnable()
    {
        _vehicle.OnImpact += HandleImpact;

        if (engineClip?.Clip != null)
            _engineSource = AudioSystem.Instance?.PlayLooped(engineClip.Clip, engineBaseVolume, true);
        if (tireSkidClip?.Clip != null)
            _skidSource = AudioSystem.Instance?.PlayLooped(tireSkidClip.Clip, 0f, true);
        if (windClip?.Clip != null)
            _windSource = AudioSystem.Instance?.PlayLooped(windClip.Clip, 0f, true);
    }

    private void OnDisable()
    {
        _vehicle.OnImpact -= HandleImpact;

        if (_engineSource != null) AudioSystem.Instance?.StopLooped(_engineSource);
        if (_skidSource != null) AudioSystem.Instance?.StopLooped(_skidSource);
        if (_windSource != null) AudioSystem.Instance?.StopLooped(_windSource);

        _engineSource = null;
        _skidSource = null;
        _windSource = null;
    }

    private void Update()
    {
        UpdateEngine();
        UpdateSkid();
        UpdateWind();
    }

    private void UpdateEngine()
    {
        if (_engineSource == null || _vehicle.Drivetrain == null) return;

        float rpmNorm = Mathf.InverseLerp(
            _vehicle.Drivetrain.IdleRPM,
            _vehicle.Drivetrain.MaxRPM,
            _vehicle.EngineRPM);

        _engineSource.pitch = Mathf.Lerp(engineMinPitch, engineMaxPitch, rpmNorm);
        _engineSource.volume = engineBaseVolume + Mathf.Abs(_vehicle.ThrottleInput) * engineVolumeThrottleBoost;

        if (_engineSource.transform != null)
            _engineSource.transform.position = transform.position;
    }

    private void UpdateSkid()
    {
        if (_skidSource == null) return;

        var maxSlip = 0f;
        foreach (VehicleWheel w in _vehicle.Wheels)
        {
            if (!w.IsGrounded) continue;
            if (w.CombinedSlip > maxSlip) maxSlip = w.CombinedSlip;
        }

        float skidVol = maxSlip > skidSlipThreshold
            ? Mathf.InverseLerp(skidSlipThreshold, 1.5f, maxSlip) * skidMaxVolume
            : 0f;

        _skidSource.volume = skidVol;
        if (_skidSource.transform != null)
            _skidSource.transform.position = transform.position;
    }

    private void UpdateWind()
    {
        if (_windSource == null) return;

        float speed = Mathf.Abs(_vehicle.ForwardSpeed);
        float windVol = speed > windSpeedThreshold
            ? Mathf.InverseLerp(windSpeedThreshold, 60f, speed) * windMaxVolume
            : 0f;

        _windSource.volume = windVol;
        _windSource.pitch = 0.8f + Mathf.InverseLerp(0f, 60f, speed) * 0.4f;
    }

    private void HandleImpact(Vector3 point, Vector3 normal, float impulse)
    {
        if (impactSound == null) return;

        float volume = Mathf.Clamp01(impulse / 5000f);
        if (volume < 0.05f) return;
        impactSound.Play3D(point);
    }
}