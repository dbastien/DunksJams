using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class ScriptBrowserWindow : AssetBrowserWindow<ScriptBrowserTreeView, ScriptBrowserTreeView.TreeViewItem>
{
    protected override string WinTitle => "Scripts";

    [MenuItem("‽/Asset Browser/Scripts", false, 100)]
    public static void ShowWindow() => AssetBrowserWindowManager.ShowWindow<ScriptBrowserWindow>();
}

public class ScriptBrowserTreeView : AssetBrowserTreeView<ScriptBrowserTreeView.TreeViewItem>
{
    public class TreeViewItem : AssetBrowserTreeViewItem<MonoScript, MonoImporter>
    {
        public MonoScript Script => TypedAsset;

        public TreeViewItem(int id, string guid, string path, MonoScript asset, MonoImporter importer)
            : base(id, guid, path, asset, importer) { }
    }

    public ScriptBrowserTreeView(TreeViewState<int> state) : base(state) { }

    protected override string[] GatherGuids(bool sceneOnly, string path)
    {
        if (!sceneOnly) return FindAssetGuids("Script", path);

        var scripts = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        var guids = new System.Collections.Generic.List<string>();
        foreach (var script in scripts)
            if (MonoScript.FromMonoBehaviour(script) is { } monoScript &&
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(monoScript, out var guid, out _))
                guids.Add(guid);
        return guids.ToArray();
    }

    protected override void AddAsset(ref int id, string guid, string path)
    {
        var asset = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
        var importer = AssetImporter.GetAtPath(path) as MonoImporter;
        if (asset && importer) AllItems.Add(new TreeViewItem(id++, guid, path, asset, importer));
    }

    protected override MultiColumnHeaderState CreateHeaderState() => new(new MultiColumnHeaderState.Column[]
    {
        CreateColumn("Object", ColumnType.Object, t => t.Script, type: typeof(MonoScript)),
        CreatePathColumn(),
        CreateRuntimeMemoryColumn(),
        CreateStorageSizeColumn(),
        CreateReferencesColumn(),
        CreateDependenciesColumn(),
        CreateWrittenColumn()
    });
}
