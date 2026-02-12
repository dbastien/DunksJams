using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

/// <summary>Draws the Toolbox customization UI in Preferences.</summary>
public class ToolbarCustomizeDrawer
{
    static GUIContent newButtonContent;
    static GUIContent deleteButtonContent;
    static GUIContent moveUpButtonContent;
    static GUIContent moveDownButtonContent;

    ToolboxUserSettings _userSettings;
    List<ToolsetLibrary.ToolsetInfo> _activeToolsetInfos;
    TreeViewState _activeToolsetTreeViewState;
    ToolsetTreeView _activeToolsetTreeView;

    public List<ToolsetLibrary.ToolsetInfo> ActiveToolsetInfos => _activeToolsetInfos;

    public void Setup(ToolboxUserSettings settings)
    {
        newButtonContent = EditorGUIUtils.IconContentSafe("d_Toolbar Plus", "Toolbar Plus", "Add toolset");
        deleteButtonContent = EditorGUIUtils.IconContentSafe("d_Toolbar Minus", "Toolbar Minus", "Remove toolset");
        moveUpButtonContent = EditorGUIUtils.IconContentSafe("CollabPush", "Animation.PrevKey", "Move up");
        moveDownButtonContent = EditorGUIUtils.IconContentSafe("CollabPull", "Animation.NextKey", "Move down");

        _userSettings = settings;
        _activeToolsetInfos = new List<ToolsetLibrary.ToolsetInfo>();
        PopulateWithUserSettings();
        _activeToolsetTreeViewState = new TreeViewState();
        _activeToolsetTreeView = new ToolsetTreeView(_activeToolsetTreeViewState);
        _activeToolsetTreeView.Infos = _activeToolsetInfos;
        _activeToolsetTreeView.Reload();
    }

    public void Teardown()
    {
        _activeToolsetTreeViewState = null;
        _activeToolsetTreeView = null;
        _activeToolsetInfos?.Clear();
        _activeToolsetInfos = null;
        _userSettings = null;
        ToolbarNewToolsetWindow.CloseWindow();
    }

    public void Draw()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(newButtonContent, ToolbarStyles.ToolbarButtonRightStyle)) OnNewToolset();
        if (GUILayout.Button(deleteButtonContent, ToolbarStyles.ToolbarButtonRightStyle)) OnDeleteToolset();
        if (GUILayout.Button(moveUpButtonContent, ToolbarStyles.ToolbarButtonRightStyle)) OnMoveUpToolset();
        if (GUILayout.Button(moveDownButtonContent, ToolbarStyles.ToolbarButtonRightStyle)) OnMoveDownToolset();
        EditorGUILayout.EndHorizontal();

        var rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
        _activeToolsetTreeView.OnGUI(rect);
        GUI.enabled = false;
        EditorGUILayout.TextArea(GetSelectedDescription(), ToolbarStyles.ToolbarTextAreaStyle, GUILayout.Height(100));
        GUI.enabled = true;

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Apply")) OnApply();
        if (GUILayout.Button("Reset")) OnReset();
        EditorGUILayout.EndHorizontal();

        if (Event.current is { isKey: true })
        {
            if (Event.current.keyCode == KeyCode.Return) OnApply();
            else if (Event.current.keyCode == KeyCode.Escape) OnReset();
        }
    }

    public void AddToolsets(List<ToolsetLibrary.ToolsetInfo> infos)
    {
        var index = _activeToolsetTreeView.Selections.Count > 0 ? _activeToolsetTreeView.Selections[0] : _activeToolsetInfos.Count;
        foreach (var info in infos)
        {
            _activeToolsetInfos.Insert(index, info);
            ++index;
        }
        _activeToolsetTreeView.Infos = _activeToolsetInfos;
        _activeToolsetTreeView.Reload();
    }

    string GetSelectedDescription()
    {
        if (_activeToolsetTreeView.Selections.Count == 0) return "";
        var idx = _activeToolsetTreeView.Selections[0];
        return idx >= 0 && idx < _activeToolsetInfos.Count ? _activeToolsetInfos[idx].description : "";
    }

    void PopulateWithUserSettings()
    {
        foreach (var name in _userSettings.Toolsets)
        {
            var info = Kernel.Instance.ToolsetLibrary.GetToolsetInfo(name);
            if (info != null) _activeToolsetInfos.Add(info);
            else DLog.LogE($"Invalid toolset in settings: {name}");
        }
    }

    void OnNewToolset() => ToolbarNewToolsetWindow.ShowWindow(this);
    void OnDeleteToolset()
    {
        if (_activeToolsetTreeView.Selections.Count == 0) return;
        var idx = _activeToolsetTreeView.Selections[0];
        if (idx < 0 || idx >= _activeToolsetInfos.Count) return;
        ToolbarNewToolsetWindow.CloseWindow();
        _activeToolsetInfos.RemoveAt(idx);
        _activeToolsetTreeView.Reload();
        _activeToolsetTreeView.SetSelection(idx < _activeToolsetInfos.Count ? new[] { idx } : System.Array.Empty<int>(),
            TreeViewSelectionOptions.FireSelectionChanged);
    }

    void OnMoveUpToolset()
    {
        if (_activeToolsetTreeView.Selections.Count == 0 || _activeToolsetTreeView.Selections[0] == 0) return;
        var idx = _activeToolsetTreeView.Selections[0];
        var info = _activeToolsetInfos[idx];
        _activeToolsetInfos.RemoveAt(idx);
        _activeToolsetInfos.Insert(idx - 1, info);
        _activeToolsetTreeView.Reload();
        _activeToolsetTreeView.SetSelection(new[] { idx - 1 }, TreeViewSelectionOptions.FireSelectionChanged);
    }

    void OnMoveDownToolset()
    {
        if (_activeToolsetTreeView.Selections.Count == 0) return;
        var idx = _activeToolsetTreeView.Selections[0];
        if (idx >= _activeToolsetInfos.Count - 1) return;
        var info = _activeToolsetInfos[idx];
        _activeToolsetInfos.RemoveAt(idx);
        _activeToolsetInfos.Insert(idx + 1, info);
        _activeToolsetTreeView.Reload();
        _activeToolsetTreeView.SetSelection(new[] { idx + 1 }, TreeViewSelectionOptions.FireSelectionChanged);
    }

    void OnApply()
    {
        var changed = _userSettings.Toolsets.Count != _activeToolsetInfos.Count;
        if (!changed)
        {
            for (var i = 0; i < _userSettings.Toolsets.Count; ++i)
            {
                if (_userSettings.Toolsets[i] != _activeToolsetInfos[i].type.FullName) { changed = true; break; }
            }
        }
        if (changed)
        {
            _userSettings.Toolsets.Clear();
            foreach (var info in _activeToolsetInfos)
                _userSettings.Toolsets.Add(info.type.FullName ?? info.type.Name);
            _userSettings.Save();
            ToolbarWindow.GetWindowIfOpened()?.Reload();
        }
    }

    void OnReset()
    {
        _activeToolsetInfos.Clear();
        PopulateWithUserSettings();
        _activeToolsetTreeView.Reload();
    }
}
