using UnityEngine;
using UnityEditor;

public static class LutRegistryBuilder
{
    const string RegistryPath = "Packages/com.dbastien.dunksjams/Resources/LutRegistry.asset";
    const string LutSearchFolder = "Packages/com.dbastien.dunksjams/Runtime/Textures/LDR LUTs";

    [MenuItem("‽/Refresh LUT Registry")]
    public static void DiscoverAndBuild()
    {
        var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { LutSearchFolder });
        var luts = new System.Collections.Generic.List<Texture2D>();
        var names = new System.Collections.Generic.List<string>();

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (!tex) continue;

            luts.Add(tex);
            names.Add(tex.name);
        }

        luts.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
        names.Clear();
        foreach (var t in luts) names.Add(t.name);

        var registry = RegistryPath.FindOrCreateAsset<LutRegistry>();
        registry.SetEntries(luts.ToArray(), names.ToArray());
        EditorUtility.SetDirty(registry);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        DLog.Log($"LUT Registry: discovered {luts.Count} textures in {LutSearchFolder}");
    }
}
