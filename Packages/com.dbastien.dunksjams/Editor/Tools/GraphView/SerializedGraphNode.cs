using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class SerializedGraphNode : Node, IPropagatingNode
{
    public static readonly Vector2 DefaultSize = new(200, 150);

    public virtual void Init(Vector2 pos, Vector2 size = default)
    {
        SetPosition(new Rect(pos, size == default ? DefaultSize : size));
        Refresh();
    }

    public virtual void Refresh()
    {
        RefreshPorts();
        RefreshExpandedState();
    }

    public virtual void PropagateData()
    {
        foreach (var output in outputContainer.Children().OfType<IDataPort>())
        {
            foreach (var edge in ((Port)output).connections)
                (edge.input as IDataPort)?.SetDataFromObject(output.GetDataAsObject());
        }
    }

    protected DataPort<T> CreatePort<T>(string portName, Direction dir, Action<Port> onRemove = null)
    {
        DataPort<T> port = new(this, dir, Port.Capacity.Single) { portName = portName };

        if (onRemove != null)
            port.contentContainer.Add(new Button(() => onRemove(port)) { text = "X" });

        if (dir == Direction.Output)
            outputContainer.Add(port);
        else
            inputContainer.Add(port);

        return port;
    }
}

public interface IDataPort
{
    object GetDataAsObject();
    void SetDataFromObject(object data);
}

public interface IDataPort<T> : IDataPort
{
    T GetData();
    void SetData(T data);
}

public class DataPort<T> : Port, IDataPort<T>
{
    public Node Parent { get; }
    T _data;

    public DataPort(Node parent, Direction dir, Capacity cap)
        : base(Orientation.Horizontal, dir, cap, typeof(T))
    {
        Parent = parent;
        m_EdgeConnector = new EdgeConnector<Edge>(new DefaultEdgeConnectorListener());
        this.AddManipulator(m_EdgeConnector);
    }

    public T GetData() => _data;
    public object GetDataAsObject() => _data;

    public void SetData(T data)
    {
        _data = data;
        if (Parent is IDataPort<T> dataOwner)
            dataOwner.SetData(data);
        DLog.Log($"{portName} received data: {data}");
    }

    public void SetDataFromObject(object data) => SetData((T)data);
}

public class DefaultEdgeConnectorListener : IEdgeConnectorListener
{
    public Action<Edge> OnEdgeCreated { get; set; }
    public Action<Edge> OnEdgeDroppedOutside { get; set; }

    public void OnDropOutsidePort(Edge e, Vector2 pos)
    {
        OnEdgeDroppedOutside?.Invoke(e);
        DLog.LogW("Edge dropped outside of any port.");
    }

    public void OnDrop(GraphView gv, Edge e)
    {
        if (e?.input == null || e.output == null) return;

        var edgesToCreate = new List<Edge> { e };
        var elementsToRemove = new List<GraphElement>();

        if (e.input.capacity == Port.Capacity.Single)
            elementsToRemove.AddRange(e.input.connections);
        if (e.output.capacity == Port.Capacity.Single)
            elementsToRemove.AddRange(e.output.connections);

        if (gv.graphViewChanged != null)
        {
            var change = gv.graphViewChanged(new GraphViewChange
            {
                edgesToCreate = edgesToCreate,
                elementsToRemove = elementsToRemove
            });
            edgesToCreate = change.edgesToCreate ?? edgesToCreate;
            if (change.elementsToRemove != null)
                elementsToRemove = change.elementsToRemove.ToList();
        }

        if (elementsToRemove.Count > 0)
            gv.DeleteElements(elementsToRemove);

        foreach (var edge in edgesToCreate)
        {
            gv.AddElement(edge);
            edge.input.Connect(edge);
            edge.output.Connect(edge);
            OnEdgeCreated?.Invoke(edge);
            DLog.Log($"Edge created between {edge.output.node.title} and {edge.input.node.title}");
        }
    }
}

public interface IPropagatingNode
{
    void PropagateData();
}

/// <summary>Interface for nodes that need cleanup when removed from the graph. Called by SerializedGraphView.RemoveNode() and ClearGraph().</summary>
public interface ICleanupNode
{
    void Cleanup();
}