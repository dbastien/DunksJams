using System;
using System.Linq;
using UnityEditor; 
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public class WaveformParameters
{
    public float Amplitude { get; set; } = 1f;
    public float Frequency { get; set; } = 440f;
    public float Duration { get; set; } = 1f;

    public bool Draw()
    {
        bool changed = false;
        EditorGUI.BeginChangeCheck();
        Amplitude = EditorGUILayout.FloatField("Amplitude", Amplitude);
        Frequency = EditorGUILayout.FloatField("Frequency", Frequency);
        Duration = EditorGUILayout.FloatField("Duration", Duration);
        if (EditorGUI.EndChangeCheck()) changed = true;
        return changed;
    }
}

public class WaveformNodeBase : SerializedGraphNode
{
}

public abstract class WaveformNodeBase<T> : WaveformNodeBase, IDataPort<T>
{
    protected T Data;

    public T GetData() => Data;
    public void SetData(T data) => Data = data;

    public override void PropagateData()
    {
        foreach (var output in outputContainer.Children().OfType<DataPort<T>>())
        {
            foreach (Edge edge in output.connections)
                if (edge.input is IDataPort<T> inputPort)
                    inputPort.SetData(Data);
        }
    }
}

public class AnimationCurveNode : WaveformNodeBase<AnimationCurve>
{
    readonly WaveformParameters _params = new();
    DataPort<AnimationCurve> _outPort;

    public override void Init(Vector2 pos, Vector2 size = default)
    {
        title = "Animation Curve";
        extensionContainer.Add(new IMGUIContainer(Draw));
        _outPort = CreatePort<AnimationCurve>("Out", Direction.Output);
        outputContainer.Add(_outPort);
        Data = new AnimationCurve();
        base.Init(pos, size);
    }

    void Draw()
    {
        if (_params.Draw()) PropagateData();
        Data = EditorGUILayout.CurveField(Data, GUILayout.Height(100));
    }
}

public class SineWaveNode : WaveformNodeBase<AnimationCurve>
{
    WaveformParameters _params = new();
    int _sampleCount = 100;
    DataPort<AnimationCurve> _outPort;

    public SineWaveNode() { }

    public override void Init(Vector2 pos, Vector2 size = default)
    {
        title = "Sine Wave";
        extensionContainer.Add(new IMGUIContainer(Draw));
        _outPort = CreatePort<AnimationCurve>("Out", Direction.Output);
        outputContainer.Add(_outPort);
        base.Init(pos, size);

        Data = GenerateSineWave();
    }

    void Draw()
    {
        if (_params.Draw())
        {
            Data = GenerateSineWave();
            PropagateData();
        }
        using (new EditorGUI.DisabledScope(true)) EditorGUILayout.CurveField(Data, GUILayout.Height(100));
    }

    public AnimationCurve GenerateSineWave()
    {
        var curve = new AnimationCurve();
        float step = _params.Duration / _sampleCount;
        for (int i = 0; i <= _sampleCount; ++i)
        {
            float time = i * step;
            float value = _params.Amplitude * MathF.Sin(MathConsts.Tau * _params.Frequency * time);
            curve.AddKey(time, value);
        }
        return curve;
    }
}

public class AudioOutputNode : WaveformNodeBase<AnimationCurve>
{
    public float Duration { get; set; } = 2f;

    AudioSource _audioSource;
    int _sampleRate = 44100;
    DataPort<AnimationCurve> _inPort;

    public AudioOutputNode() { }

    ~AudioOutputNode()
    {
        if (_audioSource != null) Object.DestroyImmediate(_audioSource.gameObject);
    }

    //todo: maybe add a cleanup to all our nodes and then in this one we destroy the audio source

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
        inputContainer.Add(_inPort);
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
        var samples = GenerateAudioSamples(curve, _sampleRate, Duration);
        _audioSource.clip = CreateAudioClipFromSamples(samples, _sampleRate);
        _audioSource.Play();
    }

    void Draw()
    {
        Duration = EditorGUILayout.FloatField("Duration", Duration);

        if (GUILayout.Button("Fetch data from _inPort")) FetchDataFromInputPort();

        if (GUILayout.Button("Play") && Data != null) PlayAudio(Data);
        using (new EditorGUI.DisabledScope(true)) EditorGUILayout.CurveField(Data, GUILayout.Height(100));
    }

    void FetchDataFromInputPort()
    {
        if (_inPort.connected)
        {
            var connection = _inPort.connections.FirstOrDefault();
            if (connection?.output is IDataPort<AnimationCurve> sourcePort)
            {
                SetData(sourcePort.GetData());
                DLog.Log("Fetched data from _inPort");
            }
            else
                DLog.LogW("_inPort is not correctly connected.");
        }
        else
            DLog.LogW("_inPort is not connected.");
    }

    float[] GenerateAudioSamples(AnimationCurve curve, int sampleRate, float duration)
    {
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        var samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; ++i)
        {
            float time = (float)i / sampleRate;
            samples[i] = curve.Evaluate(time);
        }

        return samples;
    }

    AudioClip CreateAudioClipFromSamples(float[] samples, int sampleRate)
    {
        var clip = AudioClip.Create("GeneratedWaveform", samples.Length, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}