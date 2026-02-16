using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A single line of dialogue within a node.
/// Multiple lines can exist in one node (The "Stack" pattern).
/// </summary>
[Serializable]
public class DialogLine
{
    public string actorName;
    [TextArea(2, 5)]
    public string text;
    [TextArea(2, 5)]
    public string sequence;

    public List<Field> fields = new();

    public DialogLine() { }

    public string GetFieldValue(string name, string fallback = "") => fields.GetFieldValue(name, fallback);
    public void SetField(string name, string value, FieldType type = FieldType.Text) => fields.SetField(name, value, type);
}