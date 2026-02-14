using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class ComputeShaderBrowserWindow : AssetBrowserWindow<ComputeShaderBrowserTreeView, ComputeShaderBrowserTreeView.TreeViewItem>
{
    protected override string WinTitle => "Compute Shaders";

    [MenuItem("‽/Asset Browser/Compute Shaders", false, 100)]
    public static void ShowWindow() => AssetBrowserWindowManager.ShowWindow<ComputeShaderBrowserWindow>();
}

public class ComputeShaderBrowserTreeView : AssetBrowserTreeView<ComputeShaderBrowserTreeView.TreeViewItem>
{
    public class TreeViewItem : AssetBrowserTreeViewItem<ComputeShader>
    {
        public TreeViewItem(int id, string guid, string path, ComputeShader asset)
            : base(id, guid, path, asset) { }
    }

    public ComputeShaderBrowserTreeView(TreeViewState<int> state) : base(state) { }

    protected override string[] GatherGuids(bool sceneOnly, string path) =>
        sceneOnly ? System.Array.Empty<string>() : FindAssetGuids("ComputeShader", path);

    protected override void AddAsset(ref int id, string guid, string path)
    {
        var asset = AssetDatabase.LoadAssetAtPath<ComputeShader>(path);
        if (asset) AllItems.Add(new TreeViewItem(id++, guid, path, asset));
    }

    protected override MultiColumnHeaderState CreateHeaderState() => new(new MultiColumnHeaderState.Column[]
    {
        CreateColumn("Object", ColumnType.Object, t => t.TypedAsset, type: typeof(ComputeShader)),
        CreatePathColumn(),
        CreateRuntimeMemoryColumn(),
        CreateStorageSizeColumn(),
        CreateReferencesColumn(),
        CreateDependenciesColumn(),
        CreateWrittenColumn()
    });
}
