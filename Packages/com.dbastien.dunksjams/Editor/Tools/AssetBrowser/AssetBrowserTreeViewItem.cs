using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
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

    const string TimeFormat = "yyyy/MM/dd HH:mm:ss";

    protected static readonly Dictionary<AssetImporterPlatform, string> AssetImporterPlatformStrings = new();

    static AssetBrowserTreeViewItem()
    {
        foreach (var p in Enum.GetValues(typeof(AssetImporterPlatform)))
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

    public void SaveImportSettings()
    {
        AssetDatabase.WriteImportSettingsIfDirty(AssetPath);
        //AssetDatabase.SaveAssets();
    }
}