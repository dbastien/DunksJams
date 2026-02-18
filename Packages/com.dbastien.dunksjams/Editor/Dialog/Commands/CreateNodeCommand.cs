using UnityEditor;
using UnityEngine;

/// <summary>
/// Command to create a new dialog node.
/// </summary>
public class CreateNodeCommand : DialogGraphCommand
{
    private readonly DialogConversation _conversation;
    private readonly DialogGraphView _graphView;
    private readonly DialogNodeType _nodeType;
    private readonly Rect _rect;
    private readonly string _guid;
    private DialogEntry _createdEntry;

    public override string Name => $"Create {_nodeType} Node";

    public CreateNodeCommand
    (
        DialogConversation conversation, DialogGraphView graphView, DialogNodeType nodeType, Rect rect,
        string guid = null
    )
    {
        _conversation = conversation;
        _graphView = graphView;
        _nodeType = nodeType;
        _rect = rect;
        _guid = guid ?? System.Guid.NewGuid().ToString();
    }

    public override void Execute()
    {
        // Create the entry
        _createdEntry = ScriptableObject.CreateInstance<DialogEntry>();
        _createdEntry.Initialize(_guid, _nodeType);
        _createdEntry.canvasRect = _rect;

        // Add to conversation
        AssetDatabase.AddObjectToAsset(_createdEntry, _conversation);
        _conversation.entries.Add(_createdEntry);

        EditorUtility.SetDirty(_createdEntry);
        EditorUtility.SetDirty(_conversation);
        AssetDatabase.SaveAssets();

        // Refresh graph view
        _graphView.PopulateView(_conversation);
    }

    public override void Undo()
    {
        if (_createdEntry == null) return;

        // Remove from conversation
        _conversation.entries.Remove(_createdEntry);
        AssetDatabase.RemoveObjectFromAsset(_createdEntry);

        EditorUtility.SetDirty(_conversation);
        AssetDatabase.SaveAssets();

        // Refresh graph view
        _graphView.PopulateView(_conversation);
    }
}