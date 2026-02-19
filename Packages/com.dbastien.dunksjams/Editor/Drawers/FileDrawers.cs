using System;
using UnityEditor;
using UnityEngine;

public abstract class BasePathDrawer : PropertyDrawer
{
    protected abstract string GetPath();

    public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
    {
        if (prop.propertyType != SerializedPropertyType.String) throw new ArgumentException();

        var btnWidth = 30f;
        EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width - btnWidth, rect.height), prop, label);

        if (GUI.Button(new Rect(rect.x + rect.width - btnWidth, rect.y, btnWidth, rect.height), "…"))
        {
            string path = GetPath();
            if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
                prop.stringValue = path[(Application.dataPath.Length + 1)..].Replace("/", "\\");
        }
    }

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) =>
        EditorGUIUtility.singleLineHeight;
}

[CustomPropertyDrawer(typeof(OpenLocalFileAttribute))]
public class OpenLocalFileDrawer : BasePathDrawer
{
    protected override string GetPath() => EditorUtility.OpenFilePanel("Select a file", Application.dataPath, "");
}

[CustomPropertyDrawer(typeof(OpenLocalFolderAttribute))]
public class OpenLocalFolderDrawer : BasePathDrawer
{
    protected override string GetPath() => EditorUtility.OpenFolderPanel("Select a folder", Application.dataPath, "");
}

[CustomPropertyDrawer(typeof(SaveLocalFileAttribute))]
public class SaveLocalFileDrawer : BasePathDrawer
{
    protected override string GetPath() => EditorUtility.SaveFilePanel("Select a file", Application.dataPath, "", "");
}