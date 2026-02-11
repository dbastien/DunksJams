using UnityEditor;

/// <summary>Toolbox kernel. Manages toolset library and lifecycle.</summary>
public class Kernel : SingletonEditorBehaviour<Kernel>
{
    ToolsetLibrary toolsetLibrary;

    public ToolsetLibrary ToolsetLibrary => toolsetLibrary;

    protected override void InitInternal()
    {
        Startup();
    }

    public void Startup()
    {
        if (toolsetLibrary != null) return;

        toolsetLibrary = new ToolsetLibrary();
        toolsetLibrary.Setup();
    }

    public void Shutdown()
    {
        toolsetLibrary?.Teardown();
        toolsetLibrary = null;
        ToolbarStyles.Shutdown();
    }
}
