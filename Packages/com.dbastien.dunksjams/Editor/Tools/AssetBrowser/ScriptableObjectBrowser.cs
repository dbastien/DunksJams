using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class ScriptableObjectBrowserWindow : AssetBrowserWindow<ScriptableObjectBrowserTreeView, ScriptableObjectBrowserTreeView.TreeViewItem>
{
    protected override string WinTitle => "Scriptable Objects";

    [MenuItem("‽/Asset Browser/Scriptable Objects", false, 100)]
    public static void ShowWindow() => AssetBrowserWindowManager.ShowWindow<ScriptableObjectBrowserWindow>();
}

public class ScriptableObjectBrowserTreeView : AssetBrowserTreeView<ScriptableObjectBrowserTreeView.TreeViewItem>
{
    public class TreeViewItem : AssetBrowserTreeViewItem<ScriptableObject>
    {
        public string TypeName;

        public TreeViewItem(int id, string guid, string path, ScriptableObject asset)
            : base(id, guid, path, asset) { }

        protected sealed override void Rebuild()
        {
            TypeName = TypedAsset ? TypedAsset.GetType().Name : "";
            base.Rebuild();
        }
    }

    public ScriptableObjectBrowserTreeView(TreeViewState<int> state) : base(state) { }

    protected override string[] GatherGuids(bool sceneOnly, string path) =>
        sceneOnly ? System.Array.Empty<string>() : FindAssetGuids("ScriptableObject", path);

    protected override void AddAsset(ref int id, string guid, string path)
    {
        var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
        if (asset) AllItems.Add(new TreeViewItem(id++, guid, path, asset));
    }

    protected override MultiColumnHeaderState CreateHeaderState() => new(new MultiColumnHeaderState.Column[]
    {
        CreateColumn("Object", ColumnType.Object, t => t.TypedAsset, type: typeof(ScriptableObject)),
        CreatePathColumn(),
        CreateRuntimeMemoryColumn(),
        CreateStorageSizeColumn(),
        CreateColumn("Type", ColumnType.Large, t => t.TypeName),
        CreateReferencesColumn(),
        CreateDependenciesColumn(),
        CreateWrittenColumn()
    });
}
