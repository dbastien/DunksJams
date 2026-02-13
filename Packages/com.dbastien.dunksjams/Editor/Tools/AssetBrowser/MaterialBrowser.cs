using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

public class MaterialBrowserWindow : AssetBrowserWindow<MaterialBrowserTreeView, MaterialBrowserTreeView.TreeViewItem>
{
    protected override string WinTitle => "Materials";

    [MenuItem("‽/Asset Browser/Materials", false, 100)]
    public static void ShowWindow() => AssetBrowserWindowManager.ShowWindow<MaterialBrowserWindow>();

    protected override void Rebuild()
    {
        treeViewState ??= new TreeViewState<int>();
        var items = treeView?.AllItems ?? new List<MaterialBrowserTreeView.TreeViewItem>(32);
        treeView = new MaterialBrowserTreeView(treeViewState) { AllItems = items };
        treeView.Reload();
    }

    protected override void AddCustomSaveFunctions(GenericMenu menu)
    {
        menu.AddItem(new GUIContent("Properties"), false, () => SavePanel(SaveProperties));
        menu.AddItem(new GUIContent("Keywords"), false, () => SavePanel(SaveKeywords));
    }

    protected virtual void SaveProperties()
    {
        var rows = treeView.AllItems;
        using var file = new StreamWriter(outPath);

        foreach (var r in rows)
        {
            if (!treeView.DoesItemMatchSearch(r)) continue;
            var line = r.AssetPath;
            var so = new SerializedObject(r.Asset);

            if (so.FindProperty("m_SavedProperties.m_TexEnvs") is { } texProps)
                for (var p = 0; p < texProps.arraySize; ++p)
                {
                    var sp = texProps.GetArrayElementAtIndex(p);
                    var propName = sp?.FindPropertyRelative("first");
                    if (propName == null) continue;

                    var propChildren = sp.FindPropertyRelative("second");
                    if (propChildren == null) continue;

                    var propTex = propChildren.FindPropertyRelative("m_Texture");
                    var propScale = propChildren.FindPropertyRelative("m_Scale");
                    var propOffset = propChildren.FindPropertyRelative("m_Offset");
                    var tex = propTex.objectReferenceValue as Texture;
                    var texPath = AssetDatabase.GetAssetPath(tex);

                    line +=
                        $",{propName.stringValue}: {texPath}" +
                        $" | scale: {propScale?.vector2Value.ToString().CsvSafe()}" +
                        $" | offset: {propOffset?.vector2Value.ToString().CsvSafe()}";
                }

            if (so.FindProperty("m_SavedProperties.m_Ints") is { } intProps)
                for (var p = 0; p < intProps.arraySize; ++p)
                {
                    var sp = intProps.GetArrayElementAtIndex(p);
                    var f = sp?.FindPropertyRelative("first");
                    var s = sp?.FindPropertyRelative("second");
                    if (f != null && s != null && r.Mat.HasInt(f.stringValue))
                        line += $",{f.stringValue}: {s.intValue}";
                }
            else if (so.FindProperty("m_SavedProperties.m_Floats") is { } floatProps)
                for (var p = 0; p < floatProps.arraySize; ++p)
                {
                    var sp = floatProps.GetArrayElementAtIndex(p);
                    var f = sp?.FindPropertyRelative("first");
                    var s = sp?.FindPropertyRelative("second");
                    if (f != null && s != null && r.Mat.HasInt(f.stringValue))
                        line += $",{f.stringValue}: {s.floatValue}";
                }

            //float, colors, ???

            file.WriteLine(line);
        }
    }

    protected virtual void SaveKeywords()
    {
        var rows = treeView.AllItems;
        using StreamWriter file = new(outPath);
        foreach (var r in rows.Where(r => treeView.DoesItemMatchSearch(r)))
            file.WriteLine($"{r.AssetPath},{string.Join(',', r.Mat.enabledKeywords)}");
    }
}

public class MaterialBrowserTreeView : AssetBrowserTreeView<MaterialBrowserTreeView.TreeViewItem>
{
    public class TreeViewItem : AssetBrowserTreeViewItem
    {
        public Material Mat;
        public override UnityEngine.Object Asset => Mat;
        public override string AssetName => Mat.name;
        //public override AssetImporter AssetImporter => Importer;

        [SerializeReference] public readonly List<ShaderMessage> Errors = new();
        [SerializeReference] public readonly List<ShaderMessage> Warnings = new();

        public TreeViewItem(int id, string guid, string path, Material asset) : base(id, guid, path)
        {
            Mat = asset;
            Rebuild();
        }

        protected sealed override void Rebuild()
        {
            RuntimeMemory = Profiler.GetRuntimeMemorySizeLong(Asset);

            Errors.Clear();
            Warnings.Clear();

        #pragma warning disable CS0618 // No modern equivalent for shader message APIs
            var n = ShaderUtil.GetShaderMessageCount(Mat.shader);
            var msgs = n > 0 ? ShaderUtil.GetShaderMessages(Mat.shader) : null;
        #pragma warning restore CS0618
            for (var i = 0; i < n; ++i)
                (msgs[i].severity == ShaderCompilerMessageSeverity.Error ? Errors : Warnings).Add(msgs[i]);

            base.Rebuild();
        }
    }

    public MaterialBrowserTreeView(TreeViewState<int> state) : base(state)
    {
        multiColumnHeader = new MultiColumnHeader(CreateHeaderState());
        InitHeader(multiColumnHeader);
    }

    protected override string[] GatherGuids(bool sceneOnly, string path)
    {
        if (!sceneOnly) return AssetDatabase.FindAssets("t:Material", new[] { path });

        HashSet<Material> materials = new();
        var renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.sharedMaterials)
            {
                if (material)
                    materials.Add(material);
            }
        }

        List<string> guids = new();
        foreach (var material in materials)
        {
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(material, out var guid, out _))
                guids.Add(guid);
        }

        return guids.ToArray();
    }

    protected override void AddAsset(ref int id, string guid, string path) =>
        AllItems.Add(new TreeViewItem(id++, guid, path, AssetDatabase.LoadAssetAtPath<Material>(path)));

    public override void FindReferences()
    {
        //todo: test if this convoluted way is faster than the legit way
        EditorUtility.DisplayProgressBar(TitleReferences, "", 0f);

        var pathsUnity = Directory.GetFiles(Application.dataPath, "*.unity", SearchOption.AllDirectories);
        var pathsPrefab = Directory.GetFiles(Application.dataPath, "*.prefab", SearchOption.AllDirectories);
        var targetPaths = new List<string>(pathsUnity.Length + pathsPrefab.Length);
        targetPaths.AddRange(pathsUnity);
        targetPaths.AddRange(pathsPrefab);

        var guidStrings = new List<string>();
        for (var i = 0; i < targetPaths.Count; ++i)
        {
            var path = targetPaths[i];
            var pathShort = path.Remove(Application.dataPath);

            if (EditorUtility.DisplayCancelableProgressBar(TitleReferences, pathShort, i / (float)targetPaths.Count))
                break;

            guidStrings.Clear();

            try
            {
                using var fs = File.OpenText(path);
                while (fs.ReadLine() is { } line)
                {
                    if (line.Contains("objectReference"))
                        guidStrings.Add(line);
                    if (line.StartsWithFast("  m_Material"))
                        guidStrings.Add(fs.ReadLine());
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
                    row.Refs.Add(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets" + pathShort));
                    break;
                }
            }
        }

        EditorUtility.ClearProgressBar();
    }

    MultiColumnHeaderState CreateHeaderState()
    {
        MultiColumnHeaderState.Column[] cols =
        {
            CreateColumn("Object", ColumnType.Object, t => t.Asset as Material, type: typeof(Material)),
            CreatePathColumn(),
            CreateRuntimeMemoryColumn(),
            CreateColumn("Shader", ColumnType.Object, t => t.Mat.shader, shader => shader.name, typeof(Shader)),
            CreateColumn("Queue", ColumnType.Int, t => t.Mat.renderQueue),
            CreateCollectionColumn<List<ShaderMessage>, ShaderMessage>("Warnings", ColumnType.Int, DropdownWarn,
                t => t.Warnings, m => $"{m.file} {m.line}: {m.message}\n"),
            CreateCollectionColumn<List<ShaderMessage>, ShaderMessage>("Errors", ColumnType.Int, DropdownError,
                t => t.Errors, m => $"{m.file} {m.line}: {m.message}\n"),
            CreateCollectionColumn<LocalKeyword[], LocalKeyword>("Keywords", ColumnType.Int, null,
                t => t.Mat.enabledKeywords, keyword => $"{keyword} "),
            CreateReferencesColumn(),
            CreateDependenciesColumn(),
            CreateWrittenColumn()
        };

        return new MultiColumnHeaderState(cols);
    }
}