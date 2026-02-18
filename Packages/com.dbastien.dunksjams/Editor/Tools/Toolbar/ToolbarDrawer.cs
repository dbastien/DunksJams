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

    private static void OnUpdate(double timeDelta)
    {
        foreach (IToolbarItem toolbarItem in ToolbarGatherer.AllToolbars)
            if (toolbarItem is IUpdatingToolbarItem updatingToolbarItem)
                updatingToolbarItem.Update(timeDelta);
    }

    private static void DrawItemsList(IReadOnlyList<IToolbarItem> toolbarItems)
    {
        foreach (IToolbarItem toolbarItem in toolbarItems)
        {
            if (!toolbarItem.Enabled)
                continue;
            toolbarItem.DrawInToolbar();
        }
    }

    private static void OnLeftToolbarGUI()
    {
        if (!ToolbarGatherer.ToolbarsByPosition.TryGetValue(ToolbarItemPosition.Left,
                out IReadOnlyDictionary<ToolbarItemAnchor, IReadOnlyList<IToolbarItem>> positionDictionary))
            return;

        // Left
        if (positionDictionary.TryGetValue(ToolbarItemAnchor.Left, out IReadOnlyList<IToolbarItem> leftAnchorList))
            DrawItemsList(leftAnchorList);

        GUILayout.FlexibleSpace();

        // Center
        if (positionDictionary.TryGetValue(ToolbarItemAnchor.Center, out IReadOnlyList<IToolbarItem> centerAnchorList))
            DrawItemsList(centerAnchorList);

        GUILayout.FlexibleSpace();

        // Right
        if (positionDictionary.TryGetValue(ToolbarItemAnchor.Right, out IReadOnlyList<IToolbarItem> rightAnchorList))
            DrawItemsList(rightAnchorList);
    }

    private static void OnRightToolbarGUI()
    {
        if (!ToolbarGatherer.ToolbarsByPosition.TryGetValue(ToolbarItemPosition.Right,
                out IReadOnlyDictionary<ToolbarItemAnchor, IReadOnlyList<IToolbarItem>> positionDictionary))
            return;

        // Left
        if (positionDictionary.TryGetValue(ToolbarItemAnchor.Left, out IReadOnlyList<IToolbarItem> leftAnchorList))
            DrawItemsList(leftAnchorList);

        GUILayout.FlexibleSpace();

        // Center
        if (positionDictionary.TryGetValue(ToolbarItemAnchor.Center, out IReadOnlyList<IToolbarItem> centerAnchorList))
            DrawItemsList(centerAnchorList);

        GUILayout.FlexibleSpace();

        // Right
        if (positionDictionary.TryGetValue(ToolbarItemAnchor.Right, out IReadOnlyList<IToolbarItem> rightAnchorList))
            DrawItemsList(rightAnchorList);
    }
}