using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;

public class ScriptBrowserWindow : AssetBrowserWindow<ScriptBrowserTreeView, ScriptBrowserTreeView.TreeViewItem>
{
    protected override string WinTitle => "Scripts";
    
    [MenuItem("‽/Asset Browser/Scripts", false, 100)]
    public static void ShowWindow() => AssetBrowserWindowManager.ShowWindow<ScriptBrowserWindow>();
    
    protected override void Rebuild()
    {
        treeViewState ??= new();
        List<ScriptBrowserTreeView.TreeViewItem> items = treeView?.AllItems ?? new(32);
        treeView = new(treeViewState) { AllItems = items };
        treeView.Reload();
    }
}

public class ScriptBrowserTreeView : AssetBrowserTreeView<ScriptBrowserTreeView.TreeViewItem>
{
    public class TreeViewItem : AssetBrowserTreeViewItem
    {
        public override Object Asset => Script;
        public MonoScript Script;
        public MonoImporter Importer;
        public override string AssetName => Asset.name;
        public override AssetImporter AssetImporter => Importer;

        public TreeViewItem(int id, string guid, string path, MonoScript asset, MonoImporter importer) : base(id, guid, path)
        {
            Script = asset;
            Importer = importer;
            Rebuild();
        }

        protected sealed override void Rebuild()
        {
            RuntimeMemory = Profiler.GetRuntimeMemorySizeLong(Script);
            //StorageSize = new FileInfo(AssetDatabase.GetAssetPath(Mesh)).Length;
            
            base.Rebuild();
        }
    }

    public ScriptBrowserTreeView(TreeViewState<int> state) : base(state)
    {
        multiColumnHeader = new(CreateHeaderState(this));
        InitHeader(multiColumnHeader);
    }

    protected override string[] GatherGuids(bool sceneOnly, string path)
    {
        if (!sceneOnly) return AssetDatabase.FindAssets("t:Script", new[] { path });

        List<string> guids = new();
        var scripts = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (var script in scripts)
            if (MonoScript.FromMonoBehaviour(script) is { } monoScript &&
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(monoScript, out string guid, out _))
            {
                guids.Add(guid);
            }

        return guids.ToArray();
    }

    protected override void AddAsset(ref int id, string guid, string path)
    {
        var asset = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
        var importer = AssetImporter.GetAtPath(path) as MonoImporter;
        
        if (asset && importer) AllItems.Add(new(id++, guid, path, asset, importer));
    }
    
    MultiColumnHeaderState CreateHeaderState(ScriptBrowserTreeView tv)
    {
        MultiColumnHeaderState.Column[] columns =
        {
            CreateColumn("Object", ColumnType.Object, t => t.Asset as MonoBehaviour, type: typeof(MonoBehaviour)),
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