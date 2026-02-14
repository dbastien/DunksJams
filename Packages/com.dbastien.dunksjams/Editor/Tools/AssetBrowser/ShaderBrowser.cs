using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class ShaderBrowserWindow : AssetBrowserWindow<ShaderBrowserTreeView, ShaderBrowserTreeView.TreeViewItem>
{
    protected override string WinTitle => "Shaders";

    [MenuItem("‽/Asset Browser/Shaders", false, 100)]
    public static void ShowWindow() => AssetBrowserWindowManager.ShowWindow<ShaderBrowserWindow>();
}

public class ShaderBrowserTreeView : AssetBrowserTreeView<ShaderBrowserTreeView.TreeViewItem>
{
    public class TreeViewItem : AssetBrowserTreeViewItem<Shader, ShaderImporter>
    {
        public Shader Shader => TypedAsset;
        public ShaderImporter Importer => TypedImporter;
        public ShaderData Data;

        public bool HasShadowCasterPass;
        public bool HasMotionVectorsPass;
        public bool IsSRPBatcherCompatible;

        public ulong VariantCount;
        public ulong VariantCountSceneOnly;

        public string[] KeywordsLocal;

        [SerializeReference] public List<string> PropNames = new();
        [SerializeReference] public readonly List<ShaderMessage> Errors = new();
        [SerializeReference] public readonly List<ShaderMessage> Warnings = new();

        public TreeViewItem(int id, string guid, string assetPath, Shader asset, ShaderImporter importer)
            : base(id, guid, assetPath, asset, importer) { }

        protected sealed override void Rebuild()
        {
            PropNames.Clear();
            Errors.Clear();
            Warnings.Clear();

        #pragma warning disable CS0618 // No modern equivalent for GetShaderData
            Data = ShaderUtil.GetShaderData(TypedAsset);
        #pragma warning restore CS0618

            HasShadowCasterPass = ShaderUtilWrapper.HasShadowCasterPass(TypedAsset);
            IsSRPBatcherCompatible = ShaderUtilWrapper.IsSRPBatcherCompatible(TypedAsset);
            VariantCount = ShaderUtilWrapper.GetVariantCount(TypedAsset, false);
            VariantCountSceneOnly = ShaderUtilWrapper.GetVariantCount(TypedAsset, true);
            KeywordsLocal = ShaderUtilWrapper.GetShaderLocalKeywords(TypedAsset);

            SerializedObject so = new(TypedAsset);
            var subShaders = so.FindProperty("m_ParsedForm.m_SubShaders");
            for (var s = 0; s < subShaders.arraySize; ++s)
            {
                var subShader = subShaders.GetArrayElementAtIndex(s);
                var passes = subShader.FindPropertyRelative("m_Passes");
                for (var p = 0; p < passes.arraySize; ++p)
                {
                    var pass = passes.GetArrayElementAtIndex(p);
                    var tags = pass.FindPropertyRelative("m_State.m_Tags.tags");
                    for (var t = 0; t < tags.arraySize; ++t)
                    {
                        var tag = tags.GetArrayElementAtIndex(t);
                        var first = tag.FindPropertyRelative("first");
                        var second = tag.FindPropertyRelative("second");

                        if (string.CompareOrdinal(first.stringValue, "LIGHTMODE") != 0) continue;
                        var lightmodeType = second.stringValue;
                        if (Enum.TryParse(lightmodeType, true, out PassType passType))
                            if (passType == PassType.MotionVectors)
                                HasMotionVectorsPass = true;
                    }
                }
            }

            var propertyCount = TypedAsset.GetPropertyCount();
            for (var i = 0; i < propertyCount; ++i)
            {
                var name = TypedAsset.GetPropertyName(i);
                if (!string.IsNullOrEmpty(name)) PropNames.Add(name);
            }

        #pragma warning disable CS0618 // No modern equivalent for shader message APIs
            var n = ShaderUtil.GetShaderMessageCount(TypedAsset);
            var msgs = n > 0 ? ShaderUtil.GetShaderMessages(TypedAsset) : null;
        #pragma warning restore CS0618
            for (var i = 0; i < n; ++i)
                (msgs[i].severity == ShaderCompilerMessageSeverity.Error ? Errors : Warnings).Add(msgs[i]);

            base.Rebuild();
        }
    }

    public ShaderBrowserTreeView(TreeViewState<int> state) : base(state) { }

    protected override string[] GatherGuids(bool sceneOnly, string path)
    {
        if (!sceneOnly) return FindAssetGuids("Shader", path);

        HashSet<Shader> shaders = new();
        var renderers =
            UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var renderer in renderers)
            foreach (var material in renderer.sharedMaterials)
                if (material) shaders.Add(material.shader);
        return AssetGuidsFromObjects(shaders);
    }

    protected override void AddAsset(ref int id, string guid, string path)
    {
        var asset = AssetDatabase.LoadAssetAtPath<Shader>(path);
        var importer = AssetImporter.GetAtPath(path) as ShaderImporter;
        if (asset && importer) AllItems.Add(new TreeViewItem(id++, guid, path, asset, importer));
    }

    protected override MultiColumnHeaderState CreateHeaderState() => new(new MultiColumnHeaderState.Column[]
    {
        CreateColumn("Object", ColumnType.Object, t => t.Shader, type: typeof(Shader)),
        CreatePathColumn(),
        CreateRuntimeMemoryColumn(),
        CreateColumn("Queue", ColumnType.Int, t => t.Shader.renderQueue),
        CreateCollectionColumn<List<ShaderMessage>, ShaderMessage>("Warnings", ColumnType.Int, DropdownWarn,
            t => t.Warnings, m => $"{m.file} {m.line}: {m.message}\n"),
        CreateCollectionColumn<List<ShaderMessage>, ShaderMessage>("Errors", ColumnType.Int, DropdownError,
            t => t.Errors, m => $"{m.file} {m.line}: {m.message}\n"),
        CreateCollectionColumn<string[], string>("Keywords", ColumnType.Int, null, t => t.KeywordsLocal,
            keyword => $"{keyword} "),
        CreateColumn("Variant #s", ColumnType.Int, t => t.VariantCount),
        CreateColumn("Variant #s SceneOnly", ColumnType.Int, t => t.VariantCountSceneOnly),
        CreateColumn("Subshaders", ColumnType.Int, t => t.Shader.subshaderCount),
        CreateColumn("SRP Batchable", ColumnType.Bool, t => t.IsSRPBatcherCompatible),
        CreateColumn("Motion Vectors", ColumnType.Bool, t => t.HasMotionVectorsPass),
        CreateColumn("Shadow Pass", ColumnType.Bool, t => t.HasShadowCasterPass),
        CreateReferencesColumn(),
        CreateDependenciesColumn(),
        CreateWrittenColumn()
    });

    public override void FindReferences()
    {
        const string lineStart = "  m_Shader";
        EditorUtility.DisplayProgressBar(TitleReferences, "", 0f);

        var targetPaths = Directory.GetFiles(Application.dataPath, "*.mat", SearchOption.AllDirectories);
        var guidStrings = new List<string>();
        for (var i = 0; i < targetPaths.Length; ++i)
        {
            var path = targetPaths[i];
            var pathShort = path.Remove(Application.dataPath);

            if (EditorUtility.DisplayCancelableProgressBar(TitleReferences, pathShort, i / (float)targetPaths.Length))
                break;

            guidStrings.Clear();

            try
            {
                using var fs = File.OpenText(path);
                while (fs.Peek() != -1)
                {
                    var line = fs.ReadLine();
                    if (line.StartsWithFast(lineStart))
                        guidStrings.Add(line?[lineStart.Length..]);
                }
            }
            catch (Exception ex)
            {
                DLog.Log($"Error reading file: {path} - {ex.Message}");
            }

            foreach (var row in AllItems)
            {
                foreach (var gs in guidStrings)
                {
                    if (!gs.Contains(row.Guid)) continue;
                    row.Refs.Add(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>($"Assets{pathShort}"));
                    break;
                }
            }
        }

        EditorUtility.ClearProgressBar();
    }
}
