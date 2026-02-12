using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class ToolbarNewToolsetWindow : EditorWindow
{
    static readonly GUIContent OkayContent = new("Okay");
    static readonly GUIContent CancelContent = new("Cancel");

    ToolbarCustomizeDrawer _toolbarCustomize;
    List<ToolsetLibrary.ToolsetInfo> _availableToolsetInfos;
    TreeViewState _availableToolsetTreeViewState;
    ToolsetTreeView _availableToolsetTreeView;

    public static void ShowWindow(ToolbarCustomizeDrawer drawer)
    {
        var w = GetWindowWithRect<ToolbarNewToolsetWindow>(new Rect(0, 0, 320, 320), false, "Add Toolset");
        w.Populate(drawer);
    }

    public static void CloseWindow()
    {
        if (HasOpenInstances<ToolbarNewToolsetWindow>())
            GetWindow<ToolbarNewToolsetWindow>().Close();
    }

    void Setup()
    {
        _availableToolsetInfos = new List<ToolsetLibrary.ToolsetInfo>();
        _availableToolsetTreeViewState ??= new TreeViewState();
        _availableToolsetTreeView = new ToolsetTreeView(_availableToolsetTreeViewState);
        _availableToolsetTreeView.Infos = _availableToolsetInfos;
    }

    void Teardown()
    {
        _availableToolsetInfos?.Clear();
        _availableToolsetInfos = null;
        _toolbarCustomize = null;
    }

    void OnEnable() => Setup();
    void OnDisable() => Teardown();

    void OnGUI()
    {
        var rect = GUILayoutUtility.GetRect(1000, 1000, 0, 100000);
        _availableToolsetTreeView.OnGUI(rect);
        GUI.enabled = false;
        EditorGUILayout.TextArea(GetSelectedDescription(), ToolbarStyles.ToolbarTextAreaStyle, GUILayout.Height(100));
        GUI.enabled = true;

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(OkayContent)) OnOkay();
        if (GUILayout.Button(CancelContent)) OnCancel();
        EditorGUILayout.EndHorizontal();

        if (Event.current is { isKey: true })
        {
            if (Event.current.keyCode == KeyCode.Return) OnOkay();
            else if (Event.current.keyCode == KeyCode.Escape) OnCancel();
        }
    }

    void Populate(ToolbarCustomizeDrawer drawer)
    {
        _toolbarCustomize = drawer;
        _availableToolsetInfos.Clear();
        _availableToolsetInfos.AddRange(
            Kernel.Instance.ToolsetLibrary.ToolsetInfos.Except(_toolbarCustomize.ActiveToolsetInfos));
        _availableToolsetTreeView.Infos = _availableToolsetInfos;
        _availableToolsetTreeView.Reload();
    }

    string GetSelectedDescription()
    {
        if (_availableToolsetTreeView.Selections.Count == 0) return "";
        var idx = _availableToolsetTreeView.Selections[0];
        return idx >= 0 && idx < _availableToolsetInfos.Count ? _availableToolsetInfos[idx].description : "";
    }

    void OnOkay()
    {
        if (_availableToolsetTreeView.Selections.Count == 0) return;
        var selections = _availableToolsetTreeView.Selections.Select(i => _availableToolsetInfos[i]).ToList();
        _toolbarCustomize.AddToolsets(selections);
        Close();
    }

    void OnCancel() => Close();
}
