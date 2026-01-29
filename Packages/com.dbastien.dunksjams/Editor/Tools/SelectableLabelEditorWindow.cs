using System;
using UnityEditor;
using UnityEngine;

//todo: handle things other than just labels like object fields
public class SelectableLabelEditorWindow : EditorWindow
{
    protected Vector2 scrollPos;
    protected string text;
    protected GUIContent content;
    protected Vector2 size;

    public void Init(string label)
    {
        text = label;
        content = new(text);
        size = EditorStyles.textField.CalcSize(content);
    }

    void OnGUI()
    {
        Event e = Event.current;
        if (hasFocus && e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
        {
            Close();
            return;
        }
        
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        EditorGUILayout.SelectableLabel
        (
            text,
            EditorStyles.textArea,
            GUILayout.ExpandHeight(true),
            GUILayout.MinHeight(size.y),
            GUILayout.MinWidth(MathF.Max(size.x, position.width - 20)) // Adjust width dynamically
            //GUILayout.MinWidth(size.x)
        );
        EditorGUILayout.EndScrollView();
    }
}