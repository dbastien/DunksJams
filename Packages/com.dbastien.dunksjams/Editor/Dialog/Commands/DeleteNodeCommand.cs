using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Command to delete a dialog node.
/// Stores full node state including connections for undo.
/// </summary>
public class DeleteNodeCommand : DialogGraphCommand
{
    private readonly DialogConversation _conversation;
    private readonly DialogGraphView _graphView;
    private readonly DialogEntry _entry;
    private readonly string _nodeID;
    private readonly DialogNodeType _nodeType;
    private readonly Rect _canvasRect;
    private readonly List<DialogLine> _lines;
    private readonly List<DialogLink> _outgoingLinks;
    private readonly string _condition;
    private readonly string _onExecuteScript;
    private readonly List<Field> _fields;

    // Store incoming connections (other nodes pointing to this one)
    private readonly List<(string sourceNodeID, int linkIndex)> _incomingConnections = new();

    public override string Name => $"Delete {_nodeType} Node";

    public DeleteNodeCommand(DialogConversation conversation, DialogGraphView graphView, DialogEntry entry)
    {
        _conversation = conversation;
        _graphView = graphView;
        _entry = entry;

        // Store full state for undo
        _nodeID = entry.nodeID;
        _nodeType = entry.nodeType;
        _canvasRect = entry.canvasRect;
        _condition = entry.condition;
        _onExecuteScript = entry.onExecuteScript;

        // Deep copy lists
        _lines = new List<DialogLine>(entry.lines.Select(line => new DialogLine
        {
            actorName = line.actorName,
            text = line.text,
            sequence = line.sequence,
            fields = new List<Field>(line.fields)
        }));

        _outgoingLinks = new List<DialogLink>(entry.outgoingLinks.Select(link => new DialogLink
        {
            guid = link.guid,
            text = link.text,
            condition = link.condition,
            priority = link.priority,
            portName = link.portName,
            destination = link.destination,
            fields = new List<Field>(link.fields)
        }));

        _fields = new List<Field>(entry.fields);

        // Find all incoming connections
        foreach (DialogEntry otherEntry in conversation.entries)
        {
            if (otherEntry == entry) continue;

            for (var i = 0; i < otherEntry.outgoingLinks.Count; i++)
                if (otherEntry.outgoingLinks[i].destination == entry)
                    _incomingConnections.Add((otherEntry.nodeID, i));
        }
    }

    public override void Execute()
    {
        // Remove all incoming connections
        foreach ((string sourceNodeID, int linkIndex) in _incomingConnections)
        {
            DialogEntry sourceEntry = _conversation.GetEntry(sourceNodeID);
            if (sourceEntry != null && linkIndex < sourceEntry.outgoingLinks.Count)
            {
                sourceEntry.outgoingLinks[linkIndex].destination = null;
                EditorUtility.SetDirty(sourceEntry);
            }
        }

        // Remove from conversation
        _conversation.entries.Remove(_entry);
        AssetDatabase.RemoveObjectFromAsset(_entry);

        EditorUtility.SetDirty(_conversation);
        AssetDatabase.SaveAssets();

        // Refresh graph view
        _graphView.PopulateView(_conversation);
    }

    public override void Undo()
    {
        // Recreate the entry
        var entry = ScriptableObject.CreateInstance<DialogEntry>();
        entry.Initialize(_nodeID, _nodeType);
        entry.canvasRect = _canvasRect;
        entry.condition = _condition;
        entry.onExecuteScript = _onExecuteScript;

        // Restore lines
        entry.lines.Clear();
        foreach (DialogLine line in _lines)
            entry.lines.Add(new DialogLine
            {
                actorName = line.actorName,
                text = line.text,
                sequence = line.sequence,
                fields = new List<Field>(line.fields)
            });

        // Restore outgoing links
        entry.outgoingLinks.Clear();
        foreach (DialogLink link in _outgoingLinks)
            entry.outgoingLinks.Add(new DialogLink
            {
                guid = link.guid,
                text = link.text,
                condition = link.condition,
                priority = link.priority,
                portName = link.portName,
                destination = link.destination,
                fields = new List<Field>(link.fields)
            });

        // Restore fields
        entry.fields.Clear();
        entry.fields.AddRange(_fields);

        // Add back to conversation
        AssetDatabase.AddObjectToAsset(entry, _conversation);
        _conversation.entries.Add(entry);

        // Restore incoming connections
        foreach ((string sourceNodeID, int linkIndex) in _incomingConnections)
        {
            DialogEntry sourceEntry = _conversation.GetEntry(sourceNodeID);
            if (sourceEntry != null && linkIndex < sourceEntry.outgoingLinks.Count)
            {
                sourceEntry.outgoingLinks[linkIndex].destination = entry;
                EditorUtility.SetDirty(sourceEntry);
            }
        }

        EditorUtility.SetDirty(entry);
        EditorUtility.SetDirty(_conversation);
        AssetDatabase.SaveAssets();

        // Refresh graph view
        _graphView.PopulateView(_conversation);
    }
}