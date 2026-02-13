using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

[InitializeOnLoad]
public static class ToolbarGatherer
{
    private const string AUTO_REFRESH_ENABLED = "ToolbarGatherer_Tool_AutoRefresh";
    private const string AUTO_REFRESH_SECONDS = "ToolbarGatherer_Tool_AutoRefreshSeconds";

    public static readonly IReadOnlyDictionary<ToolbarItemPosition, IReadOnlyDictionary<ToolbarItemAnchor, IReadOnlyList<IToolbarItem>>> ToolbarsByPosition;
    public static readonly IReadOnlyCollection<IToolbarItem> AllToolbars;

    public static bool AutoUpdateToolbar { get; private set; } = true;
    public static float AutoUpdateSeconds { get; private set; } = 5f;

    static ToolbarGatherer()
    {
        Type interfaceType = typeof(IToolbarItem);
        AllToolbars = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => interfaceType.IsAssignableFrom(x) &&
                        !x.IsAbstract &&
                        !x.IsInterface &&
                        x.IsClass)
            .Select(x => x.GetConstructor(Type.EmptyTypes))
            .Where(x => x != null)
            .Select(x => x.Invoke(null) as IToolbarItem)
            .Where(x => x != null)
            .OrderBy(x => x.Position)
            .ThenBy(x => x.Anchor)
            .ThenBy(x => x.Priority)
            .ToArray();

        var toolbarDictionary = new Dictionary<ToolbarItemPosition, Dictionary<ToolbarItemAnchor, List<IToolbarItem>>>();

        foreach (IToolbarItem toolbarItem in AllToolbars)
        {
            toolbarItem.Init();
            if (!toolbarDictionary.TryGetValue(toolbarItem.Position, out var anchorDict))
                toolbarDictionary[toolbarItem.Position] = anchorDict = new Dictionary<ToolbarItemAnchor, List<IToolbarItem>>();
            
            if (!anchorDict.TryGetValue(toolbarItem.Anchor, out var toolbarList))
                anchorDict[toolbarItem.Anchor] = toolbarList = new List<IToolbarItem>();
            
            toolbarList.Add(toolbarItem);
        }

        ToolbarsByPosition = toolbarDictionary
            .ToDictionary(x => x.Key,
                x => (IReadOnlyDictionary<ToolbarItemAnchor, IReadOnlyList<IToolbarItem>>)x.Value
                    .ToDictionary(y => y.Key,
                        y => (IReadOnlyList<IToolbarItem>)y.Value));

        AutoUpdateToolbar = EditorPrefs.GetBool(AUTO_REFRESH_ENABLED, true);
        AutoUpdateSeconds = EditorPrefs.GetFloat(AUTO_REFRESH_SECONDS, 5);
    }

    [SettingsProviderGroup]
    private static SettingsProvider[] GetSettingsProviders()
    {
        return AllToolbars
            .Select(x => x.GetSettingsProvider())
            .Where(x => x != null)
            .Prepend(
                new SettingsProvider("Preferences/Custom Toolbar", SettingsScope.User)
                {
                    guiHandler = value =>
                    {
                        AutoUpdateToolbar = EditorGUILayout.Toggle("Auto Refresh: ", AutoUpdateToolbar);
                        if (AutoUpdateToolbar)
                        {
                            AutoUpdateSeconds = EditorGUILayout.FloatField("Update Seconds:", AutoUpdateSeconds);
                        }
                        EditorPrefs.SetBool(AUTO_REFRESH_ENABLED, AutoUpdateToolbar);
                        EditorPrefs.SetFloat(AUTO_REFRESH_SECONDS, AutoUpdateSeconds);
                    }
                })
            .ToArray();
    }
}
