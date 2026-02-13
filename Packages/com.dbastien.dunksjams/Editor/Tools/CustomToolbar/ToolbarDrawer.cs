using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class ToolbarDrawer
{
    static ToolbarDrawer()
    {
        CustomToolbarExtension.OnUpdate -= OnUpdate;
        CustomToolbarExtension.OnLeftToolbarGUI -= OnLeftToolbarGUI;
        CustomToolbarExtension.OnRightToolbarGUI -= OnRightToolbarGUI;
        
        CustomToolbarExtension.OnUpdate += OnUpdate;
        CustomToolbarExtension.OnLeftToolbarGUI += OnLeftToolbarGUI;
        CustomToolbarExtension.OnRightToolbarGUI += OnRightToolbarGUI;
    }

    private static void OnUpdate(double timeDelta)
    {
        foreach (IToolbarItem toolbarItem in ToolbarGatherer.AllToolbars)
        {
            if (toolbarItem is IUpdatingToolbarItem updatingToolbarItem)
            {
                updatingToolbarItem.Update(timeDelta);
            }
        }
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
        if (!ToolbarGatherer.ToolbarsByPosition.TryGetValue(ToolbarItemPosition.Left, out var positionDictionary))
            return;

        // Left
        if (positionDictionary.TryGetValue(ToolbarItemAnchor.Left, out var leftAnchorList))
        {
            DrawItemsList(leftAnchorList);
        }
        
        GUILayout.FlexibleSpace();
        
        // Center
        if (positionDictionary.TryGetValue(ToolbarItemAnchor.Center, out var centerAnchorList))
        {
            DrawItemsList(centerAnchorList);
        }
        
        GUILayout.FlexibleSpace();
        
        // Right
        if (positionDictionary.TryGetValue(ToolbarItemAnchor.Right, out var rightAnchorList))
        {
            DrawItemsList(rightAnchorList);
        }
    }

    private static void OnRightToolbarGUI()
    {
        if (!ToolbarGatherer.ToolbarsByPosition.TryGetValue(ToolbarItemPosition.Right, out var positionDictionary))
            return;

        // Left
        if (positionDictionary.TryGetValue(ToolbarItemAnchor.Left, out var leftAnchorList))
        {
            DrawItemsList(leftAnchorList);
        }
        
        GUILayout.FlexibleSpace();
        
        // Center
        if (positionDictionary.TryGetValue(ToolbarItemAnchor.Center, out var centerAnchorList))
        {
            DrawItemsList(centerAnchorList);
        }
        
        GUILayout.FlexibleSpace();
        
        // Right
        if (positionDictionary.TryGetValue(ToolbarItemAnchor.Right, out var rightAnchorList))
        {
            DrawItemsList(rightAnchorList);
        }
    }
}
