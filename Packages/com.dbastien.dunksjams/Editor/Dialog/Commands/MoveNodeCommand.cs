using UnityEditor;
using UnityEngine;

/// <summary>
/// Command to move a dialog node.
/// Supports merging multiple move operations for smooth dragging.
/// </summary>
public class MoveNodeCommand : DialogGraphCommand
{
    private readonly DialogConversation _conversation;
    private readonly DialogEntry _entry;
    private readonly DialogGraphView _graphView;
    private Rect _startRect;
    private Rect _endRect;

    public override string Name => "Move Node";

    public MoveNodeCommand(DialogConversation conversation, DialogGraphView graphView, DialogEntry entry, Rect startRect, Rect endRect)
    {
        _conversation = conversation;
        _entry = entry;
        _graphView = graphView;
        _startRect = startRect;
        _endRect = endRect;
    }

    public override void Execute()
    {
        _entry.canvasRect = _endRect;
        EditorUtility.SetDirty(_entry);
        EditorUtility.SetDirty(_conversation);
    }

    public override void Undo()
    {
        _entry.canvasRect = _startRect;
        EditorUtility.SetDirty(_entry);
        EditorUtility.SetDirty(_conversation);
        _graphView.PopulateView(_conversation);
    }

    public override bool TryMerge(DialogGraphCommand other)
    {
        if (other is MoveNodeCommand moveCommand && moveCommand._entry == _entry)
        {
            // Merge the move: update end position
            _endRect = moveCommand._endRect;
            return true;
        }
        return false;
    }
}
