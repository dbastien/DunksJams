using UnityEditor;

[InitializeOnLoad]
public static class Bootstrapper
{
    static Bootstrapper()
    {
        if (!Singleton<Kernel>.Instance.Initialized)
            Singleton<Kernel>.Instance.Startup();
    }
}
