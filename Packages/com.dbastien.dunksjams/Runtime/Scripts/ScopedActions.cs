using System;

public class ScopedActions : IDisposable
{
    private Action _onDispose;

    public ScopedActions(Action onCreate, Action onDispose)
    {
        _onDispose = onDispose;
        onCreate?.Invoke();
    }

    public void Dispose()
    {
        _onDispose?.Invoke();
        _onDispose = null;
    }
}