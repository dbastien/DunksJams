using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class FontBrowserWindow : AssetBrowserWindow<FontBrowserTreeView, FontBrowserTreeView.TreeViewItem>
{
    protected override string WinTitle => "Fonts";

    [MenuItem("‽/Asset Browser/Fonts", false, 100)]
    public static void ShowWindow() => AssetBrowserWindowManager.ShowWindow<FontBrowserWindow>();
}

public class FontBrowserTreeView : AssetBrowserTreeView<FontBrowserTreeView.TreeViewItem>
{
    public class TreeViewItem : AssetBrowserTreeViewItem<Font, TrueTypeFontImporter>
    {
        public Font Font => TypedAsset;
        public TrueTypeFontImporter Importer => TypedImporter;

        public TreeViewItem(int id, string guid, string path, Font asset, TrueTypeFontImporter importer)
            : base(id, guid, path, asset, importer) { }
    }

    public FontBrowserTreeView(TreeViewState<int> state) : base(state) { }

    protected override string[] GatherGuids(bool sceneOnly, string path) =>
        sceneOnly
            ? GatherSceneGuids<TextMesh>(tm => tm.font)
            : FindAssetGuids("Font", path);

    protected override void AddAsset(ref int id, string guid, string path)
    {
        var asset = AssetDatabase.LoadAssetAtPath<Font>(path);
        var importer = AssetImporter.GetAtPath(path) as TrueTypeFontImporter;
        if (asset && importer) AllItems.Add(new TreeViewItem(id++, guid, path, asset, importer));
    }

    protected override MultiColumnHeaderState CreateHeaderState() => new(new MultiColumnHeaderState.Column[]
    {
        CreateColumn("Object", ColumnType.Object, t => t.Font, type: typeof(Font)),
        CreatePathColumn(),
        CreateRuntimeMemoryColumn(),
        CreateStorageSizeColumn(),
        CreateColumn("FontSize", ColumnType.Int, t => t.Importer.fontSize),
        CreateColumn("TextureCase", ColumnType.Medium, t => t.Importer.fontTextureCase),
        CreateColumn("IncludeData", ColumnType.Bool, t => t.Importer.includeFontData),
        CreateColumn("Dynamic", ColumnType.Bool, t => t.Font.dynamic),
        CreateReferencesColumn(),
        CreateDependenciesColumn(),
        CreateWrittenColumn()
    });
}