using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public static class AssetBrowserWindowManager
{
    private static readonly Vector2 WindowMinSize = new(610, 100);

    private static readonly Type[] WinTypes = typeof(AssetBrowserWindow<,>).GetDerived().ToArray();

    [MenuItem("‽/Asset Browser/Open All", false, 200)]
    public static void BuildAllWindows()
    {
        foreach (Type type in WinTypes)
        {
            MethodInfo method =
                typeof(AssetBrowserWindowManager).GetMethod(nameof(ShowWindow),
                    BindingFlags.Public | BindingFlags.Static);
            MethodInfo genericMethod = method?.MakeGenericMethod(type);
            genericMethod?.Invoke(null, new object[] { });
        }
    }

    public static EditorWindow ShowWindow<T>() where T : EditorWindow
    {
        //dock with other asset browser windows (WinTypes)
        var w = EditorWindow.GetWindow<T>(WinTypes);
        w.minSize = WindowMinSize;
        PropertyInfo titleProperty = typeof(T).GetProperty("WinTitle", BindingFlags.Instance | BindingFlags.NonPublic);
        w.titleContent = new GUIContent(titleProperty?.GetValue(w) as string ?? typeof(T).Name);
        typeof(T).GetMethod("Rebuild")?.Invoke(w, null);
        w.Show();
        return w;
    }
}

public class AssetBrowserWindow<TTree, TItem> : EditorWindow
    where TTree : AssetBrowserTreeView<TItem>
    where TItem : AssetBrowserTreeViewItem
{
    protected virtual string WinTitle => "Assets";
    protected virtual string AssetTypeName => "Asset";

    [SerializeField] protected TreeViewState<int> treeViewState;
    [SerializeReference] protected TTree treeView;
    protected SearchField searchField;
    protected bool sceneOnly;
    protected string assetPath = "Assets";

    [SerializeField] protected AssetImporterPlatform platform;

    [NonSerialized] protected string outPath;

    [NonSerialized] private bool initialized;
    [NonSerialized] private bool initializedStatic;

    private static Texture saveIcon;
    private static Texture findIcon;

    private Rect saveDropDownRect;
    private Rect gatherDropDownRect;

    public delegate void SaveFunction();

    protected virtual void AddCustomSaveFunctions(GenericMenu menu) { }

    protected Rect topToolBarRect => new(margin, margin, position.width - margin, 20f);

    //protected Rect botToolBarRect => new Rect(margin, position.height - 20f - margin, position.width - margin, 20f);
    protected Rect treeViewRect => new(margin, 30f, position.width - margin, position.height - 30f);

    protected const int margin = 5;

    private void OnGUI()
    {
        InitIfNeeded();
        TopToolBar(topToolBarRect);
        treeView?.OnGUI(treeViewRect);
    }

    private void TopToolBar(Rect bar)
    {
        using var _ = new EditorGUILayout.HorizontalScope();

        sceneOnly = EditorGUILayout.ToggleLeft(new GUIContent("Scene", "Search only the active scene"), sceneOnly,
            GUILayout.Width(60));
        if (sceneOnly) return;

        assetPath = EditorGUILayout.TextField(assetPath);
        if (GUILayout.Button(new GUIContent("…", "Select folder to search"), GUILayout.Width(22)))
        {
            string absPath = EditorUtility.OpenFolderPanel("Folder to search (recursively)", assetPath, "");
            if (absPath.StartsWithFast(IOUtils.ProjectRootFolder))
            {
                assetPath = absPath[IOUtils.ProjectRootFolder.Length..];
                if (assetPath.StartsWith("/") || assetPath.StartsWith("\\")) assetPath = assetPath[1..];
            }
            else if (!string.IsNullOrEmpty(absPath))
            {
                DLog.LogE($"Asset path must start with {IOUtils.ProjectRootFolder}");
            }
        }

        if (GUILayout.Button("Find", GUILayout.Width(40)))
            treeView?.GatherAssets(sceneOnly, assetPath);

        GUILayout.Space(10);
        GUILayout.FlexibleSpace();

        GUILayout.FlexibleSpace();
        platform = (AssetImporterPlatform)EditorGUILayout.EnumPopup(platform, GUILayout.Width(85));
        GUILayout.FlexibleSpace();

        DrawButtonWithMenu(findIcon, ref gatherDropDownRect, ShowGatherMenu);
        DrawButtonWithMenu(saveIcon, ref saveDropDownRect, ShowSaveMenu);

        GUILayout.FlexibleSpace();

        Rect searchRect = GUILayoutUtility.GetRect(150, EditorGUIUtility.singleLineHeight);
        treeView.searchString = searchField.OnGUI(searchRect, treeView.searchString);
        GUILayout.Label($"{treeView.FilteredCount}/{treeView.AllItems.Count}");
    }

    private void DrawButtonWithMenu(Texture icon, ref Rect dropDownRect, Action showMenu)
    {
        if (EditorGUILayout.DropdownButton(new GUIContent(icon), FocusType.Passive))
            showMenu();

        if (Event.current.type == EventType.Repaint)
            dropDownRect = GUILayoutUtility.GetLastRect();
    }

    private void ShowGatherMenu()
    {
        var menu = new GenericMenu();
        menu.AddItem(new GUIContent("References"), false, () => treeView?.FindReferences());
        menu.AddItem(new GUIContent("Dependencies"), false, () => treeView?.FindDependencies());
        menu.DropDown(gatherDropDownRect);
    }

    private void ShowSaveMenu()
    {
        var menu = new GenericMenu();
        menu.AddItem(new GUIContent("Rows"), false, () => SavePanel(Save));
        menu.AddItem(new GUIContent("References"), false, () => SavePanel(SaveReferences));
        menu.AddItem(new GUIContent("Dependencies"), false, () => SavePanel(SaveDependencies));
        AddCustomSaveFunctions(menu);
        menu.DropDown(saveDropDownRect);
    }

    protected void SavePanel(SaveFunction func)
    {
        string dir = string.IsNullOrEmpty(outPath)
            ? IOUtils.ProjectRootFolder
            : System.IO.Path.GetDirectoryName(outPath);
        outPath = EditorUtility.SaveFilePanel("Save rows to csv file", dir,
            typeof(TTree).Name + func.GetMethodInfo().Name, "csv");
        if (!string.IsNullOrEmpty(outPath)) func();
    }

    private void Save()
    {
        List<TItem> rows = treeView.AllItems;
        int[] visibleCols = treeView.multiColumnHeader.state.visibleColumns;
        using var file = new System.IO.StreamWriter(outPath);

        var line = "";
        for (var i = 0; i < visibleCols.Length; ++i)
        {
            var c = treeView.multiColumnHeader.GetColumn(visibleCols[i]) as AssetBrowserTreeView<TItem>.Column;
            line += c?.Header + (i != visibleCols.Length - 1 ? ',' : ' ');
        }

        file.WriteLine(line);

        foreach (TItem r in rows)
        {
            if (!treeView.DoesItemMatchSearch(r)) continue;
            line = "";
            for (var i = 0; i < visibleCols.Length; ++i)
            {
                var c = treeView.multiColumnHeader.GetColumn(visibleCols[i]) as AssetBrowserTreeView<TItem>.Column;
                line += (c?.text != null ? c.text(r) : "") + (i != visibleCols.Length - 1 ? ',' : ' ');
            }

            file.WriteLine(line);
        }
    }

    private void SaveReferences()
    {
        List<TItem> rows = treeView.AllItems;
        using System.IO.StreamWriter file = new(outPath);
        foreach (TItem r in rows)
        {
            if (!treeView.DoesItemMatchSearch(r)) continue;
            var line = $"{r.AssetPath}, {r.AssetName}";
            List<string> paths = r.Refs.Select(AssetDatabase.GetAssetPath).ToList();
            foreach (string path in paths) line += $", {path}";
            file.WriteLine(line);
        }
    }

    //todo: do the to
    private void SaveDependencies() { }

    private void ChangePlatform(AssetImporterPlatform p) { }

    protected virtual void Rebuild()
    {
        treeViewState ??= new TreeViewState<int>();
        List<TItem> items = treeView?.AllItems ?? new List<TItem>(32);
        treeView = (TTree)Activator.CreateInstance(typeof(TTree), treeViewState);
        treeView.AllItems = items;
        treeView.Reload();
    }

    private void InitIfNeeded()
    {
        if (!initializedStatic)
        {
            saveIcon = EditorGUIUtility.IconContent("d_SaveAs").image;
            findIcon = EditorGUIUtility.IconContent("Import").image;
            initializedStatic = true;
        }

        if (initialized) return;
        Rebuild();
        searchField = new SearchField();
        initialized = true;
    }
}