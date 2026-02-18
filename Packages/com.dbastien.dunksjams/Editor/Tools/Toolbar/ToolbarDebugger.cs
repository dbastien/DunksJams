using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public static class ToolbarDebugger
{
    [MenuItem("Window/Debug/Log Toolbar Structure")]
    public static void LogToolbarStructure()
    {
        Assembly editorAssembly = typeof(Editor).Assembly;

        // 1. Find the Toolbar type and dump its hierarchy
        Type toolbarType = editorAssembly.GetType("UnityEditor.Toolbar");
        if (toolbarType == null)
        {
            Debug.Log("[ToolbarDebugger] UnityEditor.Toolbar type NOT FOUND.");
            return;
        }

        Debug.Log($"[ToolbarDebugger] Toolbar type: {toolbarType.FullName}");
        Type t = toolbarType;
        while (t != null)
        {
            Debug.Log($"[ToolbarDebugger]   inherits: {t.FullName} (assembly: {t.Assembly.GetName().Name})");
            t = t.BaseType;
        }

        // 2. Check GUIView
        Type guiViewType = editorAssembly.GetType("UnityEditor.GUIView");
        Debug.Log($"[ToolbarDebugger] GUIView type: {(guiViewType != null ? guiViewType.FullName : "NOT FOUND")}");

        // 3. Search for visualTree / rootVisualElement on the entire type hierarchy
        BindingFlags flags = BindingFlags.Public |
                             BindingFlags.NonPublic |
                             BindingFlags.Instance |
                             BindingFlags.FlattenHierarchy;
        string[] propNamesToFind = { "visualTree", "rootVisualElement", "windowBackend", "baseRootVisualElement" };

        t = toolbarType;
        while (t != null && t != typeof(object))
        {
            foreach (string name in propNamesToFind)
            {
                PropertyInfo prop = t.GetProperty(name,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (prop != null)
                    Debug.Log(
                        $"[ToolbarDebugger]   FOUND property '{name}' on {t.FullName} -> returns {prop.PropertyType.Name}");
            }

            t = t.BaseType;
        }

        // Also check GUIView directly if it exists
        if (guiViewType != null)
            foreach (PropertyInfo prop in guiViewType.GetProperties(flags))
                if (prop.PropertyType == typeof(VisualElement) ||
                    prop.Name.Contains("visual", StringComparison.OrdinalIgnoreCase) ||
                    prop.Name.Contains("root", StringComparison.OrdinalIgnoreCase))
                    Debug.Log(
                        $"[ToolbarDebugger]   GUIView has property: '{prop.Name}' -> {prop.PropertyType.Name} (declared on {prop.DeclaringType?.Name})");

        // 4. Try to get an actual toolbar instance and its visual tree
        Object[] toolbars = Resources.FindObjectsOfTypeAll(toolbarType);
        Debug.Log($"[ToolbarDebugger] Toolbar instances found: {toolbars.Length}");
        if (toolbars.Length == 0) return;

        Object toolbar = toolbars[0];

        // Try rootVisualElement (EditorWindow path)
        TryGetVisualRoot(toolbar, toolbarType, "rootVisualElement");
        // Try visualTree (GUIView path)
        if (guiViewType != null)
            TryGetVisualRoot(toolbar, guiViewType, "visualTree");
        // Try on toolbar type directly
        TryGetVisualRoot(toolbar, toolbarType, "visualTree");
    }

    private static void TryGetVisualRoot(Object toolbar, Type lookupType, string propertyName)
    {
        PropertyInfo prop = lookupType.GetProperty(propertyName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

        if (prop == null)
        {
            Debug.Log($"[ToolbarDebugger]   {lookupType.Name}.{propertyName}: property not found");
            return;
        }

        try
        {
            object value = prop.GetValue(toolbar, null);
            if (value is VisualElement ve)
            {
                Debug.Log(
                    $"[ToolbarDebugger]   {lookupType.Name}.{propertyName}: SUCCESS (VisualElement, childCount={ve.childCount})");
                LogChildren(ve, 2);
            }
            else
            {
                Debug.Log(
                    $"[ToolbarDebugger]   {lookupType.Name}.{propertyName}: returned {value?.GetType().Name ?? "null"}");
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"[ToolbarDebugger]   {lookupType.Name}.{propertyName}: exception: {ex.Message}");
        }
    }

    private static void LogChildren(VisualElement element, int depth)
    {
        string indent = new(' ', depth * 2);
        Debug.Log($"[ToolbarDebugger] {indent}{element.GetType().Name} name:'{element.name}' layout:{element.layout}");

        foreach (VisualElement child in element.Children())
            LogChildren(child, depth + 1);
    }
}