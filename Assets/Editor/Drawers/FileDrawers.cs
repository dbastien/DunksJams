using System;
using UnityEditor;
using UnityEngine;

public abstract class BasePathDrawer : PropertyDrawer
{
    protected abstract string GetPath();

    public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
    {
        if (prop.propertyType != SerializedPropertyType.String) throw new ArgumentException();

        rect.width -= 30;
        EditorGUI.PropertyField(rect, prop, label);

        rect.x += rect.width;
        rect.width = 30.0f;

        if (GUI.Button(rect, "…"))
        {
            string path = GetPath();
            if (string.IsNullOrEmpty(path)) return;

            if (path.StartsWith(Application.dataPath))
            {
                path = path.Substring(Application.dataPath.Length);
                path = path.Replace("/", "\\");
            }

            prop.stringValue = path;
        }
    }

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) => EditorGUIUtility.singleLineHeight;
}

[CustomPropertyDrawer(typeof(OpenLocalFileAttribute))]
public class OpenLocalFileDrawer : BasePathDrawer
{
    protected override string GetPath() => EditorUtility.OpenFilePanel("Select a file", Application.dataPath, string.Empty);
}

[CustomPropertyDrawer(typeof(OpenLocalFolderAttribute))]
public class OpenLocalFolderDrawer : BasePathDrawer
{
    protected override string GetPath() => EditorUtility.OpenFolderPanel("Select a folder", Application.dataPath, string.Empty);
}

[CustomPropertyDrawer(typeof(SaveLocalFileAttribute))]
public class SaveLocalFileDrawer : BasePathDrawer
{
    protected override string GetPath() => EditorUtility.SaveFilePanel("Select a file", Application.dataPath, string.Empty, string.Empty);
}