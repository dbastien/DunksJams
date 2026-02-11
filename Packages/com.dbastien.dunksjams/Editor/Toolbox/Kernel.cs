using UnityEditor;

/// <summary>Toolbox kernel. Manages toolset library and lifecycle.</summary>
public class Kernel
{
    ToolsetLibrary toolsetLibrary;
    bool initialized;

    public ToolsetLibrary ToolsetLibrary => toolsetLibrary;
    public bool Initialized => initialized;

    public void Startup()
    {
        if (initialized) return;

        toolsetLibrary = new ToolsetLibrary();
        toolsetLibrary.Setup();

        initialized = true;
    }

    public void Shutdown()
    {
        initialized = false;
        toolsetLibrary?.Teardown();
        toolsetLibrary = null;
        ToolbarStyles.Shutdown();
    }
}
