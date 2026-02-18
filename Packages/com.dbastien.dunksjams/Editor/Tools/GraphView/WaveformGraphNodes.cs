using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public class WaveformParameters
{
    private const float MinAmplitude = 0f;
    private const float MaxAmplitude = 2f;

    public float Amplitude { get; set; } = 1f;
    public float Frequency { get; set; } = 440f;
    public float Duration { get; set; } = 1f;

    public bool Draw()
    {
        var changed = false;
        EditorGUI.BeginChangeCheck();
        Amplitude = EditorGUILayout.Slider("Amplitude", Amplitude, MinAmplitude, MaxAmplitude);
        Frequency = EditorGUILayout.FloatField("Frequency", Frequency);
        Duration = EditorGUILayout.FloatField("Duration", Duration);
        if (EditorGUI.EndChangeCheck()) changed = true;
        return changed;
    }
}

public class WaveformNodeBase : SerializedGraphNode { }

public abstract class WaveformNodeBase<T> : WaveformNodeBase, IDataPort<T>
{
    protected T Data;

    public T GetData() => Data;
    public object GetDataAsObject() => Data;
    public void SetData(T data) => Data = data;
    public void SetDataFromObject(object data) => SetData((T)data);
}

public class AnimationCurveNode : WaveformNodeBase<AnimationCurve>
{
    private readonly WaveformParameters _params = new();
    private DataPort<AnimationCurve> _outPort;

    public override void Init(Vector2 pos, Vector2 size = default)
    {
        title = "Animation Curve";
        extensionContainer.Add(new IMGUIContainer(Draw));
        _outPort = CreatePort<AnimationCurve>("Out", Direction.Output);
        Data = new AnimationCurve();
        _outPort.SetData(Data);
        base.Init(pos, size);
    }

    private void Draw()
    {
        bool changed = _params.Draw();
        EditorGUI.BeginChangeCheck();
        Data = EditorGUILayout.CurveField(Data, GUILayout.Height(100));
        if (EditorGUI.EndChangeCheck()) changed = true;
        if (changed)
        {
            _outPort.SetData(Data);
            PropagateData();
        }
    }
}

public class SineWaveNode : WaveformNodeBase<AnimationCurve>
{
    private WaveformParameters _params = new();
    private int _sampleCount = 100;
    private DataPort<AnimationCurve> _outPort;

    public SineWaveNode() { }

    public override void Init(Vector2 pos, Vector2 size = default)
    {
        title = "Sine Wave";
        extensionContainer.Add(new IMGUIContainer(Draw));
        _outPort = CreatePort<AnimationCurve>("Out", Direction.Output);
        base.Init(pos, size);

        Data = GenerateSineWave();
        _outPort.SetData(Data);
    }

    private void Draw()
    {
        if (_params.Draw())
        {
            Data = GenerateSineWave();
            _outPort.SetData(Data);
            PropagateData();
        }

        using (new EditorGUI.DisabledScope(true)) { EditorGUILayout.CurveField(Data, GUILayout.Height(100)); }
    }

    public AnimationCurve GenerateSineWave()
    {
        var curve = new AnimationCurve();
        float step = _params.Duration / _sampleCount;
        for (var i = 0; i <= _sampleCount; ++i)
        {
            float time = i * step;
            float value = _params.Amplitude * MathF.Sin(MathConsts.Tau * _params.Frequency * time);
            curve.AddKey(time, value);
        }

        return curve;
    }
}

public enum SfxWaveform
{
    Sine,
    Pulse,
    Noise
}

[Serializable]
public class SfxMacroStep
{
    public int Ticks = 4;
    public float Frequency = 880f;
    public float Amplitude = 1f;
    public float Duty = 0.5f;
    public SfxWaveform Waveform = SfxWaveform.Pulse;
}

public class SfxMacroNode : WaveformNodeBase<AnimationCurve>
{
    private const float MinTickRate = 1f;
    private const float MaxTickRate = 240f;
    private const float MinFrequency = 20f;
    private const float MaxFrequency = 4000f;
    private const float MinAmplitude = 0f;
    private const float MaxAmplitude = 2f;
    private const float MinDuty = 0.05f;
    private const float MaxDuty = 0.95f;
    private const int SamplesPerTick = 8;

    private readonly List<SfxMacroStep> _steps = new()
    {
        new SfxMacroStep { Ticks = 4, Frequency = 880f, Amplitude = 1f, Waveform = SfxWaveform.Pulse, Duty = 0.5f },
        new SfxMacroStep { Ticks = 4, Frequency = 440f, Amplitude = 0.6f, Waveform = SfxWaveform.Pulse, Duty = 0.5f }
    };

    private float _tickRate = 60f;
    private DataPort<AnimationCurve> _outPort;

    public override void Init(Vector2 pos, Vector2 size = default)
    {
        title = "SFX Macro";
        extensionContainer.Add(new IMGUIContainer(Draw));
        _outPort = CreatePort<AnimationCurve>("Out", Direction.Output);
        RebuildCurve(false);
        _outPort.SetData(Data);
        base.Init(pos, size);
    }

    private void Draw()
    {
        var changed = false;
        EditorGUI.BeginChangeCheck();
        _tickRate = EditorGUILayout.Slider("Tick Rate", _tickRate, MinTickRate, MaxTickRate);
        if (EditorGUI.EndChangeCheck()) changed = true;

        float duration = GetTotalDuration();
        EditorGUILayout.LabelField("Duration", $"{duration:0.00}s");

        int removeIndex = -1;
        for (var i = 0; i < _steps.Count; ++i)
        {
            SfxMacroStep step = _steps[i];
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField($"Step {i + 1}");
                EditorGUI.BeginChangeCheck();
                step.Ticks = Mathf.Max(1, EditorGUILayout.IntField("Ticks", step.Ticks));
                step.Waveform = (SfxWaveform)EditorGUILayout.EnumPopup("Wave", step.Waveform);
                step.Frequency = EditorGUILayout.Slider("Freq", step.Frequency, MinFrequency, MaxFrequency);
                step.Amplitude = EditorGUILayout.Slider("Amp", step.Amplitude, MinAmplitude, MaxAmplitude);
                if (step.Waveform == SfxWaveform.Pulse)
                    step.Duty = EditorGUILayout.Slider("Duty", step.Duty, MinDuty, MaxDuty);
                if (EditorGUI.EndChangeCheck()) changed = true;

                if (GUILayout.Button("Remove Step")) removeIndex = i;
            }
        }

        if (removeIndex >= 0 && removeIndex < _steps.Count)
        {
            _steps.RemoveAt(removeIndex);
            changed = true;
        }

        if (GUILayout.Button("Add Step"))
        {
            _steps.Add(new SfxMacroStep());
            changed = true;
        }

        if (GUILayout.Button("Regenerate"))
        {
            RebuildCurve(true);
            _outPort.SetData(Data);
            PropagateData();
            return;
        }

        if (changed)
        {
            RebuildCurve(false);
            _outPort.SetData(Data);
            PropagateData();
        }
    }

    private float GetTotalDuration()
    {
        if (_tickRate <= 0f) return 0f;
        int totalTicks = _steps.Sum(s => Mathf.Max(1, s.Ticks));
        return totalTicks / _tickRate;
    }

    private void RebuildCurve(bool log)
    {
        Data = BuildCurve();
        if (log) DLog.Log($"SFX macro rebuilt ({_steps.Count} steps).");
    }

    private AnimationCurve BuildCurve()
    {
        var curve = new AnimationCurve();
        if (_steps.Count == 0 || _tickRate <= 0f) return curve;

        float tickDuration = 1f / Mathf.Max(_tickRate, MinTickRate);
        var time = 0f;

        foreach (SfxMacroStep step in _steps)
        {
            int ticks = Mathf.Max(1, step.Ticks);
            float frequency = Mathf.Clamp(step.Frequency, MinFrequency, MaxFrequency);
            float amplitude = Mathf.Clamp(step.Amplitude, MinAmplitude, MaxAmplitude);
            float duty = Mathf.Clamp(step.Duty, MinDuty, MaxDuty);

            for (var t = 0; t < ticks; ++t)
            {
                float sampleStep = tickDuration / SamplesPerTick;
                for (var s = 0; s < SamplesPerTick; ++s)
                {
                    float sampleTime = time + s * sampleStep;
                    float value = EvaluateWave(step.Waveform, sampleTime, frequency, duty) * amplitude;
                    curve.AddKey(sampleTime, value);
                }

                time += tickDuration;
            }
        }

        return curve;
    }

    private static float EvaluateWave(SfxWaveform waveform, float time, float frequency, float duty)
    {
        return waveform switch
        {
            SfxWaveform.Sine => MathF.Sin(MathConsts.Tau * frequency * time),
            SfxWaveform.Pulse => Mathf.Repeat(frequency * time, 1f) < duty ? 1f : -1f,
            SfxWaveform.Noise => Rand.FloatRanged(-1f, 1f),
            _ => 0f
        };
    }
}

public class AudioOutputNode : WaveformNodeBase<AnimationCurve>, ICleanupNode
{
    private const float MinOutputDb = -24f;
    private const float MaxOutputDb = 0f;

    public float Duration { get; set; } = 2f;

    private AudioSource _audioSource;
    private int _sampleRate = 44100;
    private DataPort<AnimationCurve> _inPort;
    private float _outputDb = -12f;
    private bool _cacheClip = true;
    private AudioClip _cachedClip;
    private int _cachedHash;

    public AudioOutputNode() { }

    public void Cleanup()
    {
        if (_audioSource != null)
        {
            Object.DestroyImmediate(_audioSource.gameObject);
            _audioSource = null;
        }

        ClearCachedClip();
    }

    public override void Init(Vector2 pos, Vector2 size = default)
    {
        title = "Audio Output Node";

        if (_audioSource == null)
        {
            GameObject go = GameObjectUtils.FindOrCreate("AudioSource");
            _audioSource = go.FindOrAddComponent<AudioSource>();
            go.hideFlags = HideFlags.HideAndDontSave;
        }

        extensionContainer.Add(new IMGUIContainer(Draw));
        _inPort = CreatePort<AnimationCurve>("In", Direction.Input);
        base.Init(pos, size);
    }

    public void PlayAudio(AnimationCurve curve)
    {
        if (_audioSource == null)
        {
            DLog.LogE("AudioSource is null.");
            return;
        }

        Data = curve;
        AudioClip clip = GetOrCreateClip(curve, !_cacheClip);
        if (clip == null)
        {
            DLog.LogW("No clip generated.");
            return;
        }

        _audioSource.clip = clip;
        _audioSource.Play();
    }

    private void Draw()
    {
        Duration = EditorGUILayout.FloatField("Duration", Duration);
        _outputDb = EditorGUILayout.Slider("Output (dB)", _outputDb, MinOutputDb, MaxOutputDb);
        _cacheClip = EditorGUILayout.Toggle("Cache Clip", _cacheClip);

        if (GUILayout.Button("Fetch data from _inPort")) FetchDataFromInputPort();

        using (new EditorGUI.DisabledScope(Data == null))
        {
            if (GUILayout.Button("Play")) PlayAudio(Data);
            if (GUILayout.Button("Save Clip")) SaveClipAsset();
        }

        using (new EditorGUI.DisabledScope(true)) { EditorGUILayout.CurveField(Data, GUILayout.Height(100)); }
    }

    private void FetchDataFromInputPort()
    {
        if (_inPort.connected)
        {
            Edge connection = _inPort.connections.FirstOrDefault();
            if (connection?.output is IDataPort<AnimationCurve> sourcePort)
            {
                SetData(sourcePort.GetData());
                DLog.Log("Fetched data from _inPort");
            }
            else { DLog.LogW("_inPort is not correctly connected."); }
        }
        else { DLog.LogW("_inPort is not connected."); }
    }

    private float[] GenerateAudioSamples(AnimationCurve curve, int sampleRate, float duration, float gain)
    {
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        var samples = new float[sampleCount];

        for (var i = 0; i < sampleCount; ++i)
        {
            float time = (float)i / sampleRate;
            samples[i] = curve.Evaluate(time) * gain;
        }

        return samples;
    }

    private static float DbToLinear(float db) => Mathf.Pow(10f, db / 20f);

    private AudioClip GetOrCreateClip(AnimationCurve curve, bool forceRebuild)
    {
        if (curve == null) return null;

        int hash = ComputeCurveHash(curve, Duration, _sampleRate, _outputDb);
        if (!forceRebuild && _cachedClip != null && _cachedHash == hash)
            return _cachedClip;

        float gain = DbToLinear(_outputDb);
        float[] samples = GenerateAudioSamples(curve, _sampleRate, Duration, gain);
        AudioClip clip = CreateAudioClipFromSamples(samples, _sampleRate);
        CacheClip(clip, hash);
        return clip;
    }

    private void CacheClip(AudioClip clip, int hash)
    {
        if (_cachedClip != null && _cachedClip != clip && !AssetDatabase.Contains(_cachedClip))
            Object.DestroyImmediate(_cachedClip);

        _cachedClip = clip;
        _cachedHash = hash;
    }

    private void ClearCachedClip()
    {
        if (_cachedClip != null && !AssetDatabase.Contains(_cachedClip))
            Object.DestroyImmediate(_cachedClip);

        _cachedClip = null;
        _cachedHash = 0;
    }

    private static int ComputeCurveHash(AnimationCurve curve, float duration, int sampleRate, float outputDb)
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 31 + sampleRate;
            hash = hash * 31 + duration.GetHashCode();
            hash = hash * 31 + outputDb.GetHashCode();

            if (curve == null) return hash;

            hash = hash * 31 + (int)curve.preWrapMode;
            hash = hash * 31 + (int)curve.postWrapMode;

            Keyframe[] keys = curve.keys;
            hash = hash * 31 + keys.Length;
            for (var i = 0; i < keys.Length; ++i)
            {
                Keyframe key = keys[i];
                hash = hash * 31 + key.time.GetHashCode();
                hash = hash * 31 + key.value.GetHashCode();
                hash = hash * 31 + key.inTangent.GetHashCode();
                hash = hash * 31 + key.outTangent.GetHashCode();
                hash = hash * 31 + key.inWeight.GetHashCode();
                hash = hash * 31 + key.outWeight.GetHashCode();
                hash = hash * 31 + (int)key.weightedMode;
            }

            return hash;
        }
    }

    private void SaveClipAsset()
    {
        if (Data == null)
        {
            DLog.LogW("No data to save.");
            return;
        }

        AudioClip clip = GetOrCreateClip(Data, false);
        if (clip == null)
        {
            DLog.LogW("Clip generation failed.");
            return;
        }

        string path = EditorUtility.SaveFilePanelInProject(
            "Save SFX Clip",
            "SfxClip",
            "asset",
            "Choose a location for the generated AudioClip.");

        if (string.IsNullOrEmpty(path)) return;

        AudioClip clipAsset = Object.Instantiate(clip);
        clipAsset.name = Path.GetFileNameWithoutExtension(path);
        AssetDatabase.CreateAsset(clipAsset, path);
        AssetDatabase.SaveAssets();
        DLog.Log($"Saved AudioClip asset to {path}");
    }

    private AudioClip CreateAudioClipFromSamples(float[] samples, int sampleRate)
    {
        var clip = AudioClip.Create("GeneratedWaveform", samples.Length, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}