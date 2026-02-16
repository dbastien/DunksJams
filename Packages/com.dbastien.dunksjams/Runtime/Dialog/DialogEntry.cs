using System;
using System.Collections.Generic;
using UnityEngine;

public enum DialogNodeType
{
    Dialogue,    // Standard NPC dialogue with lines
    Choice,      // Player choice branch point
    Logic,       // Pure logic/conditions, no dialogue
    Event,       // Triggers game events
    Start,       // Conversation start point
    End,         // Conversation end point
    Comment,     // Documentation/notes (non-functional)
    Random,      // Picks random output path
    Jump         // Jumps to another node or conversation
}

[Serializable]
public class DialogLink
{
    public string guid;
    public string text; // For player choices
    public DialogEntry destination;
    public string condition;
    public int priority;
    public string portName;
    public List<Field> fields = new();

    public DialogLink()
    {
        guid = Guid.NewGuid().ToString();
    }
}

/// <summary>
/// A node in the dialogue graph. Can contain a "Stack" of dialogue lines.
/// Inherits from ScriptableObject to allow for sub-asset persistence.
/// </summary>
public class DialogEntry : ScriptableObject
{
    public string nodeID; // Unique ID/GUID
    public Rect canvasRect; // Position in graph
    public DialogNodeType nodeType = DialogNodeType.Dialogue;

    // The "Stack" of lines
    public List<DialogLine> lines = new();

    // Outgoing connections
    public List<DialogLink> outgoingLinks = new();

    // Logic
    public string condition;
    public string onExecuteScript;

    // Metadata
    public List<Field> fields = new();

    public void Initialize(string guid, DialogNodeType type = DialogNodeType.Dialogue)
    {
        nodeID = guid;
        nodeType = type;
        name = type + "_" + guid.Substring(0, 8);
    }

    public string GetFieldValue(string name, string fallback = "") => fields.GetFieldValue(name, fallback);
    public void SetField(string name, string value, FieldType type = FieldType.Text) => fields.SetField(name, value, type);
}