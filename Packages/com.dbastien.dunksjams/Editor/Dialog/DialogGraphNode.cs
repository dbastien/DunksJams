using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogGraphNode : Node
{
    private VisualElement _linesContainer;
    public DialogEntry Entry;
    public string GUID;
    public Port InputPort;
    public List<Port> OutputPorts = new();

    public DialogGraphNode(DialogEntry entry)
    {
        Entry = entry;
        GUID = entry.nodeID;

        // Set a more readable title
        UpdateTitle();

        // Set position from entry
        SetPosition(entry.canvasRect);

        // Add ports based on node type
        BuildPorts();

        // Build UI based on node type
        BuildNodeUI();

        RefreshExpandedState();
    }

    private void UpdateTitle()
    {
        // Show type icon and a preview of content
        string icon = Entry.nodeType switch
        {
            DialogNodeType.Dialogue => "💬",
            DialogNodeType.Choice => "🔀",
            DialogNodeType.Logic => "⚙",
            DialogNodeType.Event => "⚡",
            DialogNodeType.Start => "▶",
            DialogNodeType.End => "⏹",
            DialogNodeType.Comment => "📝",
            DialogNodeType.Random => "🎲",
            DialogNodeType.Jump => "↗",
            _ => "•"
        };

        // Show first line of dialogue as preview, or just type name
        var preview = "";
        if (Entry.lines.Count > 0 && !string.IsNullOrEmpty(Entry.lines[0].text))
            preview = Entry.lines[0].text.Length > 30
                ? Entry.lines[0].text[..30] + "..."
                : Entry.lines[0].text;
        else
            preview = Entry.nodeType.ToString();

        title = $"{icon} {preview}";
    }

    private void BuildPorts()
    {
        // Input port (all except Start have input)
        if (Entry.nodeType != DialogNodeType.Start)
        {
            InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            InputPort.portName = "In";
            InputPort.tooltip = "Connect from another node's output port";
            inputContainer.Add(InputPort);
        }

        // Output ports based on existing links
        foreach (DialogLink link in Entry.outgoingLinks) AddChoicePort(link);

        // Auto-add default output for non-End nodes
        if (Entry.nodeType != DialogNodeType.End &&
            Entry.nodeType != DialogNodeType.Comment &&
            Entry.outgoingLinks.Count == 0)
        {
            var defaultLink = new DialogLink { text = "Next" };
            Entry.outgoingLinks.Add(defaultLink);
            AddChoicePort(defaultLink);
        }
    }

    private void BuildNodeUI()
    {
        // Comment node: Just show text field
        if (Entry.nodeType == DialogNodeType.Comment)
        {
            var commentField = new TextField("Comment")
            {
                value = Entry.lines.Count > 0 ? Entry.lines[0].text : "",
                tooltip = "Add notes or documentation for this conversation",
                multiline = true
            };
            commentField.style.whiteSpace = WhiteSpace.Normal;
            commentField.style.minHeight = 60;
            commentField.RegisterValueChangedCallback(evt =>
            {
                if (Entry.lines.Count == 0) Entry.lines.Add(new DialogLine());
                Entry.lines[0].text = evt.newValue;
                UpdateTitle();
            });
            extensionContainer.Add(commentField);
            return;
        }

        // Jump node: Show target field
        if (Entry.nodeType == DialogNodeType.Jump)
        {
            var jumpField = new TextField("Jump To")
            {
                value = Entry.condition, // Reuse condition field for jump target
                tooltip = "Node ID or conversation name to jump to"
            };
            jumpField.RegisterValueChangedCallback(evt => Entry.condition = evt.newValue);
            extensionContainer.Add(jumpField);
        }

        // Condition/Script fields (show for Logic, Event, Random, Jump, and nodes that have values)
        if (Entry.nodeType == DialogNodeType.Logic ||
            Entry.nodeType == DialogNodeType.Event ||
            Entry.nodeType == DialogNodeType.Random ||
            !string.IsNullOrEmpty(Entry.condition) ||
            !string.IsNullOrEmpty(Entry.onExecuteScript))
        {
            var conditionField = new TextField("Condition")
            {
                value = Entry.condition,
                tooltip = "Condition to check before executing this node (e.g., 'hasKey==true')"
            };
            conditionField.RegisterValueChangedCallback(evt => Entry.condition = evt.newValue);
            extensionContainer.Add(conditionField);

            var scriptField = new TextField("Script")
            {
                value = Entry.onExecuteScript,
                tooltip = "Script to execute when this node runs (e.g., 'questComplete=true')"
            };
            scriptField.RegisterValueChangedCallback(evt => Entry.onExecuteScript = evt.newValue);
            extensionContainer.Add(scriptField);
        }

        // Lines container (show for Dialogue, Choice, Start, End types)
        if (Entry.nodeType == DialogNodeType.Dialogue ||
            Entry.nodeType == DialogNodeType.Choice ||
            Entry.nodeType == DialogNodeType.Start ||
            Entry.nodeType == DialogNodeType.End ||
            Entry.lines.Count > 0)
        {
            _linesContainer = new VisualElement();
            _linesContainer.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.5f);
            _linesContainer.style.paddingTop = 5;
            _linesContainer.style.paddingBottom = 5;
            extensionContainer.Add(_linesContainer);

            // Initialize with default line for dialogue nodes
            if (Entry.lines.Count == 0 &&
                (Entry.nodeType == DialogNodeType.Dialogue || Entry.nodeType == DialogNodeType.Start))
                Entry.lines.Add(new DialogLine { actorName = "Actor", text = "Dialogue..." });
            RefreshLines();
        }

        // Buttons for adding things
        var buttonContainer = new VisualElement();
        buttonContainer.style.flexDirection = FlexDirection.Row;
        buttonContainer.style.justifyContent = Justify.Center;
        buttonContainer.style.paddingTop = 5;
        buttonContainer.style.paddingBottom = 5;

        // Add Line button (for Dialogue/Choice/Start/End)
        if (Entry.nodeType != DialogNodeType.Logic &&
            Entry.nodeType != DialogNodeType.Event &&
            Entry.nodeType != DialogNodeType.Comment &&
            Entry.nodeType != DialogNodeType.Jump &&
            Entry.nodeType != DialogNodeType.Random)
        {
            var addLineButton = new Button(AddLine)
            {
                text = "Add Line",
                tooltip = "Add another line of dialogue to this node"
            };
            addLineButton.style.flexGrow = 1;
            buttonContainer.Add(addLineButton);
        }

        // Add Choice button (for all except End and Comment)
        if (Entry.nodeType != DialogNodeType.End && Entry.nodeType != DialogNodeType.Comment)
        {
            var addChoiceButton = new Button(AddChoice)
            {
                text = "Add Choice",
                tooltip = "Add an output port for branching dialogue or player choices"
            };
            addChoiceButton.style.flexGrow = 1;
            buttonContainer.Add(addChoiceButton);
        }

        if (buttonContainer.childCount > 0) mainContainer.Add(buttonContainer);

        // Apply node type styling
        ApplyNodeTypeStyle();
    }

    private void ApplyNodeTypeStyle()
    {
        Color titleColor = Entry.nodeType switch
        {
            DialogNodeType.Dialogue => new Color(0.2f, 0.3f, 0.5f, 0.8f),
            DialogNodeType.Choice => new Color(0.5f, 0.3f, 0.2f, 0.8f),
            DialogNodeType.Logic => new Color(0.3f, 0.2f, 0.5f, 0.8f),
            DialogNodeType.Event => new Color(0.5f, 0.4f, 0.2f, 0.8f),
            DialogNodeType.Start => new Color(0.2f, 0.5f, 0.2f, 0.8f),
            DialogNodeType.End => new Color(0.5f, 0.2f, 0.2f, 0.8f),
            DialogNodeType.Comment => new Color(0.4f, 0.4f, 0.3f, 0.8f),
            DialogNodeType.Random => new Color(0.4f, 0.2f, 0.5f, 0.8f),
            DialogNodeType.Jump => new Color(0.2f, 0.4f, 0.5f, 0.8f),
            _ => new Color(0.3f, 0.3f, 0.3f, 0.8f)
        };

        titleContainer.style.backgroundColor = titleColor;
    }

    private void RebuildNode()
    {
        // Clear existing UI
        extensionContainer.Clear();
        mainContainer.Clear();

        // Rebuild
        BuildNodeUI();
        RefreshExpandedState();
    }

    private void AddLine()
    {
        Entry.lines.Add(new DialogLine { actorName = "Actor", text = "Dialogue..." });
        RefreshLines();
        UpdateTitle();
    }

    private void RefreshLines()
    {
        _linesContainer.Clear();
        for (var i = 0; i < Entry.lines.Count; i++)
        {
            DialogLine line = Entry.lines[i];
            int lineIndex = i;

            var lineBox = new VisualElement();
            lineBox.style.flexDirection = FlexDirection.Column;
            lineBox.style.marginBottom = 5;
            lineBox.style.paddingLeft = 5;
            lineBox.style.paddingRight = 5;
            lineBox.style.borderBottomWidth = 1;
            lineBox.style.borderBottomColor = Color.gray;

            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;

            var actorField = new TextField
            {
                value = line.actorName,
                label = "Actor",
                tooltip = "Name of the character speaking this line"
            };
            actorField.style.flexGrow = 1;
            actorField.RegisterValueChangedCallback(evt => line.actorName = evt.newValue);
            header.Add(actorField);

            var removeBtn = new Button(() =>
            {
                Entry.lines.RemoveAt(lineIndex);
                RefreshLines();
                UpdateTitle();
            })
            {
                text = "X",
                tooltip = "Remove this dialogue line"
            };
            header.Add(removeBtn);
            lineBox.Add(header);

            var textField = new TextField
            {
                value = line.text,
                multiline = true,
                tooltip = "The dialogue text. Supports tags like [em1]emphasis[/em1] and [pause=0.5]"
            };
            textField.RegisterValueChangedCallback(evt =>
            {
                line.text = evt.newValue;
                if (lineIndex == 0) UpdateTitle(); // Update title when first line changes
            });
            lineBox.Add(textField);

            var seqField = new TextField
            {
                value = line.sequence,
                label = "Seq",
                tooltip = "Sequencer commands (e.g., 'Camera(actor,2); Fade(in,1)')"
            };
            seqField.RegisterValueChangedCallback(evt => line.sequence = evt.newValue);
            lineBox.Add(seqField);

            // Add to container
            _linesContainer.Add(lineBox);
        }
    }

    private void AddChoice()
    {
        var link = new DialogLink { text = "Choice Text" };
        Entry.outgoingLinks.Add(link);
        AddChoicePort(link);
    }

    public void AddChoicePort(DialogLink link)
    {
        Port port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
        port.portName = string.IsNullOrEmpty(link.text) ? "Next" : link.text;
        port.tooltip = "Drag to another node's input port to connect";

        // Allow editing choice text
        var textField = new TextField
        {
            value = link.text,
            tooltip = "Text for this choice/branch. Leave empty for automatic 'Next'"
        };
        textField.RegisterValueChangedCallback(evt =>
        {
            link.text = evt.newValue;
            port.portName = string.IsNullOrEmpty(evt.newValue) ? "Next" : evt.newValue;
        });
        port.Add(textField);

        var removeBtn = new Button(() =>
        {
            Entry.outgoingLinks.Remove(link);
            OutputPorts.Remove(port);
            outputContainer.Remove(port);
            RefreshPorts();
        })
        {
            text = "X",
            tooltip = "Remove this output connection"
        };
        port.Add(removeBtn);

        outputContainer.Add(port);
        OutputPorts.Add(port);
        RefreshPorts();
    }
}