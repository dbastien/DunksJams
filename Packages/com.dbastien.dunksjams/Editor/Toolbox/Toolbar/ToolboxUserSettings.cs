using System.Collections.Generic;
using UnityEngine;

/// <summary>User preferences for the toolbox toolbar (stored in UserSettings).</summary>
public class ToolboxUserSettings : ScriptableObject
{
    const string SettingsPath = "./UserSettings/ToolboxUserSettings.asset";

    [SerializeField] List<string> toolsets = new();

    public List<string> Toolsets => toolsets;

    internal static ToolboxUserSettings GetOrCreateSettings()
    {
        var tmp = UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget(SettingsPath);
        if (tmp.Length > 0 && tmp[0] is ToolboxUserSettings s) return s;
        return CreateInstance<ToolboxUserSettings>();
    }

    internal void Save()
    {
        UnityEditorInternal.InternalEditorUtility.SaveToSerializedFileAndForget(
            new Object[] { this }, SettingsPath, true);
    }
}
