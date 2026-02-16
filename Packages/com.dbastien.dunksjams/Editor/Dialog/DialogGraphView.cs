using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogGraphView : GraphView
{
    private DialogGraphWindow _window;
    private DialogConversation _conversation;
    private DialogCommandHistory _commandHistory;
    private Dictionary<DialogGraphNode, Rect> _nodeStartPositions = new Dictionary<DialogGraphNode, Rect>();

    public DialogConversation Conversation => _conversation;

        public DialogGraphView(DialogGraphWindow window, DialogCommandHistory commandHistory)
        {
            _window = window;
            _commandHistory = commandHistory;

            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.dbastien.dunksjams/Editor/Dialog/DialogGraph.uss"));

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

        private void OnMouseDown(MouseDownEvent evt)
        {
            // Track starting positions when mouse is pressed on selected nodes
            if (evt.button == 0) // Left mouse button
            {
                _nodeStartPositions.Clear();
                foreach (var node in selection.OfType<DialogGraphNode>())
                {
                    _nodeStartPositions[node] = node.GetPosition();
                }
            }
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            // Create move commands for nodes that moved
            if (evt.button == 0 && _nodeStartPositions.Count > 0)
            {
                foreach (var kvp in _nodeStartPositions)
                {
                    var node = kvp.Key;
                    var oldPos = kvp.Value;
                    var newPos = node.GetPosition();

                    if (oldPos != newPos)
                    {
                        var command = new MoveNodeCommand(_conversation, this, node.Entry, oldPos, newPos);
                        _commandHistory.ExecuteCommand(command);
                    }
                }
                _nodeStartPositions.Clear();
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(endPort =>
                endPort.direction != startPort.direction &&
                endPort.node != startPort.node).ToList();
        }

        public void PopulateView(DialogConversation conversation)
        {
            _conversation = conversation;
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;

            // 1. Create Nodes
            Dictionary<string, DialogGraphNode> nodeMap = new Dictionary<string, DialogGraphNode>();
            foreach (var entry in conversation.entries)
            {
                if (entry == null) continue;
                var node = CreateNode(entry);
                nodeMap[entry.nodeID] = node;
                
                if (entry.nodeID == conversation.startNodeGUID)
                {
                    node.titleContainer.style.backgroundColor = new Color(0.2f, 0.4f, 0.2f, 0.8f);
                }
            }

            // 2. Create Edges
            foreach (var entry in conversation.entries)
            {
                var sourceNode = nodeMap[entry.nodeID];
                for (int i = 0; i < entry.outgoingLinks.Count; i++)
                {
                    var link = entry.outgoingLinks[i];
                    if (link.destination != null && nodeMap.TryGetValue(link.destination.nodeID, out var destNode))
                    {
                        var outputPort = sourceNode.OutputPorts[i];
                        var inputPort = destNode.InputPort;

                        var edge = outputPort.ConnectTo(inputPort);
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

            foreach (var link in entry.outgoingLinks)
            {
                node.AddChoicePort(link);
            }

            return node;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (_conversation == null)
            {
                evt.menu.AppendAction("Create Node", null, DropdownMenuAction.Status.Disabled);
            }
            else
            {
                var mousePosition = contentViewContainer.WorldToLocal(evt.localMousePosition);

                // Add submenu for different node types
                evt.menu.AppendAction("Create/Dialogue Node", action => CreateNewNode(mousePosition, DialogNodeType.Dialogue));
                evt.menu.AppendAction("Create/Choice Node", action => CreateNewNode(mousePosition, DialogNodeType.Choice));
                evt.menu.AppendAction("Create/Logic Node", action => CreateNewNode(mousePosition, DialogNodeType.Logic));
                evt.menu.AppendAction("Create/Event Node", action => CreateNewNode(mousePosition, DialogNodeType.Event));
                evt.menu.AppendSeparator("Create/");
                evt.menu.AppendAction("Create/Random Node", action => CreateNewNode(mousePosition, DialogNodeType.Random));
                evt.menu.AppendAction("Create/Jump Node", action => CreateNewNode(mousePosition, DialogNodeType.Jump));
                evt.menu.AppendAction("Create/Comment Node", action => CreateNewNode(mousePosition, DialogNodeType.Comment));
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
            PopulateView(_conversation); // Refresh to rebuild node UI
        }

        private void SetStartNode(DialogGraphNode node)
        {
            _conversation.startNodeGUID = node.GUID;
            EditorUtility.SetDirty(_conversation);
            PopulateView(_conversation); // Refresh to update colors
        }

        public void CreateNewNode(Vector2 localPosition, DialogNodeType nodeType = DialogNodeType.Dialogue)
        {
            if (_conversation == null)
            {
                Debug.LogWarning("Cannot create node: No conversation loaded.");
                EditorUtility.DisplayDialog("Dialog Graph", "Please load a conversation asset first.", "OK");
                return;
            }

            // If position is zero, use viewport center
            Vector2 graphPosition = localPosition;
            if (localPosition == Vector2.zero)
            {
                var viewCenter = contentViewContainer.layout.center;
                graphPosition = contentViewContainer.WorldToLocal(viewCenter);
            }

            var rect = new Rect(graphPosition, new Vector2(250, 300));
            var command = new CreateNodeCommand(_conversation, this, nodeType, rect);
            _commandHistory.ExecuteCommand(command);
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null)
            {
                foreach (var element in graphViewChange.elementsToRemove)
                {
                    if (element is DialogGraphNode node)
                    {
                        var command = new DeleteNodeCommand(_conversation, this, node.Entry);
                        _commandHistory.ExecuteCommand(command);
                    }
                    else if (element is Edge edge)
                    {
                        var sourceNode = edge.output.node as DialogGraphNode;
                        var destNode = edge.input.node as DialogGraphNode;

                        var linkIndex = sourceNode.OutputPorts.IndexOf(edge.output);
                        if (linkIndex >= 0 && linkIndex < sourceNode.Entry.outgoingLinks.Count)
                        {
                            var command = new DeleteConnectionCommand(_conversation, this, sourceNode.Entry, destNode.Entry, linkIndex);
                            _commandHistory.ExecuteCommand(command);
                        }
                    }
                }
            }

            if (graphViewChange.edgesToCreate != null)
            {
                foreach (var edge in graphViewChange.edgesToCreate)
                {
                    var sourceNode = edge.output.node as DialogGraphNode;
                    var destNode = edge.input.node as DialogGraphNode;

                    var linkIndex = sourceNode.OutputPorts.IndexOf(edge.output);
                    if (linkIndex >= 0 && linkIndex < sourceNode.Entry.outgoingLinks.Count)
                    {
                        var command = new CreateConnectionCommand(_conversation, this, sourceNode.Entry, destNode.Entry, linkIndex);
                        _commandHistory.ExecuteCommand(command);
                    }
                }
            }

            return graphViewChange;
        }

        public void SaveToAsset(DialogConversation conversation)
        {
            foreach (var node in nodes.ToList().Cast<DialogGraphNode>())
            {
                node.Entry.canvasRect = node.GetPosition();
                EditorUtility.SetDirty(node.Entry);
            }
            EditorUtility.SetDirty(conversation);
        }
    }