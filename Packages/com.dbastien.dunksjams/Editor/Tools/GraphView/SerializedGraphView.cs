using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class SerializedGraphView<TNode, TEdge> : GraphView
    where TNode : SerializedGraphNode, new()
    where TEdge : Edge, new()
{
    protected readonly Dictionary<string, Node> _nodes = new();
    protected SerializedGraphView() => Setup();
    protected abstract string FilePath { get; }
    public virtual IEnumerable<Type> GetNodeTypes() => ReflectionUtils.GetNonGenericDerivedTypes<TNode>();

    private void Setup()
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        Insert(0, new GridBackground());
        AddManipulators();
    }

    private void AddManipulators()
    {
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        this.AddManipulator(new ContextualMenuManipulator(BuildMenu));
    }

    private void BuildMenu(ContextualMenuPopulateEvent evt)
    {
        Vector2 pos = contentViewContainer.WorldToLocal(evt.mousePosition);
        foreach (Type type in GetNodeTypes())
            evt.menu.AppendAction($"Create {type.Name}", _ => AddNode(type, pos));
    }

    public void AddNode(Type type, Vector2 pos)
    {
        var n = (TNode)Activator.CreateInstance(type);
        n.viewDataKey = Guid.NewGuid().ToString();
        n.Init(pos);
        AddElement(n);
        _nodes[n.viewDataKey] = n;
    }

    public void RemoveNode(TNode n)
    {
        (n as ICleanupNode)?.Cleanup();
        RemoveElement(n);
        _nodes.Remove(n.viewDataKey);
    }

    public void AddEdge(Port output, Port input)
    {
        TEdge e = new() { output = output, input = input };
        output.Connect(e);
        input.Connect(e);
        AddElement(e);
    }

    public void RemoveEdge(TEdge e)
    {
        e.output?.Disconnect(e);
        e.input?.Disconnect(e);
        RemoveElement(e);
    }

    public void SaveGraph()
    {
        if (string.IsNullOrEmpty(FilePath)) return;

        var gd = new GraphData
        {
            nodes = nodes.Select(SerializeNode).Where(n => n != null).ToList(),
            edges = edges.Where(e => e.input != null && e.output != null).Select(SerializeEdge).ToList()
        };

        File.WriteAllText(FilePath, JsonUtility.ToJson(gd, true));
        DLog.Log($"Graph saved to {FilePath}");
    }

    public void LoadGraph()
    {
        if (!File.Exists(FilePath)) return;
        ClearGraph();

        var data = JsonUtility.FromJson<GraphData>(File.ReadAllText(FilePath));
        _nodes.Clear();

        foreach (NodeData nd in data.nodes) InstantiateNode(nd);
        foreach (EdgeData ed in data.edges) InstantiateEdge(ed);
    }

    protected NodeData SerializeNode(Node n)
    {
        var nd = new NodeData
        {
            guid = n.viewDataKey,
            nodeType = n.GetType().AssemblyQualifiedName,
            pos = n.GetPosition().position
        };

        foreach (FieldInfo fi in n.GetType().GetSerializableFields())
            nd.fields.Add(new FieldData { name = fi.Name, val = fi.GetValue(n)?.ToString() ?? "" });

        return nd;
    }

    protected EdgeData SerializeEdge(Edge e) => new()
    {
        sourceNodeGUID = e.output.node.viewDataKey,
        sourcePortName = e.output.portName,
        targetNodeGUID = e.input.node.viewDataKey,
        targetPortName = e.input.portName
    };

    protected void InstantiateNode(NodeData nd)
    {
        Node n = CreateNodeFromType(nd.nodeType);
        if (n == null)
        {
            DLog.LogW($"Node type '{nd.nodeType}' not found.");
            return;
        }

        n.viewDataKey = nd.guid;
        n.SetPosition(new Rect(nd.pos, SerializedGraphNode.DefaultSize));
        _nodes[n.viewDataKey] = n;

        foreach (FieldData fd in nd.fields)
        {
            FieldInfo field = n.GetType().GetField(fd.name, ReflectionUtils.AllInstance);
            if (field == null) continue;
            try
            {
                object val = ReflectionUtils.ConvertToType(fd.val, field.FieldType);
                field.SetValue(n, val);
            }
            catch (Exception ex)
            {
                DLog.LogW($"Failed to set field '{fd.name}' on node '{nd.nodeType}': {ex.Message}");
            }
        }

        // Initialize the node (creates ports, UI, etc.)
        if (n is SerializedGraphNode sgn)
            sgn.Init(nd.pos);

        AddElement(n);
    }

    protected Node CreateNodeFromType(string typeName)
    {
        var type = Type.GetType(typeName);
        if (type != null && typeof(Node).IsAssignableFrom(type))
            return (Node)Activator.CreateInstance(type);
        DLog.LogW($"Node type '{typeName}' not found or is not assignable to Node.");
        return null;
    }

    protected void InstantiateEdge(EdgeData ed)
    {
        if (!_nodes.TryGetValue(ed.sourceNodeGUID, out Node srcNode) ||
            !_nodes.TryGetValue(ed.targetNodeGUID, out Node tgtNode))
        {
            DLog.LogW($"Could not find nodes for edge: {ed.sourceNodeGUID} -> {ed.targetNodeGUID}");
            return;
        }

        Port srcPort = GetPortByName(srcNode.outputContainer, ed.sourcePortName);
        Port tgtPort = GetPortByName(tgtNode.inputContainer, ed.targetPortName);

        if (srcPort != null && tgtPort != null)
            AddEdge(srcPort, tgtPort);
        else
            DLog.LogW($"Could not find ports for edge: {ed.sourceNodeGUID} -> {ed.targetNodeGUID}");
    }

    private Port GetPortByName(VisualElement container, string nme) =>
        container.Children().OfType<Port>().FirstOrDefault(p => p.portName == nme);

    private void ClearGraph()
    {
        foreach (Node n in nodes)
        {
            (n as ICleanupNode)?.Cleanup();
            RemoveElement(n);
        }

        foreach (Edge e in edges) RemoveElement(e);
        _nodes.Clear();
    }
}