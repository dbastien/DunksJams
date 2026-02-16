#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

class PaletteAssetPostprocessor : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        bool needsRefresh = false;

        foreach (var asset in importedAssets)
            if (asset.EndsWith(".asset") && asset.Contains("Palette"))
                needsRefresh = true;

        foreach (var asset in deletedAssets)
            if (asset.Contains("Palette"))
                needsRefresh = true;

        foreach (var asset in movedAssets)
            if (asset.Contains("Palette"))
                needsRefresh = true;

        if (needsRefresh)
            PaletteDatabase.Refresh();
    }
}
#endif