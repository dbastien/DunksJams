using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;

public class AssetBrowserTreeViewItem : TreeViewItem<int>
{
    public string Guid;
    public string AssetPath;
    public DateTime WriteTime = DateTime.MinValue;
    public string WriteTimeAsString = "";
    public List<Object> Refs;
    public List<Object> Deps;

    public virtual Object Asset => null;
    public virtual string AssetName => "";
    public virtual AssetImporter AssetImporter => null;

    public long RuntimeMemory;
    public long StorageSize;

    private const string TimeFormat = "yyyy/MM/dd HH:mm:ss";

    protected static readonly Dictionary<AssetImporterPlatform, string> AssetImporterPlatformStrings = new();

    static AssetBrowserTreeViewItem()
    {
        foreach (object p in Enum.GetValues(typeof(AssetImporterPlatform)))
            AssetImporterPlatformStrings.Add((AssetImporterPlatform)p, p.ToString());
    }

    public AssetBrowserTreeViewItem(int id, string guid, string path) : base(id)
    {
        Refs = new List<Object>();
        Deps = new List<Object>();

        Guid = guid;
        AssetPath = path;
    }

    protected virtual void Rebuild()
    {
        WriteTime = File.GetLastWriteTime($"{IOUtils.ProjectRootFolder}/{AssetPath}");
        WriteTimeAsString = WriteTime.ToString(TimeFormat);
    }

    public void SaveImportSettings() { AssetDatabase.WriteImportSettingsIfDirty(AssetPath); }
}

/// <summary>Generic tree view item for assets without a specific importer.</summary>
public class AssetBrowserTreeViewItem<TAsset> : AssetBrowserTreeViewItem
    where TAsset : Object
{
    public TAsset TypedAsset;

    public override Object Asset => TypedAsset;
    public override string AssetName => TypedAsset ? TypedAsset.name : "";

    public AssetBrowserTreeViewItem(int id, string guid, string path, TAsset asset) : base(id, guid, path)
    {
        TypedAsset = asset;
        Rebuild();
    }

    protected override void Rebuild()
    {
        RuntimeMemory = Profiler.GetRuntimeMemorySizeLong(TypedAsset);
        base.Rebuild();
    }
}

/// <summary>Generic tree view item for assets with a typed importer.</summary>
public class AssetBrowserTreeViewItem<TAsset, TImporter> : AssetBrowserTreeViewItem
    where TAsset : Object
    where TImporter : AssetImporter
{
    public TAsset TypedAsset;
    public TImporter TypedImporter;

    public override Object Asset => TypedAsset;
    public override string AssetName => TypedAsset ? TypedAsset.name : "";
    public override AssetImporter AssetImporter => TypedImporter;

    public AssetBrowserTreeViewItem(int id, string guid, string path, TAsset asset, TImporter importer)
        : base(id, guid, path)
    {
        TypedAsset = asset;
        TypedImporter = importer;
        Rebuild();
    }

    protected override void Rebuild()
    {
        RuntimeMemory = Profiler.GetRuntimeMemorySizeLong(TypedAsset);
        base.Rebuild();
    }
}