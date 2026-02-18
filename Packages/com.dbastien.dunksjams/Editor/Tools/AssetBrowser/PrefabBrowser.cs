using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class PrefabBrowserWindow : AssetBrowserWindow<PrefabBrowserTreeView, PrefabBrowserTreeView.TreeViewItem>
{
    protected override string WinTitle => "Prefabs";

    [MenuItem("‽/Asset Browser/Prefabs", false, 100)]
    public static void ShowWindow() => AssetBrowserWindowManager.ShowWindow<PrefabBrowserWindow>();
}

public class PrefabBrowserTreeView : AssetBrowserTreeView<PrefabBrowserTreeView.TreeViewItem>
{
    public class TreeViewItem : AssetBrowserTreeViewItem<GameObject>
    {
        public int ComponentCount;
        public int ChildCount;
        public int MissingScriptCount;
        public PrefabAssetType PrefabType;

        public TreeViewItem(int id, string guid, string path, GameObject asset) : base(id, guid, path, asset) { }

        protected sealed override void Rebuild()
        {
            ComponentCount = TypedAsset.GetComponentsInChildren<Component>(true).Length;
            ChildCount = TypedAsset.transform.childCount;
            MissingScriptCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(TypedAsset);
            PrefabType = PrefabUtility.GetPrefabAssetType(TypedAsset);
            base.Rebuild();
        }
    }

    public PrefabBrowserTreeView(TreeViewState<int> state) : base(state) { }

    protected override string[] GatherGuids(bool sceneOnly, string path) =>
        sceneOnly ? System.Array.Empty<string>() : FindAssetGuids("Prefab", path);

    protected override void AddAsset(ref int id, string guid, string path)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (asset) AllItems.Add(new TreeViewItem(id++, guid, path, asset));
    }

    protected override MultiColumnHeaderState CreateHeaderState() => new(new MultiColumnHeaderState.Column[]
    {
        CreateColumn("Object", ColumnType.Object, t => t.TypedAsset, type: typeof(GameObject)),
        CreatePathColumn(),
        CreateRuntimeMemoryColumn(),
        CreateStorageSizeColumn(),
        CreateColumn("Components", ColumnType.Int, t => t.ComponentCount),
        CreateColumn("Children", ColumnType.Int, t => t.ChildCount),
        CreateColumn("MissingScripts", ColumnType.Int, t => t.MissingScriptCount),
        CreateColumn("PrefabType", ColumnType.Medium, t => t.PrefabType),
        CreateReferencesColumn(),
        CreateDependenciesColumn(),
        CreateWrittenColumn()
    });
}