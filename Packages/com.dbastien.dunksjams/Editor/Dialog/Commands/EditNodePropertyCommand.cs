using System;
using UnityEditor;

/// <summary>
/// Generic command to edit a node property.
/// Uses delegates to get/set property values.
/// </summary>
public class EditNodePropertyCommand : DialogGraphCommand
{
    private readonly DialogEntry _entry;
    private readonly string _propertyName;
    private readonly string _oldValue;
    private readonly string _newValue;
    private readonly Action<string> _setter;
    private readonly Func<string> _getter;

    public override string Name => $"Edit {_propertyName}";

    public EditNodePropertyCommand(DialogEntry entry, string propertyName, string oldValue, string newValue, Action<string> setter, Func<string> getter)
    {
        _entry = entry;
        _propertyName = propertyName;
        _oldValue = oldValue;
        _newValue = newValue;
        _setter = setter;
        _getter = getter;
    }

    public override void Execute()
    {
        _setter?.Invoke(_newValue);
        EditorUtility.SetDirty(_entry);
    }

    public override void Undo()
    {
        _setter?.Invoke(_oldValue);
        EditorUtility.SetDirty(_entry);
    }

    public override bool TryMerge(DialogGraphCommand other)
    {
        // Can merge if editing the same property on the same entry
        // This prevents spam when typing in text fields
        if (other is EditNodePropertyCommand editCommand &&
            editCommand._entry == _entry &&
            editCommand._propertyName == _propertyName)
        {
            // Merge by updating new value
            // Keep the original old value
            return false; // Actually don't merge for now - it causes issues with text fields
        }
        return false;
    }
}
