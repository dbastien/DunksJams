using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;

public class SceneBrowserWindow : AssetBrowserWindow<SceneBrowserTreeView, SceneBrowserTreeView.TreeViewItem>
{
    protected override string WinTitle => "Scenes";
    
    [MenuItem("‽/Asset Browser/Scenes", false, 100)]
    public static void ShowWindow() => AssetBrowserWindowManager.ShowWindow<SceneBrowserWindow>();
    
    protected override void Rebuild()
    {
        treeViewState ??= new();
        List<SceneBrowserTreeView.TreeViewItem> items = treeView?.AllItems ?? new(32);
        treeView = new(treeViewState) { AllItems = items };
        treeView.Reload();
    }
}

public class SceneBrowserTreeView : AssetBrowserTreeView<SceneBrowserTreeView.TreeViewItem>
{
    public class TreeViewItem : AssetBrowserTreeViewItem
    {
        public override Object Asset => Scene;
        public SceneAsset Scene;
        public AssetImporter Importer;
        public override string AssetName => Asset.name;
        public override AssetImporter AssetImporter => Importer;

        public TreeViewItem(int id, string guid, string path, SceneAsset asset, AssetImporter importer) : base(id, guid, path)
        {
            Scene = asset;
            Importer = importer;
            Rebuild();
        }

        protected sealed override void Rebuild()
        {
            RuntimeMemory = Profiler.GetRuntimeMemorySizeLong(Scene);
            //StorageSize = new FileInfo(AssetDatabase.GetAssetPath(Mesh)).Length;
            
            base.Rebuild();
        }
    }

    public SceneBrowserTreeView(TreeViewState<int> state) : base(state)
    {
        multiColumnHeader = new(CreateHeaderState(this));
        InitHeader(multiColumnHeader);
    }

    protected override string[] GatherGuids(bool sceneOnly, string path)
    {
        if (!sceneOnly) return AssetDatabase.FindAssets("t:SceneAsset", new[] { path });

        List<string> guids = new();
        var scenes = Object.FindObjectsByType<SceneAsset>(FindObjectsSortMode.None);

        foreach (var scene in scenes)
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(scene, out string guid, out long _))
                guids.Add(guid);

        return guids.ToArray();
    }

    protected override void AddAsset(ref int id, string guid, string path)
    {
        var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
        var importer = AssetImporter.GetAtPath(path);
        
        if (asset && importer) AllItems.Add(new(id++, guid, path, asset, importer));
    }
    
    MultiColumnHeaderState CreateHeaderState(SceneBrowserTreeView tv)
    {
        MultiColumnHeaderState.Column[] columns =
        {
            CreateColumn("Object", ColumnType.Object, t => t.Asset as SceneAsset, type: typeof(SceneAsset)),
            CreatePathColumn(),
            CreateRuntimeMemoryColumn(),
            CreateStorageSizeColumn(),
            CreateReferencesColumn(),
            CreateDependenciesColumn(),
            CreateWrittenColumn()
        };
        
        return new(columns);
    }
}