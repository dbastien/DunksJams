using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Object = UnityEngine.Object;

/// <summary> Utilitiies / helpers for Unity's AssetDatabase </summary>
public static class AssetDatabaseUtils
{
    public static List<T> FindAssetsByType<T>() where T : Object
    {
        var guids = AssetDatabase.FindAssets($"t:{typeof(T)}", null);
        var assets = new List<T>(guids.Length);

        foreach (var guid in guids)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);

            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);

            //TODO: verify cases where this can be null - it'd be nice to allocate an array instead of list
            if (asset != null)
            {
                assets.Add(asset);
            }
        }

        return assets;
    }

    public static List<T> FindAndLoadAssets<T>() where T : Object
    {
        var guids = FindAssetGUIDs<T>();

        return LoadAssetsByGUIDs<T>(guids);
    }

    public static string[] FindAssetGUIDs<T>() where T : Object
    {
        var typeName = typeof(T).ToString();

        var lastIndex = typeName.LastIndexOf('.');

        if (lastIndex > 0 && lastIndex < typeName.Length - 2)
        {
            typeName = typeName.Substring(lastIndex + 1);
        }

        var search = string.Format("t:{0}", typeName);

        return AssetDatabase.FindAssets(search);
    }

    public static T LoadAssetByGUID<T>(string guid) where T : Object
    {
        var assetPath = AssetDatabase.GUIDToAssetPath(guid);
        return AssetDatabase.LoadAssetAtPath<T>(assetPath);     
    }
    
    public static List<T> LoadAssetsByGUIDs<T>(string[] guids) where T : Object
    {
        var assets = new List<T>(guids.Length);
        assets.AddRange(guids.Select(LoadAssetByGUID<T>).Where(asset => asset != null));
        return assets;
    }
}
