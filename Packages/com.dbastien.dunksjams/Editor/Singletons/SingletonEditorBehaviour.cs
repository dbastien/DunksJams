using UnityEditor;

/// <summary>
/// A singleton base class for Editor-only classes, similar to SingletonEagerBehaviour but for the Editor.
/// Inherits from ScriptableSingleton to leverage Unity's built-in editor singleton management.
/// </summary>
public abstract class SingletonEditorBehaviour<T> : ScriptableSingleton<T> where T : SingletonEditorBehaviour<T>
{
    bool _initialized;

    public static T Instance => instance;

    protected virtual void OnEnable()
    {
        EnsureInit();
    }

    void EnsureInit()
    {
        if (_initialized) return;
        _initialized = true;
        InitInternal();
    }

    protected abstract void InitInternal();
}