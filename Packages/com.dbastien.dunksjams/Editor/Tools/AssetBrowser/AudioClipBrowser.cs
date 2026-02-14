using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class AudioClipBrowserWindow : AssetBrowserWindow<AudioClipBrowserTreeView, AudioClipBrowserTreeView.TreeViewItem>
{
    protected override string WinTitle => "Audio Clips";

    [MenuItem("‽/Asset Browser/Audio Clips", false, 100)]
    public static void ShowWindow() => AssetBrowserWindowManager.ShowWindow<AudioClipBrowserWindow>();
}

public class AudioClipBrowserTreeView : AssetBrowserTreeView<AudioClipBrowserTreeView.TreeViewItem>
{
    public class TreeViewItem : AssetBrowserTreeViewItem<AudioClip, AudioImporter>
    {
        public AudioClip Clip => TypedAsset;
        public AudioImporter Importer => TypedImporter;

        public TreeViewItem(int id, string guid, string path, AudioClip asset, AudioImporter importer)
            : base(id, guid, path, asset, importer) { }
    }

    public AudioClipBrowserTreeView(TreeViewState<int> state) : base(state) { }

    protected override string[] GatherGuids(bool sceneOnly, string path) =>
        sceneOnly
            ? GatherSceneGuids<AudioSource>(src => src.clip)
            : FindAssetGuids("AudioClip", path);

    protected override void AddAsset(ref int id, string guid, string path)
    {
        var asset = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        var importer = AssetImporter.GetAtPath(path) as AudioImporter;
        if (asset && importer) AllItems.Add(new TreeViewItem(id++, guid, path, asset, importer));
    }

    protected override MultiColumnHeaderState CreateHeaderState() => new(new MultiColumnHeaderState.Column[]
    {
        CreateColumn("Object", ColumnType.Object, t => t.Clip, type: typeof(AudioClip)),
        CreatePathColumn(),
        CreateRuntimeMemoryColumn(),
        CreateStorageSizeColumn(),
        CreateColumn("Length (s)", ColumnType.Small, t => t.Clip.length),
        CreateColumn("Channels", ColumnType.Small, t => t.Clip.channels),
        CreateColumn("Frequency", ColumnType.Int, t => t.Clip.frequency),
        CreateColumn("Samples", ColumnType.Int, t => t.Clip.samples),
        CreateColumn("LoadType", ColumnType.Medium, t => t.Importer.defaultSampleSettings.loadType),
        CreateColumn("Compression", ColumnType.Medium, t => t.Importer.defaultSampleSettings.compressionFormat),
        CreateColumn("Quality", ColumnType.Small, t => t.Importer.defaultSampleSettings.quality),
        CreateColumn("ForceToMono", ColumnType.Bool, t => t.Importer.forceToMono),
        CreateColumn("Ambisonic", ColumnType.Bool, t => t.Importer.ambisonic),
        CreateColumn("LoadInBg", ColumnType.Bool, t => t.Importer.loadInBackground),
        CreateColumn("Preload", ColumnType.Bool, t => t.Importer.defaultSampleSettings.preloadAudioData),
        CreateReferencesColumn(),
        CreateDependenciesColumn(),
        CreateWrittenColumn()
    });
}
