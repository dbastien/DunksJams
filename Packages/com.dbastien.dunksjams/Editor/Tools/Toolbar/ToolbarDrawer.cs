using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class ToolbarDrawer
{
    static ToolbarDrawer()
    {
        ToolbarExtension.OnUpdate -= OnUpdate;
        ToolbarExtension.OnLeftToolbarGUI -= OnLeftToolbarGUI;
        ToolbarExtension.OnRightToolbarGUI -= OnRightToolbarGUI;

        ToolbarExtension.OnUpdate += OnUpdate;
        ToolbarExtension.OnLeftToolbarGUI += OnLeftToolbarGUI;
        ToolbarExtension.OnRightToolbarGUI += OnRightToolbarGUI;
    }

    static void OnUpdate(double timeDelta)
    {
        foreach (var toolbarItem in ToolbarGatherer.AllToolbars)
        {
            if (toolbarItem is IUpdatingToolbarItem updatingToolbarItem)
                updatingToolbarItem.Update(timeDelta);
        }
    }

    static void DrawItemsList(IReadOnlyList<IToolbarItem> toolbarItems)
    {
        foreach (var toolbarItem in toolbarItems)
        {
            if (!toolbarItem.Enabled)
                continue;
            toolbarItem.DrawInToolbar();
        }
    }

    static void OnLeftToolbarGUI()
    {
        if (!ToolbarGatherer.ToolbarsByPosition.TryGetValue(ToolbarItemPosition.Left, out var positionDictionary))
            return;

        // Left
        if (positionDictionary.TryGetValue(ToolbarItemAnchor.Left, out var leftAnchorList))
            DrawItemsList(leftAnchorList);

        GUILayout.FlexibleSpace();

        // Center
        if (positionDictionary.TryGetValue(ToolbarItemAnchor.Center, out var centerAnchorList))
            DrawItemsList(centerAnchorList);

        GUILayout.FlexibleSpace();

        // Right
        if (positionDictionary.TryGetValue(ToolbarItemAnchor.Right, out var rightAnchorList))
            DrawItemsList(rightAnchorList);
    }

    static void OnRightToolbarGUI()
    {
        if (!ToolbarGatherer.ToolbarsByPosition.TryGetValue(ToolbarItemPosition.Right, out var positionDictionary))
            return;

        // Left
        if (positionDictionary.TryGetValue(ToolbarItemAnchor.Left, out var leftAnchorList))
            DrawItemsList(leftAnchorList);

        GUILayout.FlexibleSpace();

        // Center
        if (positionDictionary.TryGetValue(ToolbarItemAnchor.Center, out var centerAnchorList))
            DrawItemsList(centerAnchorList);

        GUILayout.FlexibleSpace();

        // Right
        if (positionDictionary.TryGetValue(ToolbarItemAnchor.Right, out var rightAnchorList))
            DrawItemsList(rightAnchorList);
    }
}