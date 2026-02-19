using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class GenericFieldEditorWindow<T> : EditorWindow
{
    protected Vector2 scrollPos;
    protected List<Field<T>> fields = new();

    public void Init(List<Field<T>> initialFields) => fields = initialFields;

    protected virtual void OnGUI()
    {
        HandleEscapeKey();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        foreach (Field<T> field in fields) field.DrawField();
        EditorGUILayout.EndScrollView();
    }

    protected void HandleEscapeKey()
    {
        Event e = Event.current;
        if (hasFocus && e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape) Close();
    }
}

public abstract class Field<T>
{
    public string Label { get; private set; }
    public T Value { get; set; }

    public Field(string label, T initialValue)
    {
        Label = label;
        Value = initialValue;
    }

    public abstract void DrawField();
}

public class StringField : Field<string>
{
    public StringField(string label, string initialValue) : base(label, initialValue) { }

    public override void DrawField() => Value = EditorGUILayout.TextField(Label, Value);
}

public class IntField : Field<int>
{
    public IntField(string label, int initialValue) : base(label, initialValue) { }

    public override void DrawField() => Value = EditorGUILayout.IntField(Label, Value);
}

public class FloatField : Field<float>
{
    public FloatField(string label, float initialValue) : base(label, initialValue) { }

    public override void DrawField() => Value = EditorGUILayout.FloatField(Label, Value);
}

public class ObjectField<T> : Field<T> where T : Object
{
    public ObjectField(string label, T initialValue) : base(label, initialValue) { }

    public override void DrawField() => Value = (T)EditorGUILayout.ObjectField(Label, Value, typeof(T), true);
}