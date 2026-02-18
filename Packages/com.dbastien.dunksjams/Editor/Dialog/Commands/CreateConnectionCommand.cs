using UnityEditor;

/// <summary>
/// Command to create a connection between two nodes.
/// </summary>
public class CreateConnectionCommand : DialogGraphCommand
{
    private readonly DialogConversation _conversation;
    private readonly DialogGraphView _graphView;
    private readonly DialogEntry _sourceEntry;
    private readonly DialogEntry _destEntry;
    private readonly int _linkIndex;

    public override string Name => "Create Connection";

    public CreateConnectionCommand
    (
        DialogConversation conversation, DialogGraphView graphView, DialogEntry sourceEntry, DialogEntry destEntry,
        int linkIndex
    )
    {
        _conversation = conversation;
        _graphView = graphView;
        _sourceEntry = sourceEntry;
        _destEntry = destEntry;
        _linkIndex = linkIndex;
    }

    public override void Execute()
    {
        if (_linkIndex >= 0 && _linkIndex < _sourceEntry.outgoingLinks.Count)
        {
            _sourceEntry.outgoingLinks[_linkIndex].destination = _destEntry;
            EditorUtility.SetDirty(_sourceEntry);
            EditorUtility.SetDirty(_conversation);
            _graphView.PopulateView(_conversation);
        }
    }

    public override void Undo()
    {
        if (_linkIndex >= 0 && _linkIndex < _sourceEntry.outgoingLinks.Count)
        {
            _sourceEntry.outgoingLinks[_linkIndex].destination = null;
            EditorUtility.SetDirty(_sourceEntry);
            EditorUtility.SetDirty(_conversation);
            _graphView.PopulateView(_conversation);
        }
    }
}