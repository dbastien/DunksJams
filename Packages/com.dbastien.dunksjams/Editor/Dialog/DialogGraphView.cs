using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogGraphView : GraphView
{
    private readonly DialogCommandHistory _commandHistory;
    private readonly Dictionary<DialogGraphNode, Rect> _nodeStartPositions = new();
    private DialogGraphWindow _window;

    public DialogGraphView(DialogGraphWindow window, DialogCommandHistory commandHistory)
    {
        _window = window;
        _commandHistory = commandHistory;

        styleSheets.Add(
            AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.dbastien.dunksjams/Editor/Dialog/DialogGraph.uss"));

        // Setup zoom
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        // Add manipulators
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        this.AddManipulator(new ClickSelector());

        // Add grid background
        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();

        // Ensure view fills parent
        style.flexGrow = 1;

        // Register for node move tracking
        RegisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.TrickleDown);
        RegisterCallback<MouseUpEvent>(OnMouseUp, TrickleDown.TrickleDown);
    }

    public DialogConversation Conversation { get; private set; }

    private void OnMouseDown(MouseDownEvent evt)
    {
        // Track starting positions when mouse is pressed on selected nodes
        if (evt.button == 0) // Left mouse button
        {
            _nodeStartPositions.Clear();
            foreach (DialogGraphNode node in selection.OfType<DialogGraphNode>())
                _nodeStartPositions[node] = node.GetPosition();
        }
    }

    private void OnMouseUp(MouseUpEvent evt)
    {
        // Create move commands for nodes that moved
        if (evt.button == 0 && _nodeStartPositions.Count > 0)
        {
            foreach (KeyValuePair<DialogGraphNode, Rect> kvp in _nodeStartPositions)
            {
                DialogGraphNode node = kvp.Key;
                Rect oldPos = kvp.Value;
                Rect newPos = node.GetPosition();

                if (oldPos != newPos)
                {
                    var command = new MoveNodeCommand(Conversation, this, node.Entry, oldPos, newPos);
                    _commandHistory.ExecuteCommand(command);
                }
            }

            _nodeStartPositions.Clear();
        }
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().
            Where(endPort =>
                endPort.direction != startPort.direction &&
                endPort.node != startPort.node).
            ToList();
    }

    public void PopulateView(DialogConversation conversation)
    {
        Conversation = conversation;
        graphViewChanged -= OnGraphViewChanged;
        DeleteElements(graphElements.ToList());
        graphViewChanged += OnGraphViewChanged;

        // 1. Create Nodes
        var nodeMap = new Dictionary<string, DialogGraphNode>();
        foreach (DialogEntry entry in conversation.entries)
        {
            if (entry == null) continue;
            DialogGraphNode node = CreateNode(entry);
            nodeMap[entry.nodeID] = node;

            if (entry.nodeID == conversation.startNodeGUID)
                node.titleContainer.style.backgroundColor = new Color(0.2f, 0.4f, 0.2f, 0.8f);
        }

        // 2. Create Edges
        foreach (DialogEntry entry in conversation.entries)
        {
            DialogGraphNode sourceNode = nodeMap[entry.nodeID];
            for (var i = 0; i < entry.outgoingLinks.Count; i++)
            {
                DialogLink link = entry.outgoingLinks[i];
                if (link.destination != null &&
                    nodeMap.TryGetValue(link.destination.nodeID, out DialogGraphNode destNode))
                {
                    Port outputPort = sourceNode.OutputPorts[i];
                    Port inputPort = destNode.InputPort;

                    Edge edge = outputPort.ConnectTo(inputPort);
                    AddElement(edge);
                }
            }
        }
    }

    private DialogGraphNode CreateNode(DialogEntry entry)
    {
        var node = new DialogGraphNode(entry);
        node.SetPosition(entry.canvasRect);
        AddElement(node);

        foreach (DialogLink link in entry.outgoingLinks) node.AddChoicePort(link);

        return node;
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        if (Conversation == null) { evt.menu.AppendAction("Create Node", null, DropdownMenuAction.Status.Disabled); }
        else
        {
            Vector2 mousePosition = contentViewContainer.WorldToLocal(evt.localMousePosition);

            // Add submenu for different node types
            evt.menu.AppendAction("Create/Dialogue Node", action => CreateNewNode(mousePosition));
            evt.menu.AppendAction("Create/Choice Node", action => CreateNewNode(mousePosition, DialogNodeType.Choice));
            evt.menu.AppendAction("Create/Logic Node", action => CreateNewNode(mousePosition, DialogNodeType.Logic));
            evt.menu.AppendAction("Create/Event Node", action => CreateNewNode(mousePosition, DialogNodeType.Event));
            evt.menu.AppendSeparator("Create/");
            evt.menu.AppendAction("Create/Random Node", action => CreateNewNode(mousePosition, DialogNodeType.Random));
            evt.menu.AppendAction("Create/Jump Node", action => CreateNewNode(mousePosition, DialogNodeType.Jump));
            evt.menu.AppendAction("Create/Comment Node",
                action => CreateNewNode(mousePosition, DialogNodeType.Comment));
            evt.menu.AppendSeparator("Create/");
            evt.menu.AppendAction("Create/Start Node", action => CreateNewNode(mousePosition, DialogNodeType.Start));
            evt.menu.AppendAction("Create/End Node", action => CreateNewNode(mousePosition, DialogNodeType.End));
        }

        if (selection.Count == 1 && selection[0] is DialogGraphNode node)
        {
            evt.menu.AppendAction("Set as Start Node", action => SetStartNode(node));
            evt.menu.AppendSeparator();

            // Add "Change Type" submenu
            evt.menu.AppendAction("Change Type/Dialogue", action => ChangeNodeType(node, DialogNodeType.Dialogue));
            evt.menu.AppendAction("Change Type/Choice", action => ChangeNodeType(node, DialogNodeType.Choice));
            evt.menu.AppendAction("Change Type/Logic", action => ChangeNodeType(node, DialogNodeType.Logic));
            evt.menu.AppendAction("Change Type/Event", action => ChangeNodeType(node, DialogNodeType.Event));
            evt.menu.AppendSeparator("Change Type/");
            evt.menu.AppendAction("Change Type/Random", action => ChangeNodeType(node, DialogNodeType.Random));
            evt.menu.AppendAction("Change Type/Jump", action => ChangeNodeType(node, DialogNodeType.Jump));
            evt.menu.AppendAction("Change Type/Comment", action => ChangeNodeType(node, DialogNodeType.Comment));
            evt.menu.AppendSeparator("Change Type/");
            evt.menu.AppendAction("Change Type/Start", action => ChangeNodeType(node, DialogNodeType.Start));
            evt.menu.AppendAction("Change Type/End", action => ChangeNodeType(node, DialogNodeType.End));
        }

        base.BuildContextualMenu(evt);
    }

    private void ChangeNodeType(DialogGraphNode node, DialogNodeType newType)
    {
        node.Entry.nodeType = newType;
        EditorUtility.SetDirty(node.Entry);
        PopulateView(Conversation); // Refresh to rebuild node UI
    }

    private void SetStartNode(DialogGraphNode node)
    {
        Conversation.startNodeGUID = node.GUID;
        EditorUtility.SetDirty(Conversation);
        PopulateView(Conversation); // Refresh to update colors
    }

    public void CreateNewNode(Vector2 localPosition, DialogNodeType nodeType = DialogNodeType.Dialogue)
    {
        if (Conversation == null)
        {
            Debug.LogWarning("Cannot create node: No conversation loaded.");
            EditorUtility.DisplayDialog("Dialog Graph", "Please load a conversation asset first.", "OK");
            return;
        }

        // If position is zero, use viewport center
        Vector2 graphPosition = localPosition;
        if (localPosition == Vector2.zero)
        {
            Vector2 viewCenter = contentViewContainer.layout.center;
            graphPosition = contentViewContainer.WorldToLocal(viewCenter);
        }

        var rect = new Rect(graphPosition, new Vector2(250, 300));
        var command = new CreateNodeCommand(Conversation, this, nodeType, rect);
        _commandHistory.ExecuteCommand(command);
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {
        if (graphViewChange.elementsToRemove != null)
            foreach (GraphElement element in graphViewChange.elementsToRemove)
                if (element is DialogGraphNode node)
                {
                    var command = new DeleteNodeCommand(Conversation, this, node.Entry);
                    _commandHistory.ExecuteCommand(command);
                }
                else if (element is Edge edge)
                {
                    var sourceNode = edge.output.node as DialogGraphNode;
                    var destNode = edge.input.node as DialogGraphNode;

                    int linkIndex = sourceNode.OutputPorts.IndexOf(edge.output);
                    if (linkIndex >= 0 && linkIndex < sourceNode.Entry.outgoingLinks.Count)
                    {
                        var command = new DeleteConnectionCommand(Conversation, this, sourceNode.Entry, destNode.Entry,
                            linkIndex);
                        _commandHistory.ExecuteCommand(command);
                    }
                }

        if (graphViewChange.edgesToCreate != null)
            foreach (Edge edge in graphViewChange.edgesToCreate)
            {
                var sourceNode = edge.output.node as DialogGraphNode;
                var destNode = edge.input.node as DialogGraphNode;

                int linkIndex = sourceNode.OutputPorts.IndexOf(edge.output);
                if (linkIndex >= 0 && linkIndex < sourceNode.Entry.outgoingLinks.Count)
                {
                    var command = new CreateConnectionCommand(Conversation, this, sourceNode.Entry, destNode.Entry,
                        linkIndex);
                    _commandHistory.ExecuteCommand(command);
                }
            }

        return graphViewChange;
    }

    public void SaveToAsset(DialogConversation conversation)
    {
        foreach (DialogGraphNode node in nodes.ToList().Cast<DialogGraphNode>())
        {
            node.Entry.canvasRect = node.GetPosition();
            EditorUtility.SetDirty(node.Entry);
        }

        EditorUtility.SetDirty(conversation);
    }
}