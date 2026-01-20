using System;
using UnityEditor;

public struct EditorGUIIndentLevelScope : IDisposable
{
    readonly int _prev;
    public EditorGUIIndentLevelScope(int indent)
    {
        _prev = EditorGUI.indentLevel;
        EditorGUI.indentLevel = indent;
    }
    public void Dispose() => EditorGUI.indentLevel = _prev;
}
