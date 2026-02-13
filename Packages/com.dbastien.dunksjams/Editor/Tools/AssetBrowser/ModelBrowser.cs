using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;

public class ModelBrowserWindow : AssetBrowserWindow<ModelBrowserTreeView, ModelBrowserTreeView.TreeViewItem>
{
    protected override string WinTitle => "Models";

    [MenuItem("‽/Asset Browser/Models", false, 100)]
    public static void ShowWindow() => AssetBrowserWindowManager.ShowWindow<ModelBrowserWindow>();

    protected override void Rebuild()
    {
        treeViewState ??= new TreeViewState<int>();
        var items = treeView?.AllItems ?? new List<ModelBrowserTreeView.TreeViewItem>(32);
        treeView = new ModelBrowserTreeView(treeViewState) { AllItems = items };
        treeView.Reload();
    }
}

public class ModelBrowserTreeView : AssetBrowserTreeView<ModelBrowserTreeView.TreeViewItem>
{
    public class TreeViewItem : AssetBrowserTreeViewItem
    {
        public override Object Asset => Mesh;
        public Mesh Mesh;
        public ModelImporter Importer;
        public override string AssetName => Asset.name;
        public override AssetImporter AssetImporter => Importer;

        public TreeViewItem(int id, string guid, string path, Mesh asset, ModelImporter importer) : base(id, guid, path)
        {
            Mesh = asset;
            Importer = importer;
            Rebuild();
        }

        protected sealed override void Rebuild()
        {
            RuntimeMemory = Profiler.GetRuntimeMemorySizeLong(Mesh);
            //StorageSize = new FileInfo(AssetDatabase.GetAssetPath(Mesh)).Length;

            base.Rebuild();
        }
    }

    public ModelBrowserTreeView(TreeViewState<int> state) : base(state)
    {
        multiColumnHeader = new MultiColumnHeader(CreateHeaderState(this));
        InitHeader(multiColumnHeader);
    }

    protected override string[] GatherGuids(bool sceneOnly, string path)
    {
        if (!sceneOnly) return AssetDatabase.FindAssets("t:Model", new[] { path });

        List<string> guids = new();

        var meshFilters = Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var meshFilter in meshFilters)
        {
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(meshFilter.sharedMesh, out var guid, out _))
                guids.Add(guid);
        }

        return guids.ToArray();
    }

    protected override void AddAsset(ref int id, string guid, string path)
    {
        var asset = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        var importer = AssetImporter.GetAtPath(path) as ModelImporter;

        if (asset && importer) AllItems.Add(new TreeViewItem(id++, guid, path, asset, importer));
    }

    MultiColumnHeaderState CreateHeaderState(ModelBrowserTreeView tv)
    {
        MultiColumnHeaderState.Column[] columns =
        {
            CreateColumn("Object", ColumnType.Object, t => t.Asset as Mesh, type: typeof(Mesh)),
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
        };

        return new MultiColumnHeaderState(columns);
    }
}