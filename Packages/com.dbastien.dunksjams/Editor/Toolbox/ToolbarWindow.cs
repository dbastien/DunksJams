using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>Editor toolbar window hosting configurable toolsets.</summary>
public class ToolbarWindow : EditorWindow
{
    static GUIContent optionsButtonContent;
    static GUIContent configureContent;

    List<IToolset> toolsets;

    public List<IToolset> Toolsets => toolsets;

    [MenuItem("Tools/Toolbox ‽")]
    public static void ShowWindow()
    {
        var w = GetWindow<ToolbarWindow>(false, "Toolbox");
        w.minSize = new Vector2(400, 20);
        w.maxSize = new Vector2(10000, 20);
    }

    public static void CloseWindow()
    {
        if (HasOpenInstances<ToolbarWindow>())
            GetWindow<ToolbarWindow>().Close();
    }

    public static ToolbarWindow GetWindowIfOpened()
    {
        return HasOpenInstances<ToolbarWindow>() ? GetWindow<ToolbarWindow>() : null;
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    static void OnScriptsReloaded()
    {
        if (HasOpenInstances<ToolbarNewToolsetWindow>())
            GetWindow<ToolbarNewToolsetWindow>().Close();
    }

    void Setup()
    {
        optionsButtonContent = EditorGUIUtility.TrIconContent("_Popup", "Toolbox options");
        configureContent = new GUIContent("Configure", "Customize toolbar");
        toolsets = new List<IToolset>();
        CreateToolsetsFromUserSettings();
    }

    void Teardown()
    {
        foreach (var t in toolsets)
            t.Teardown();
        toolsets?.Clear();
        toolsets = null;
    }

    void OnEnable() => Setup();
    void OnDisable() => Teardown();

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        for (var i = 0; i < toolsets.Count; i++)
        {
            toolsets[i].Draw();
            if (i < toolsets.Count - 1) EditorGUILayout.Space();
        }

        GUILayout.FlexibleSpace();

        var optRect = GUILayoutUtility.GetRect(optionsButtonContent, ToolbarStyles.ToolbarButtonRightStyle);
        if (GUI.Button(optRect, optionsButtonContent, ToolbarStyles.ToolbarButtonRightStyle))
        {
            var menu = new GenericMenu();
            menu.AddItem(configureContent, false, () => SettingsService.OpenUserPreferences("Preferences/Toolbox"));
            menu.DropDown(optRect);
        }

        EditorGUILayout.EndHorizontal();
    }

    public void Reload()
    {
        CreateToolsetsFromUserSettings();
        Repaint();
    }

    void CreateToolsetsFromUserSettings()
    {
        foreach (var t in toolsets)
            t.Teardown();
        toolsets.Clear();

        var settings = ToolboxUserSettings.GetOrCreateSettings();
        foreach (var typeName in settings.Toolsets)
        {
            var toolset = Singleton<Kernel>.Instance.ToolsetLibrary.CreateToolset(typeName);
            if (toolset != null)
                toolsets.Add(toolset);
            else
                DLog.LogE($"Unable to create toolset: {typeName}");
        }
    }
}
