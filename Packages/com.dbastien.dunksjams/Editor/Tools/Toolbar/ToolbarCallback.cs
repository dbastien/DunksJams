using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[InitializeOnLoad]
public static class ToolbarCallback
{
    static readonly Type _toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
    static readonly Type _guiViewType = typeof(Editor).Assembly.GetType("UnityEditor.GUIView");

    static readonly PropertyInfo _viewVisualTree =
        _guiViewType?.GetProperty("visualTree",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

    const string LeftContainerName = "DunksJams-ToolbarLeft";
    const string RightContainerName = "DunksJams-ToolbarRight";

    static ScriptableObject _currentToolbar;
    static bool _hooked;

    public static event Action OnLeftToolbarGUI;
    public static event Action OnRightToolbarGUI;

    static ToolbarCallback()
    {
        _currentToolbar = null;
        _hooked = false;
        EditorApplication.update -= OnUpdate;
        EditorApplication.update += OnUpdate;
    }

    static void OnUpdate()
    {
        if (_hooked && _currentToolbar != null) return;
        _hooked = false;
        TryHook();
    }

    static void TryHook()
    {
        if (_toolbarType == null || _guiViewType == null || _viewVisualTree == null)
            return;

        var toolbars = Resources.FindObjectsOfTypeAll(_toolbarType);
        if (toolbars.Length == 0) return;

        _currentToolbar = toolbars[0] as ScriptableObject;
        if (_currentToolbar == null) return;

        var root = _viewVisualTree.GetValue(_currentToolbar, null) as VisualElement;
        if (root == null) return;

        // Clean up previously injected containers (from domain reload)
        root.Q(LeftContainerName)?.RemoveFromHierarchy();
        root.Q(RightContainerName)?.RemoveFromHierarchy();

        // Unity 6000+: overlay toolbar with DockArea elements
        var overlayToolbar = root.Q("overlay-toolbar__top");
        if (overlayToolbar != null)
        {
            var dockAreas = overlayToolbar.Query<VisualElement>(name: "DockArea").ToList();
            if (dockAreas.Count >= 2)
            {
                AddIMGUIContainer(dockAreas[0], LeftContainerName, () => OnLeftToolbarGUI?.Invoke());
                AddIMGUIContainer(dockAreas[1], RightContainerName, () => OnRightToolbarGUI?.Invoke());
                _hooked = true;
                return;
            }
        }

        // Fallback: older Unity ToolbarZone approach
        var leftZone = root.Q("ToolbarZoneLeftAlign");
        var rightZone = root.Q("ToolbarZoneRightAlign");

        if (leftZone != null)
            AddIMGUIContainer(leftZone, LeftContainerName, () => OnLeftToolbarGUI?.Invoke());
        if (rightZone != null)
            AddIMGUIContainer(rightZone, RightContainerName, () => OnRightToolbarGUI?.Invoke());

        _hooked = leftZone != null || rightZone != null;
    }

    static void AddIMGUIContainer(VisualElement parent, string containerName, Action onGUI)
    {
        var container = new IMGUIContainer(onGUI) { name = containerName };
        container.style.flexGrow = 1;
        container.style.flexShrink = 0;
        parent.Add(container);
    }
}
