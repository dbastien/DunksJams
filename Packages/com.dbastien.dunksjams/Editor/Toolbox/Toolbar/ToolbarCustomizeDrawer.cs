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

    ToolboxUserSettings userSettings;
    List<ToolsetLibrary.ToolsetInfo> activeToolsetInfos;
    TreeViewState activeToolsetTreeViewState;
    ToolsetTreeView activeToolsetTreeView;

    public List<ToolsetLibrary.ToolsetInfo> ActiveToolsetInfos => activeToolsetInfos;

    public void Setup(ToolboxUserSettings settings)
    {
        newButtonContent = EditorGUIUtility.TrIconContent("d_Toolbar Plus", "Add toolset");
        deleteButtonContent = EditorGUIUtility.TrIconContent("d_Toolbar Minus", "Remove toolset");
        moveUpButtonContent = EditorGUIUtility.TrIconContent("CollabPush", "Move up");
        moveDownButtonContent = EditorGUIUtility.TrIconContent("CollabPull", "Move down");

        userSettings = settings;
        activeToolsetInfos = new List<ToolsetLibrary.ToolsetInfo>();
        PopulateWithUserSettings();

        activeToolsetTreeViewState = new TreeViewState();
        activeToolsetTreeView = new ToolsetTreeView(activeToolsetTreeViewState);
        activeToolsetTreeView.Infos = activeToolsetInfos;
        activeToolsetTreeView.Reload();
    }

    public void Teardown()
    {
        activeToolsetTreeViewState = null;
        activeToolsetTreeView = null;
        activeToolsetInfos?.Clear();
        activeToolsetInfos = null;
        userSettings = null;
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
        activeToolsetTreeView.OnGUI(rect);

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
        var index = activeToolsetTreeView.Selections.Count > 0 ? activeToolsetTreeView.Selections[0] : activeToolsetInfos.Count;
        foreach (var info in infos)
            activeToolsetInfos.Insert(index++, info);
        activeToolsetTreeView.Infos = activeToolsetInfos;
        activeToolsetTreeView.Reload();
    }

    string GetSelectedDescription()
    {
        if (activeToolsetTreeView.Selections.Count == 0) return "";
        var idx = activeToolsetTreeView.Selections[0];
        return idx >= 0 && idx < activeToolsetInfos.Count ? activeToolsetInfos[idx].description : "";
    }

    void PopulateWithUserSettings()
    {
        foreach (var name in userSettings.Toolsets)
        {
            var info = Kernel.Instance.ToolsetLibrary.GetToolsetInfo(name);
            if (info != null) activeToolsetInfos.Add(info);
            else DLog.LogE($"Invalid toolset in settings: {name}");
        }
    }

    void OnNewToolset() => ToolbarNewToolsetWindow.ShowWindow(this);
    void OnDeleteToolset()
    {
        if (activeToolsetTreeView.Selections.Count == 0) return;
        var idx = activeToolsetTreeView.Selections[0];
        if (idx < 0 || idx >= activeToolsetInfos.Count) return;
        ToolbarNewToolsetWindow.CloseWindow();
        activeToolsetInfos.RemoveAt(idx);
        activeToolsetTreeView.Reload();
        activeToolsetTreeView.SetSelection(idx < activeToolsetInfos.Count ? new[] { idx } : System.Array.Empty<int>(),
            TreeViewSelectionOptions.FireSelectionChanged);
    }

    void OnMoveUpToolset()
    {
        if (activeToolsetTreeView.Selections.Count == 0 || activeToolsetTreeView.Selections[0] == 0) return;
        var idx = activeToolsetTreeView.Selections[0];
        var info = activeToolsetInfos[idx];
        activeToolsetInfos.RemoveAt(idx);
        activeToolsetInfos.Insert(idx - 1, info);
        activeToolsetTreeView.Reload();
        activeToolsetTreeView.SetSelection(new[] { idx - 1 }, TreeViewSelectionOptions.FireSelectionChanged);
    }

    void OnMoveDownToolset()
    {
        if (activeToolsetTreeView.Selections.Count == 0) return;
        var idx = activeToolsetTreeView.Selections[0];
        if (idx >= activeToolsetInfos.Count - 1) return;
        var info = activeToolsetInfos[idx];
        activeToolsetInfos.RemoveAt(idx);
        activeToolsetInfos.Insert(idx + 1, info);
        activeToolsetTreeView.Reload();
        activeToolsetTreeView.SetSelection(new[] { idx + 1 }, TreeViewSelectionOptions.FireSelectionChanged);
    }

    void OnApply()
    {
        var changed = userSettings.Toolsets.Count != activeToolsetInfos.Count;
        if (!changed)
        {
            for (var i = 0; i < userSettings.Toolsets.Count; i++)
            {
                if (userSettings.Toolsets[i] != activeToolsetInfos[i].type.FullName) { changed = true; break; }
            }
        }
        if (changed)
        {
            userSettings.Toolsets.Clear();
            foreach (var info in activeToolsetInfos)
                userSettings.Toolsets.Add(info.type.FullName ?? info.type.Name);
            userSettings.Save();
            ToolbarWindow.GetWindowIfOpened()?.Reload();
        }
    }

    void OnReset()
    {
        activeToolsetInfos.Clear();
        PopulateWithUserSettings();
        activeToolsetTreeView.Reload();
    }
}
