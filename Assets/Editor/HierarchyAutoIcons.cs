using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class HierarchyIconSettings
{
    const string SettingsPath = "Preferences/Hierarchy Icons";
    static readonly Dictionary<string, bool> ComponentVisibility = new();
    private static List<Type> ComponentTypes;

    static bool AllEnabled
    {
        get => EditorPrefs.GetBool("AllEnabled", false);
        set => EditorPrefs.SetBool("AllEnabled", value);
    }

    static HierarchyIconSettings() => LoadComponentVisibilitySettings();

    static void LoadComponentVisibilitySettings()
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

            var sortedComponents = ComponentVisibility.OrderBy(kvp => kvp.Key);
            foreach (var kvp in sortedComponents)
            {
                bool newVisible = EditorGUILayout.ToggleLeft(kvp.Key.Split('.').Last(), kvp.Value);
                if (newVisible == kvp.Value) continue;
                Type type = ComponentTypes.FirstOrDefault(t => t.FullName == kvp.Key);
                SetComponentVisibility(type, newVisible);
            }
        }
    };
}

[InitializeOnLoad]
public static class HierarchyAutoIcons
{
    static HierarchyAutoIcons() =>
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyItemGUI;

    static void OnHierarchyItemGUI(int instanceID, Rect selectionRect)
    {
        GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (!obj) return;

        var visibleComponents = obj.GetComponents<Component>()
            .Where(c => c != null && HierarchyIconSettings.IsComponentVisible(c.GetType()))
            .ToArray();

        Rect iconRect = new(selectionRect.xMax, selectionRect.y, 16, 16);

        foreach (var component in visibleComponents)
        {
            Texture icon = EditorGUIUtility.ObjectContent(null, component.GetType()).image;
            if (!icon) continue;
            GUI.DrawTexture(iconRect, icon);
            iconRect.x -= 18f;
        }
    }
}