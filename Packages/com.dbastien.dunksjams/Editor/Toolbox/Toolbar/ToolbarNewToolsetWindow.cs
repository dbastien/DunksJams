using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class ToolbarNewToolsetWindow : EditorWindow
{
    static readonly GUIContent OkayContent = new("Okay");
    static readonly GUIContent CancelContent = new("Cancel");

    ToolbarCustomizeDrawer toolbarCustomize;
    List<ToolsetLibrary.ToolsetInfo> availableToolsetInfos;
    TreeViewState availableToolsetTreeViewState;
    ToolsetTreeView availableToolsetTreeView;

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
        availableToolsetInfos = new List<ToolsetLibrary.ToolsetInfo>();
        availableToolsetTreeViewState ??= new TreeViewState();
        availableToolsetTreeView = new ToolsetTreeView(availableToolsetTreeViewState);
        availableToolsetTreeView.Infos = availableToolsetInfos;
    }

    void Teardown()
    {
        availableToolsetInfos?.Clear();
        availableToolsetInfos = null;
        toolbarCustomize = null;
    }

    void OnEnable() => Setup();
    void OnDisable() => Teardown();

    void OnGUI()
    {
        var rect = GUILayoutUtility.GetRect(1000, 1000, 0, 100000);
        availableToolsetTreeView.OnGUI(rect);

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
        toolbarCustomize = drawer;
        availableToolsetInfos.Clear();
        availableToolsetInfos.AddRange(
            Kernel.Instance.ToolsetLibrary.ToolsetInfos.Except(toolbarCustomize.ActiveToolsetInfos));
        availableToolsetTreeView.Infos = availableToolsetInfos;
        availableToolsetTreeView.Reload();
    }

    string GetSelectedDescription()
    {
        if (availableToolsetTreeView.Selections.Count == 0) return "";
        var idx = availableToolsetTreeView.Selections[0];
        return idx >= 0 && idx < availableToolsetInfos.Count ? availableToolsetInfos[idx].description : "";
    }

    void OnOkay()
    {
        if (availableToolsetTreeView.Selections.Count == 0) return;
        var selections = availableToolsetTreeView.Selections.Select(i => availableToolsetInfos[i]).ToList();
        toolbarCustomize.AddToolsets(selections);
        Close();
    }

    void OnCancel() => Close();
}
