using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class HierarchyIconSettings
{
    private const string SettingsPath = "Preferences/Hierarchy Icons";
    private static readonly Dictionary<string, bool> ComponentVisibility = new();
    private static List<Type> ComponentTypes;

    private static bool AllEnabled
    {
        get => EditorPrefs.GetBool("AllEnabled", true);
        set => EditorPrefs.SetBool("AllEnabled", value);
    }

    static HierarchyIconSettings() => LoadComponentVisibilitySettings();

    private static void LoadComponentVisibilitySettings()
    {
        ComponentVisibility.Clear();

        ComponentTypes ??= ReflectionUtils.GetNonGenericDerivedTypes<Component>().ToList();

        foreach (Type type in ComponentTypes)
        {
            string key = type.FullName ?? string.Empty;
            ComponentVisibility[key] = EditorPrefs.GetBool(key, false);
        }
    }

    public static bool IsComponentVisible(Type type)
    {
        string key = type.FullName ?? string.Empty;
        return AllEnabled || (ComponentVisibility.TryGetValue(key, out bool visible) && visible);
    }

    public static void SetComponentVisibility(Type type, bool visible)
    {
        if (type == null) return;
        string key = type.FullName ?? string.Empty;
        ComponentVisibility[key] = visible;
        EditorPrefs.SetBool(key, visible);
    }

    [SettingsProvider]
    public static SettingsProvider CreateSettingsProvider() => new(SettingsPath, SettingsScope.User)
    {
        guiHandler = _ =>
        {
            AllEnabled = EditorGUILayout.Toggle("All", AllEnabled);
            if (AllEnabled) return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Components", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Group by namespace and sort
            IOrderedEnumerable<IGrouping<string, KeyValuePair<string, bool>>> groupedComponents =
                ComponentVisibility.
                    GroupBy(kvp => kvp.Key.Contains(".") ? kvp.Key[..kvp.Key.LastIndexOf('.')] : "Global").
                    OrderBy(group => group.Key);

            foreach (IGrouping<string, KeyValuePair<string, bool>> group in groupedComponents)
            {
                bool isExpanded = EditorPrefs.GetBool(group.Key, true);
                isExpanded = EditorGUILayout.Foldout(isExpanded, group.Key, true);
                EditorPrefs.SetBool(group.Key, isExpanded);

                if (!isExpanded) continue;
                ++EditorGUI.indentLevel;

                foreach (KeyValuePair<string, bool> kvp in group.OrderBy(kvp => kvp.Key))
                {
                    bool newVisible = EditorGUILayout.ToggleLeft(kvp.Key.Split('.').Last(), kvp.Value);
                    if (newVisible == kvp.Value) continue;
                    Type type = ComponentTypes.FirstOrDefault(t => t.FullName == kvp.Key);
                    SetComponentVisibility(type, newVisible);
                }

                --EditorGUI.indentLevel;
            }
        }
    };
}

[InitializeOnLoad]
public static class HierarchyAutoIcons
{
    static HierarchyAutoIcons() =>
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyItemGUI;

    private static void OnHierarchyItemGUI(int instanceID, Rect selectionRect)
    {
        var obj = EditorUtility.EntityIdToObject(instanceID) as GameObject;
        if (!obj) return;

        HashSet<Texture> displayedIcons = new();

        IEnumerable<Component> visibleComponents = obj.GetComponents<Component>().
            Where(c => c != null && HierarchyIconSettings.IsComponentVisible(c.GetType()));

        Rect iconRect = new(selectionRect.xMax, selectionRect.y, 16, 16);

        foreach (Component component in visibleComponents)
        {
            Texture icon = EditorGUIUtility.ObjectContent(null, component.GetType()).image;
            if (!icon || displayedIcons.Contains(icon)) continue;
            GUI.DrawTexture(iconRect, icon);
            displayedIcons.Add(icon);
            iconRect.x -= 18f;
        }
    }
}