using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the undo/redo command history for the dialog graph editor.
/// </summary>
public class DialogCommandHistory
{
    private readonly Stack<DialogGraphCommand> _undoStack = new();
    private readonly Stack<DialogGraphCommand> _redoStack = new();
    private int _maxStackSize = 50;
    private bool _isExecuting;

    public int UndoCount => _undoStack.Count;
    public int RedoCount => _redoStack.Count;
    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    public event Action OnHistoryChanged;

    public int MaxStackSize
    {
        get => _maxStackSize;
        set => _maxStackSize = Mathf.Max(1, value);
    }

    /// <summary>
    /// Execute a command and add it to the undo stack.
    /// </summary>
    public void ExecuteCommand(DialogGraphCommand command)
    {
        if (command == null) return;
        if (_isExecuting) return; // Prevent recursive execution

        _isExecuting = true;
        try
        {
            command.Execute();

            // Try to merge with the last command
            if (_undoStack.Count > 0 && _undoStack.Peek().TryMerge(command))
            {
                // Command was merged, don't add to stack
                return;
            }

            _undoStack.Push(command);
            _redoStack.Clear(); // Clear redo stack when new command is executed

            // Enforce max stack size
            while (_undoStack.Count > _maxStackSize)
            {
                // Remove oldest command
                var stack = new Stack<DialogGraphCommand>(_undoStack.Count - 1);
                var count = 0;
                foreach (var cmd in _undoStack)
                {
                    if (count++ < _undoStack.Count - 1)
                        stack.Push(cmd);
                }
                _undoStack.Clear();
                while (stack.Count > 0)
                    _undoStack.Push(stack.Pop());
            }

            OnHistoryChanged?.Invoke();
        }
        finally
        {
            _isExecuting = false;
        }
    }

    /// <summary>
    /// Undo the last command.
    /// </summary>
    public void Undo()
    {
        if (!CanUndo || _isExecuting) return;

        _isExecuting = true;
        try
        {
            var command = _undoStack.Pop();
            command.Undo();
            _redoStack.Push(command);
            OnHistoryChanged?.Invoke();
            Debug.Log($"[Undo] {command.Name}");
        }
        finally
        {
            _isExecuting = false;
        }
    }

    /// <summary>
    /// Redo the last undone command.
    /// </summary>
    public void Redo()
    {
        if (!CanRedo || _isExecuting) return;

        _isExecuting = true;
        try
        {
            var command = _redoStack.Pop();
            command.Execute();
            _undoStack.Push(command);
            OnHistoryChanged?.Invoke();
            Debug.Log($"[Redo] {command.Name}");
        }
        finally
        {
            _isExecuting = false;
        }
    }

    /// <summary>
    /// Clear all undo/redo history.
    /// </summary>
    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
        OnHistoryChanged?.Invoke();
    }

    /// <summary>
    /// Get the name of the next command that would be undone.
    /// </summary>
    public string GetUndoCommandName()
    {
        return CanUndo ? _undoStack.Peek().Name : null;
    }

    /// <summary>
    /// Get the name of the next command that would be redone.
    /// </summary>
    public string GetRedoCommandName()
    {
        return CanRedo ? _redoStack.Peek().Name : null;
    }
}
