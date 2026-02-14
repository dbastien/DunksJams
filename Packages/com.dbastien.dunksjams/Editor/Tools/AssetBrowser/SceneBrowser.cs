using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class SceneBrowserWindow : AssetBrowserWindow<SceneBrowserTreeView, SceneBrowserTreeView.TreeViewItem>
{
    protected override string WinTitle => "Scenes";

    [MenuItem("‽/Asset Browser/Scenes", false, 100)]
    public static void ShowWindow() => AssetBrowserWindowManager.ShowWindow<SceneBrowserWindow>();
}

public class SceneBrowserTreeView : AssetBrowserTreeView<SceneBrowserTreeView.TreeViewItem>
{
    public class TreeViewItem : AssetBrowserTreeViewItem<SceneAsset, AssetImporter>
    {
        public TreeViewItem(int id, string guid, string path, SceneAsset asset, AssetImporter importer)
            : base(id, guid, path, asset, importer) { }
    }

    public SceneBrowserTreeView(TreeViewState<int> state) : base(state) { }

    protected override string[] GatherGuids(bool sceneOnly, string path) =>
        sceneOnly
            ? GatherSceneGuids<Transform>(_ => null) // scenes can't be gathered from scene
            : FindAssetGuids("SceneAsset", path);

    protected override void AddAsset(ref int id, string guid, string path)
    {
        var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
        var importer = AssetImporter.GetAtPath(path);
        if (asset && importer) AllItems.Add(new TreeViewItem(id++, guid, path, asset, importer));
    }

    protected override MultiColumnHeaderState CreateHeaderState() => new(new MultiColumnHeaderState.Column[]
    {
        CreateColumn("Object", ColumnType.Object, t => t.TypedAsset, type: typeof(SceneAsset)),
        CreatePathColumn(),
        CreateRuntimeMemoryColumn(),
        CreateStorageSizeColumn(),
        CreateReferencesColumn(),
        CreateDependenciesColumn(),
        CreateWrittenColumn()
    });
}
