using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Haptics;

//todo: add editor, and visualizer with real-time waveform and pad state info
public static class HapticsManager
{
    private static readonly Dictionary<int, Gamepad> _pads = new();
    private static readonly WaitForSecondsRealtime _curveTick = new(0.01f);
    private static float GlobalIntensity { get; set; } = 1f;
    private static bool _isInitialized;

    public static void Initialize()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        UpdateConnectedGamepads();
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private static void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is not Gamepad gamepad) return;

        switch (change)
        {
            case InputDeviceChange.Added:
                _pads[gamepad.deviceId] = gamepad;
                break;
            case InputDeviceChange.Removed:
                _pads.Remove(gamepad.deviceId);
                break;
        }
    }

    private static void UpdateConnectedGamepads()
    {
        _pads.Clear();
        foreach (Gamepad gp in Gamepad.all) _pads[gp.deviceId] = gp;
    }

    private static bool IsGhostDevice(Gamepad pad) => pad == null || !pad.added;

    public static void SetGlobalIntensity(float intensity) =>
        GlobalIntensity = Mathf.Clamp01(intensity);

    public static void PlayHaptic(Gamepad pad, HapticPreset preset)
    {
        if (IsGhostDevice(pad) || pad is not IDualMotorRumble rumble) return;

        if (preset.LowFreqCurve != null && preset.HighFreqCurve != null)
            StartHapticCoroutine(PlayCurveHaptic(pad, rumble, preset));
        else
            SimpleRumble(pad, rumble, preset.LowFreq, preset.HighFreq, preset.Seconds);
    }

    // feedback based on animation curves (coroutine loop).
    private static IEnumerator PlayCurveHaptic(Gamepad pad, IDualMotorRumble rumble, HapticPreset preset)
    {
        float startTime = Time.unscaledTime;

        while (Time.unscaledTime - startTime < preset.Seconds)
        {
            if (IsGhostDevice(pad)) break;

            float t = (Time.unscaledTime - startTime) / preset.Seconds;
            float low = preset.LowFreqCurve.Evaluate(t) * GlobalIntensity;
            float high = preset.HighFreqCurve.Evaluate(t) * GlobalIntensity;

            rumble.SetMotorSpeeds(low, high);
            yield return _curveTick;
        }

        rumble.ResetHaptics();
    }

    public static void SimpleRumble(Gamepad pad, IDualMotorRumble rumble, float low, float high, float seconds)
    {
        if (IsGhostDevice(pad)) return;

        rumble.SetMotorSpeeds(low * GlobalIntensity, high * GlobalIntensity);
        StartHapticCoroutine(StopRumbleAfterDuration(rumble, seconds));
    }

    private static IEnumerator StopRumbleAfterDuration(IDualMotorRumble rumble, float seconds)
    {
        if (seconds > 0f)
            yield return new WaitForSecondsRealtime(seconds);
        rumble.ResetHaptics();
    }

    public static void ResetHaptics(Gamepad pad)
    {
        if (pad is IHaptics hapticDevice) hapticDevice.ResetHaptics();
    }

    public static void Shutdown()
    {
        if (!_isInitialized) return;
        _isInitialized = false;

        InputSystem.onDeviceChange -= OnDeviceChange;

        foreach (Gamepad pad in _pads.Values)
            if (!IsGhostDevice(pad))
                ResetHaptics(pad);

        _pads.Clear();
    }

    private static void StartHapticCoroutine(IEnumerator routine)
    {
        AsyncRunner runner = AsyncRunner.Instance;
        if (runner == null) return;
        runner.StartCoroutine(routine);
    }

    [CreateAssetMenu(fileName = "NewHapticPreset", menuName = "‽/HapticPreset")]
    public class HapticPreset : ScriptableObject
    {
        public AnimationCurve LowFreqCurve, HighFreqCurve;
        public float Seconds = 1f;
        public float LowFreq = 0.5f, HighFreq = 0.5f;
    }
}