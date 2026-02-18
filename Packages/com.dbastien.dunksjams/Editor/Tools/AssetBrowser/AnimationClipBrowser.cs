using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class AnimationClipBrowserWindow
    : AssetBrowserWindow<AnimationClipBrowserTreeView, AnimationClipBrowserTreeView.TreeViewItem>
{
    protected override string WinTitle => "Animation Clips";

    [MenuItem("‽/Asset Browser/Animation Clips", false, 100)]
    public static void ShowWindow() => AssetBrowserWindowManager.ShowWindow<AnimationClipBrowserWindow>();
}

public class AnimationClipBrowserTreeView : AssetBrowserTreeView<AnimationClipBrowserTreeView.TreeViewItem>
{
    public class TreeViewItem : AssetBrowserTreeViewItem<AnimationClip, AssetImporter>
    {
        public AnimationClip Clip => TypedAsset;

        public TreeViewItem(int id, string guid, string path, AnimationClip asset, AssetImporter importer)
            : base(id, guid, path, asset, importer) { }
    }

    public AnimationClipBrowserTreeView(TreeViewState<int> state) : base(state) { }

    protected override string[] GatherGuids(bool sceneOnly, string path)
    {
        if (!sceneOnly) return FindAssetGuids("AnimationClip", path);

        var clips = new System.Collections.Generic.HashSet<Object>();
        foreach (Animator animator in Object.FindObjectsByType<Animator>(FindObjectsInactive.Include,
                     FindObjectsSortMode.None))
        {
            if (!animator.runtimeAnimatorController) continue;
            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
                if (clip)
                    clips.Add(clip);
        }

        return AssetGuidsFromObjects(clips);
    }

    protected override void AddAsset(ref int id, string guid, string path)
    {
        AssetImporter importer = AssetImporter.GetAtPath(path);
        if (importer == null) return;
        foreach (Object obj in AssetDatabase.LoadAllAssetsAtPath(path))
        {
            if (obj is not AnimationClip clip || clip.name.StartsWith("__preview")) continue;
            AllItems.Add(new TreeViewItem(id++, guid, path, clip, importer));
        }
    }

    protected override MultiColumnHeaderState CreateHeaderState() => new(new MultiColumnHeaderState.Column[]
    {
        CreateColumn("Object", ColumnType.Object, t => t.Clip, type: typeof(AnimationClip)),
        CreatePathColumn(),
        CreateRuntimeMemoryColumn(),
        CreateColumn("Length (s)", ColumnType.Small, t => t.Clip.length),
        CreateColumn("FrameRate", ColumnType.Small, t => t.Clip.frameRate),
        CreateColumn("WrapMode", ColumnType.Medium, t => t.Clip.wrapMode),
        CreateColumn("Looping", ColumnType.Bool, t => t.Clip.isLooping),
        CreateColumn("Legacy", ColumnType.Bool, t => t.Clip.legacy),
        CreateColumn("HumanMotion", ColumnType.Bool, t => t.Clip.humanMotion),
        CreateColumn("Events", ColumnType.Int, t => t.Clip.events.Length),
        CreateColumn("Empty", ColumnType.Bool, t => t.Clip.empty),
        CreateReferencesColumn(),
        CreateDependenciesColumn(),
        CreateWrittenColumn()
    });
}