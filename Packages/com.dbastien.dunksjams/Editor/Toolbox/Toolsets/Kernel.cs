using UnityEditor;

public class Kernel : SingletonEditorBehaviour<Kernel>
{
    ToolsetLibrary _toolsetLibrary;

    public ToolsetLibrary ToolsetLibrary => _toolsetLibrary;

    protected override void InitInternal() => Startup();

    public void Startup()
    {
        if (_toolsetLibrary != null) return;
        _toolsetLibrary = new ToolsetLibrary();
        _toolsetLibrary.Setup();
    }

    public void Shutdown()
    {
        _toolsetLibrary?.Teardown();
        _toolsetLibrary = null;
        ToolbarStyles.Shutdown();
    }
}
