using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class ModelBrowserWindow : AssetBrowserWindow<ModelBrowserTreeView, ModelBrowserTreeView.TreeViewItem>
{
    protected override string WinTitle => "Models";

    [MenuItem("‽/Asset Browser/Models", false, 100)]
    public static void ShowWindow() => AssetBrowserWindowManager.ShowWindow<ModelBrowserWindow>();
}

public class ModelBrowserTreeView : AssetBrowserTreeView<ModelBrowserTreeView.TreeViewItem>
{
    public class TreeViewItem : AssetBrowserTreeViewItem<Mesh, ModelImporter>
    {
        public Mesh Mesh => TypedAsset;
        public ModelImporter Importer => TypedImporter;

        public TreeViewItem(int id, string guid, string path, Mesh asset, ModelImporter importer)
            : base(id, guid, path, asset, importer) { }
    }

    public ModelBrowserTreeView(TreeViewState<int> state) : base(state) { }

    protected override string[] GatherGuids(bool sceneOnly, string path) =>
        sceneOnly
            ? GatherSceneGuids<MeshFilter>(mf => mf.sharedMesh)
            : FindAssetGuids("Model", path);

    protected override void AddAsset(ref int id, string guid, string path)
    {
        var asset = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        var importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (asset && importer) AllItems.Add(new TreeViewItem(id++, guid, path, asset, importer));
    }

    protected override MultiColumnHeaderState CreateHeaderState() => new(new MultiColumnHeaderState.Column[]
    {
        CreateColumn("Object", ColumnType.Object, t => t.Mesh, type: typeof(Mesh)),
        CreatePathColumn(),
        CreateRuntimeMemoryColumn(),
        CreateStorageSizeColumn(),
        CreateColumn("Verts", ColumnType.Int, t => t.Mesh.vertexCount),
        CreateColumn("Tris", ColumnType.Int, t => t.Mesh.triangles.Length / 3),
        CreateColumn("SubMeshes", ColumnType.Int, t => t.Mesh.subMeshCount),
        CreateColumn("Anims", ColumnType.Bool, t => t.Importer.importAnimation),
        CreateColumn("IsReadable", ColumnType.Bool, t => t.Importer.isReadable),
        CreateReferencesColumn(),
        CreateDependenciesColumn(),
        CreateWrittenColumn()
    });
}
