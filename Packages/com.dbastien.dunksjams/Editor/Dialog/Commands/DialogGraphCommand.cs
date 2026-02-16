using UnityEngine;

/// <summary>
/// Base class for all dialog graph commands.
/// Implements the Command Pattern for undo/redo functionality.
/// </summary>
public abstract class DialogGraphCommand
{
    public abstract string Name { get; }

    /// <summary>
    /// Execute the command (perform the operation).
    /// </summary>
    public abstract void Execute();

    /// <summary>
    /// Undo the command (revert the operation).
    /// </summary>
    public abstract void Undo();

    /// <summary>
    /// Optional: Merge this command with another command if they're similar.
    /// Useful for combining multiple similar operations (e.g., dragging a node).
    /// </summary>
    /// <returns>True if merged successfully</returns>
    public virtual bool TryMerge(DialogGraphCommand other) => false;
}
