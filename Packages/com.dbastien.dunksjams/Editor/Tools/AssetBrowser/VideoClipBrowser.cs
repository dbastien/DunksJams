using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Video;

public class VideoClipBrowserWindow : AssetBrowserWindow<VideoClipBrowserTreeView, VideoClipBrowserTreeView.TreeViewItem>
{
    protected override string WinTitle => "Video Clips";

    [MenuItem("‽/Asset Browser/Video Clips", false, 100)]
    public static void ShowWindow() => AssetBrowserWindowManager.ShowWindow<VideoClipBrowserWindow>();
}

public class VideoClipBrowserTreeView : AssetBrowserTreeView<VideoClipBrowserTreeView.TreeViewItem>
{
    public class TreeViewItem : AssetBrowserTreeViewItem<VideoClip, VideoClipImporter>
    {
        public VideoClip Clip => TypedAsset;
        public VideoClipImporter Importer => TypedImporter;

        public TreeViewItem(int id, string guid, string path, VideoClip asset, VideoClipImporter importer)
            : base(id, guid, path, asset, importer) { }
    }

    public VideoClipBrowserTreeView(TreeViewState<int> state) : base(state) { }

    protected override string[] GatherGuids(bool sceneOnly, string path) =>
        sceneOnly
            ? GatherSceneGuids<VideoPlayer>(vp => vp.clip)
            : FindAssetGuids("VideoClip", path);

    protected override void AddAsset(ref int id, string guid, string path)
    {
        var asset = AssetDatabase.LoadAssetAtPath<VideoClip>(path);
        var importer = AssetImporter.GetAtPath(path) as VideoClipImporter;
        if (asset && importer) AllItems.Add(new TreeViewItem(id++, guid, path, asset, importer));
    }

    protected override MultiColumnHeaderState CreateHeaderState() => new(new MultiColumnHeaderState.Column[]
    {
        CreateColumn("Object", ColumnType.Object, t => t.Clip, type: typeof(VideoClip)),
        CreatePathColumn(),
        CreateRuntimeMemoryColumn(),
        CreateStorageSizeColumn(),
        CreateColumn("Width", ColumnType.Int, t => t.Clip.width),
        CreateColumn("Height", ColumnType.Int, t => t.Clip.height),
        CreateColumn("FrameRate", ColumnType.Small, t => t.Clip.frameRate),
        CreateColumn("Frames", ColumnType.Int, t => t.Clip.frameCount),
        CreateColumn("Length (s)", ColumnType.Small, t => t.Clip.length),
        CreateColumn("AudioTracks", ColumnType.Int, t => t.Clip.audioTrackCount),
        CreateReferencesColumn(),
        CreateDependenciesColumn(),
        CreateWrittenColumn()
    });
}
