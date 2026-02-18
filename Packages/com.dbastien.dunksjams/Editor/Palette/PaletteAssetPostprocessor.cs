#if UNITY_EDITOR
using UnityEditor;

internal class PaletteAssetPostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets
        (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        var needsRefresh = false;

        foreach (string asset in importedAssets)
            if (asset.EndsWith(".asset") && asset.Contains("Palette"))
                needsRefresh = true;

        foreach (string asset in deletedAssets)
            if (asset.Contains("Palette"))
                needsRefresh = true;

        foreach (string asset in movedAssets)
            if (asset.Contains("Palette"))
                needsRefresh = true;

        if (needsRefresh)
            PaletteDatabase.Refresh();
    }
}
#endif