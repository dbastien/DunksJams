using System;
using UnityEditor;
using UnityEngine;

public class DropdownAttribute : PropertyAttribute
{
    public string[] Options;
    public DropdownAttribute(params string[] options) => Options = options;
}

[CustomPropertyDrawer(typeof(DropdownAttribute))]
public class DropdownDrawer : PropertyDrawer
{
    public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
    {
        var dropdownAttr = (DropdownAttribute)attribute;
        int index = Mathf.Max(0, Array.IndexOf(dropdownAttr.Options, prop.stringValue));
        index = EditorGUI.Popup(rect, label.text, index, dropdownAttr.Options);
        prop.stringValue = dropdownAttr.Options[index];
    }
}