using System.Linq;
using UnityEditor; 
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

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