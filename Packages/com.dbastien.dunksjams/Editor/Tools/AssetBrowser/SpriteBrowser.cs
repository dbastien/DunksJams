using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

public class SpriteBrowserWindow : AssetBrowserWindow<SpriteBrowserTreeView, SpriteBrowserTreeView.TreeViewItem>
{
    protected override string WinTitle => "Sprites";

    [MenuItem("‽/Asset Browser/Sprites", false, 100)]
    public static void ShowWindow() => AssetBrowserWindowManager.ShowWindow<SpriteBrowserWindow>();
}

public class SpriteBrowserTreeView : AssetBrowserTreeView<SpriteBrowserTreeView.TreeViewItem>
{
    public class TreeViewItem : AssetBrowserTreeViewItem<Texture2D, TextureImporter>
    {
        public Texture2D Texture => TypedAsset;
        public TextureImporter Importer => TypedImporter;

        public TreeViewItem(int id, string guid, string path, Texture2D asset, TextureImporter importer)
            : base(id, guid, path, asset, importer) { }
    }

    public SpriteBrowserTreeView(TreeViewState<int> state) : base(state) { }

    protected override string[] GatherGuids(bool sceneOnly, string path)
    {
        if (!sceneOnly) return FindAssetGuids("Texture2D", path);

        var unique = new System.Collections.Generic.HashSet<Object>();
        foreach (SpriteRenderer sr in Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include,
                     FindObjectsSortMode.None))
            if (sr.sprite && sr.sprite.texture)
                unique.Add(sr.sprite.texture);
        return AssetGuidsFromObjects(unique);
    }

    protected override void AddAsset(ref int id, string guid, string path)
    {
        var asset = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (asset && importer && importer.textureType == TextureImporterType.Sprite)
            AllItems.Add(new TreeViewItem(id++, guid, path, asset, importer));
    }

    protected override MultiColumnHeaderState CreateHeaderState() => new(new MultiColumnHeaderState.Column[]
    {
        CreateColumn("Object", ColumnType.Object, t => t.Texture, type: typeof(Texture2D)),
        CreatePathColumn(),
        CreateRuntimeMemoryColumn(),
        CreateStorageSizeColumn(),
        new Column("Res", 70, 80,
            (l, r) => (l.Texture.width * l.Texture.height).CompareTo(r.Texture.width * r.Texture.height),
            (rect, t) => EditorGUI.LabelField(rect, $"{t.Texture.width}x{t.Texture.height}", EditorStyles.label),
            t => $"{t.Texture.width}x{t.Texture.height}"),
        CreateColumn("SpriteMode", ColumnType.Medium, t => t.Importer.spriteImportMode),
        CreateColumn("PPU", ColumnType.Small, t => t.Importer.spritePixelsPerUnit),
        CreateColumn("PackingTag", ColumnType.Medium, t => t.Importer.spritePackingTag),
        CreateColumn("IsReadable", ColumnType.Bool, t => t.Importer.isReadable),
        CreateReferencesColumn(),
        CreateDependenciesColumn(),
        CreateWrittenColumn()
    });
}