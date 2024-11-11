using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Haptics;
using System.Threading.Tasks;

//todo: add editor, and visualizer with real-time waveform and pad state info
public static class HapticsManager
{
    static readonly Dictionary<int, Gamepad> _pads = new();
    static float GlobalIntensity { get; set; } = 1f;
    static bool _isInitialized;

    public static void Initialize()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        UpdateConnectedGamepads();
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    static void OnDeviceChange(InputDevice device, InputDeviceChange change)
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

    static void UpdateConnectedGamepads()
    {
        _pads.Clear();
        foreach (Gamepad gp in Gamepad.all) _pads[gp.deviceId] = gp;
    }

    static bool IsGhostDevice(Gamepad pad) => pad == null || !pad.added;

    public static void SetGlobalIntensity(float intensity) =>
        GlobalIntensity = Mathf.Clamp01(intensity);

    public static async void PlayHaptic(Gamepad pad, HapticPreset preset)
    {
        if (IsGhostDevice(pad) || pad is not IDualMotorRumble rumble) return;

        if (preset.LowFreqCurve != null && preset.HighFreqCurve != null)
            await PlayCurveHaptic(pad, rumble, preset);
        else
            SimpleRumble(pad, rumble, preset.LowFreq, preset.HighFreq, preset.Seconds);
    }

    // feedback based on animation curves (async loop).
    static async Task PlayCurveHaptic(Gamepad pad, IDualMotorRumble rumble, HapticPreset preset)
    {
        float startTime = Time.time;

        while (Time.time - startTime < preset.Seconds)
        {
            if (IsGhostDevice(pad)) break;

            float t = (Time.time - startTime) / preset.Seconds;
            float low = preset.LowFreqCurve.Evaluate(t) * GlobalIntensity;
            float high = preset.HighFreqCurve.Evaluate(t) * GlobalIntensity;

            rumble.SetMotorSpeeds(low, high);
            await Task.Delay(10);  // Prevent frame spam
        }

        rumble.ResetHaptics();
    }

    public static void SimpleRumble(Gamepad pad, IDualMotorRumble rumble, float low, float high, float seconds)
    {
        if (IsGhostDevice(pad)) return;

        rumble.SetMotorSpeeds(low * GlobalIntensity, high * GlobalIntensity);
        StopRumbleAfterDuration(rumble, seconds);
    }

    static async void StopRumbleAfterDuration(IDualMotorRumble rumble, float seconds)
    {
        await Task.Delay((int)(seconds * 1000));
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

        foreach (var pad in _pads.Values)
            if (!IsGhostDevice(pad)) ResetHaptics(pad);

        _pads.Clear();
    }

    [CreateAssetMenu(fileName = "NewHapticPreset", menuName = "‽/HapticPreset")]
    public class HapticPreset : ScriptableObject
    {
        public AnimationCurve LowFreqCurve, HighFreqCurve;
        public float Seconds = 1f;
        public float LowFreq = 0.5f, HighFreq = 0.5f;
    }
}
