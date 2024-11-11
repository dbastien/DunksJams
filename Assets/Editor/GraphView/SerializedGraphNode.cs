using System;
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
        foreach (var output in outputContainer.Children().OfType<DataPort<object>>())
        {
            foreach (Edge edge in output.connections)
                (edge.input as IDataPort<object>)?.SetData(output.GetData());
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

public interface IDataPort<T>
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

    public void SetData(T data)
    {
        _data = data;
        DLog.Log($"{portName} received data: {data}");
    }
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
        gv.AddElement(e);
        OnEdgeCreated?.Invoke(e);
        DLog.Log($"Edge created between {e.output.node.title} and {e.input.node.title}");
    }
}

public interface IPropagatingNode
{
    void PropagateData();
}