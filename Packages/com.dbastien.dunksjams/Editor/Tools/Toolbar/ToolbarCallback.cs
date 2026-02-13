using System;
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
        _guiViewType.GetProperty("visualTree", BindingFlags.Public | BindingFlags.Instance);

    static ScriptableObject _currentToolbar = null;
    static IMGUIContainer _imguiContainer = null;

    public static event Action ToolbarGUI;

    static ToolbarCallback()
    {
        EditorApplication.update -= OnUpdate;
        EditorApplication.update += OnUpdate;
    }

    static void OnUpdate()
    {
        if (_currentToolbar == null || _imguiContainer == null) FindRequiredObjects();
    }

    static void OnGUI() => ToolbarGUI?.Invoke();

    static void FindRequiredObjects()
    {
        if (_toolbarType == null || _guiViewType == null || _viewVisualTree == null) return;

        var toolbars = Resources.FindObjectsOfTypeAll(_toolbarType);
        if (toolbars.Length <= 0)
            return;

        _currentToolbar = toolbars[0] as ScriptableObject;
        if (_currentToolbar == null)
            return;

        var visualTree = _viewVisualTree.GetValue(_currentToolbar, null) as VisualElement;
        if (visualTree == null || visualTree.childCount == 0)
            return;

        _imguiContainer = visualTree[0] as IMGUIContainer;

        if (_imguiContainer == null)
            return;

        _imguiContainer.onGUIHandler -= OnGUI;
        _imguiContainer.onGUIHandler += OnGUI;
    }
}