using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

public class WaveformEditorWindow : SerializedGraphViewEditorWindow<WaveformGraphView, WaveformNodeBase, Edge>
{
    [MenuItem("‽/Waveform Editor")] public static void ShowWindow() => GetWindow<WaveformEditorWindow>();

    protected override string GetWindowTitle() => "Waveform Editor";
    protected override string GetGraphViewName() => "WaveformGraph";
}

public class WaveformGraphView : SerializedGraphView<WaveformNodeBase, Edge>
{
    public WaveformGraphView() => graphViewChanged = OnGraphViewChanged;
    protected override string FilePath => "Assets/WaveformGraphData.json";
    public override IEnumerable<Type> GetNodeTypes() => ReflectionUtils.GetNonGenericDerivedTypes<WaveformNodeBase>();

    private GraphViewChange OnGraphViewChanged(GraphViewChange change)
    {
        if (change.edgesToCreate == null) return change;

        foreach (Edge e in change.edgesToCreate)
        {
            if (e.output is not IDataPort outPort) continue;
            (e.input as IDataPort)?.SetDataFromObject(outPort.GetDataAsObject());
            (e.input.node as IPropagatingNode)?.PropagateData();
        }

        return change;
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter adapter)
    {
        return ports.ToList().
            FindAll(endPort =>
                endPort.direction != startPort.direction &&
                endPort.node != startPort.node &&
                endPort.portType == startPort.portType);
    }
}