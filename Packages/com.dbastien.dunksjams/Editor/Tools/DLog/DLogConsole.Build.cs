// Assets/Editor/DLog/DLogConsole.Build.cs

using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public sealed partial class DLogConsole
{
    internal static void ClearOnBuildIfNeeded()
    {
        if (DLogHub.ClearOnBuild)
            DLogHub.Clear();
    }
}

internal sealed class DLogBuildPreprocessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report) => DLogConsole.ClearOnBuildIfNeeded();
}