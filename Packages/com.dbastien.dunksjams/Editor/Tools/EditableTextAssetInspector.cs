using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoScript))]
[CanEditMultipleObjects]
public class MonoScriptEditor : EditableTextAssetInspector
{
    protected override string GetFilePath(Object obj) => AssetDatabase.GetAssetPath((MonoScript)obj);
}

[CustomEditor(typeof(TextAsset))]
[CanEditMultipleObjects]
public class TextAssetEditor : EditableTextAssetInspector
{
    protected override string GetFilePath(Object obj) => AssetDatabase.GetAssetPath((TextAsset)obj);
}

public abstract class EditableTextAssetInspector : Editor
{
    string _filePath;
    string _currentContent;
    bool _focused;

    void OnEnable()
    {
        _filePath = GetFilePath(target);
        _currentContent = ReadFileSafe(_filePath);
    }

    protected abstract string GetFilePath(Object obj);

    public override bool UseDefaultMargins() => false;

    public override void OnInspectorGUI()
    {
        if (targets.Length > 1)
        {
            EditorGUILayout.HelpBox("Multi-editing is not supported for this view.", MessageType.Warning);
            return;
        }
        
        if (IsBinaryFile(_filePath))
        {
            EditorGUILayout.HelpBox("This is a binary file and cannot be edited.", MessageType.Info);
            return;
        }
        
        HandleContextMenu();

        GUI.SetNextControlName("EditableTextArea");
        _currentContent = GUILayout.TextArea(_currentContent, EditorStyles.textArea, GUILayout.ExpandHeight(true));

        if (!_focused && Event.current.type == EventType.MouseDown)
        {
            GUI.FocusControl("EditableTextArea");
            _focused = true;
        }

        if (GUILayout.Button("Save")) SaveFile();
    }

    bool IsBinaryFile(string path) => Path.GetExtension(path) == ".bytes";

    void HandleContextMenu()
    {
        if (Event.current.type != EventType.ContextClick) return;
        var menu = new GenericMenu();
        menu.AddItem(new("Save"), false, SaveFile);  // Add Save option
        menu.AddItem(new("Reload"), false, ReloadFile);  // Add Reload option
        menu.ShowAsContext();
        Event.current.Use();
    }
    
    void ReloadFile()
    {
        _currentContent = ReadFileSafe(_filePath);
        Repaint();
    }
    
    void SaveFile()
    {
        var fileInfo = new FileInfo(_filePath);
        if (fileInfo.IsReadOnly)
        {
            EditorUtility.DisplayDialog("Save Failed", "The file is read-only.", "OK");
            return;
        }

        try
        {
            File.WriteAllText(_filePath, _currentContent);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Saved", "File successfully saved!", "OK");
        }
        catch (IOException e)
        {
            DLog.LogE($"Failed to save {_filePath}: {e.Message}");
        }
    }

    string ReadFileSafe(string path)
    {
        try { return File.ReadAllText(path); }
        catch (IOException e)
        {
            DLog.LogE($"Failed to read {path}: {e.Message}");
            return string.Empty;
        }
    }
}