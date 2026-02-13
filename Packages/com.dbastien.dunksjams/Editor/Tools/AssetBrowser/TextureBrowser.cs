using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

public class TextureBrowserWindow : AssetBrowserWindow<TextureBrowserTreeView, TextureBrowserTreeView.TreeViewItem>
{
    protected override string WinTitle => "Textures";
    
    [MenuItem("‽/Asset Browser/Textures", false, 100)]
    public static void ShowWindow() => AssetBrowserWindowManager.ShowWindow<TextureBrowserWindow>();
    
    protected override void Rebuild()
    {
        treeViewState ??= new();
        List<TextureBrowserTreeView.TreeViewItem> items = treeView?.AllItems ?? new(32);
        treeView = new(treeViewState) { AllItems = items };
        treeView.Reload();
    }
}

public class TextureBrowserTreeView : AssetBrowserTreeView<TextureBrowserTreeView.TreeViewItem>
{
    public class TreeViewItem : AssetBrowserTreeViewItem
    {
        public override Object Asset => Texture;
        public Texture Texture;
        public TextureImporter Importer;
        public Dictionary<AssetImporterPlatform, TextureImporterPlatformSettings> PlatformSettings = new();
        public override string AssetName => Asset.name;
        public override AssetImporter AssetImporter => Importer;

        public TreeViewItem(int id, string guid, string path, Texture asset, TextureImporter importer) : base(id, guid, path)
        {
            Texture = asset;
            Importer = importer;
            Rebuild();
        }

        protected sealed override void Rebuild()
        {
            PlatformSettings.Clear();
            
            RuntimeMemory = TextureUtilWrapper.GetRuntimeMemorySizeLong(Texture);
            StorageSize = TextureUtilWrapper.GetStorageMemorySizeLong(Texture);

            foreach (KeyValuePair<AssetImporterPlatform, string> kvp in AssetImporterPlatformStrings)
                PlatformSettings.Add(kvp.Key, Importer.GetPlatformTextureSettings(kvp.Value));
            
            base.Rebuild();
        }
    }

    public TextureBrowserTreeView(TreeViewState<int> state) : base(state)
    {
        multiColumnHeader = new(CreateHeaderState(this));
        InitHeader(multiColumnHeader);
    }

    static void ExtractTexturesFromMaterial(Material mat, HashSet<Texture> uniqueTextures)
    {
        int propCount = mat.shader.GetPropertyCount();
        for (var i = 0; i < propCount; ++i)
        {
            if (mat.shader.GetPropertyType(i) != ShaderPropertyType.Texture) continue;
            Texture tex = mat.GetTexture(mat.shader.GetPropertyName(i));
            if (tex) uniqueTextures.Add(tex);
        }
    }

    //todo: consolidate with ShaderBrowser & MaterialBrowser?
    protected override string[] GatherGuids(bool sceneOnly, string path)
    {
        if (!sceneOnly) return AssetDatabase.FindAssets("t:Texture", new[] { path });
        
        HashSet<Texture> textures = new();
        List<string> guids = new();
        Renderer[] renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Renderer renderer in renderers)
            foreach (Material mat in renderer.sharedMaterials)
                if (mat) ExtractTexturesFromMaterial(mat, textures);

        foreach (Texture tex in textures)
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(tex, out string guid, out long _))
                guids.Add(guid);
            
        return guids.ToArray();
    }

    protected override void AddAsset(ref int id, string guid, string path)
    {
        var asset = AssetDatabase.LoadAssetAtPath<Texture>(path);
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        
        if (asset && importer) AllItems.Add(new(id++, guid, path, asset, importer));
    }
    
    MultiColumnHeaderState CreateHeaderState(TextureBrowserTreeView tv)
    {
        MultiColumnHeaderState.Column[] columns =
        {
            CreateColumn("Object", ColumnType.Object, t => t.Asset as Texture, type: typeof(Texture)),
            CreatePathColumn(),
            CreateRuntimeMemoryColumn(),
            CreateStorageSizeColumn(),
            CreateColumn("Type", ColumnType.Medium, t => t.Importer.textureType),
            CreateColumn("sRGB", ColumnType.Bool, t => t.Importer.sRGBTexture),
            new Column("Res", 70, 80,
                (l, r) => (l.Texture.width * l.Texture.height).CompareTo(r.Texture.width * r.Texture.height),
                (rect, t) => EditorGUI.LabelField(rect, $"{t.Texture.width}x{t.Texture.height}", EditorStyles.label),
                t => $"{t.Texture.width}x{t.Texture.height}"),
            CreateColumn("Max Res", ColumnType.Int, t => t.Importer.maxTextureSize),
            CreateColumn("Mip", ColumnType.Bool, t => t.Importer.mipmapEnabled),
            CreateColumn("Compression", ColumnType.Large, t => t.Importer.textureCompression),
            CreateColumn("Quality", ColumnType.Int, t => t.Importer.compressionQuality),
            CreateColumn("Crunched", ColumnType.Bool, t => t.Importer.crunchedCompression),
            CreateColumn("NPOT", ColumnType.Medium, t => t.Importer.npotScale),
            CreateColumn("Aniso", ColumnType.Int, t => t.Importer.anisoLevel),
            CreateColumn("Format", ColumnType.Large, t => t.PlatformSettings[tv.Platform].format),
            CreateColumn("IsReadable", ColumnType.Bool, t => t.Importer.isReadable),
            CreateReferencesColumn(),
            CreateDependenciesColumn(),
            CreateWrittenColumn()
        };

        return new(columns);
    }
}