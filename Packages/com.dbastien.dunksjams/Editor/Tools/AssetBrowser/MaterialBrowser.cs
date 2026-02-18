using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class MaterialBrowserWindow : AssetBrowserWindow<MaterialBrowserTreeView, MaterialBrowserTreeView.TreeViewItem>
{
    protected override string WinTitle => "Materials";

    [MenuItem("‽/Asset Browser/Materials", false, 100)]
    public static void ShowWindow() => AssetBrowserWindowManager.ShowWindow<MaterialBrowserWindow>();

    protected override void AddCustomSaveFunctions(GenericMenu menu)
    {
        menu.AddItem(new GUIContent("Properties"), false, () => SavePanel(SaveProperties));
        menu.AddItem(new GUIContent("Keywords"), false, () => SavePanel(SaveKeywords));
    }

    protected virtual void SaveProperties()
    {
        List<MaterialBrowserTreeView.TreeViewItem> rows = treeView.AllItems;
        using var file = new StreamWriter(outPath);

        foreach (MaterialBrowserTreeView.TreeViewItem r in rows)
        {
            if (!treeView.DoesItemMatchSearch(r)) continue;
            string line = r.AssetPath;
            var so = new SerializedObject(r.Asset);

            if (so.FindProperty("m_SavedProperties.m_TexEnvs") is { } texProps)
                for (var p = 0; p < texProps.arraySize; ++p)
                {
                    SerializedProperty sp = texProps.GetArrayElementAtIndex(p);
                    SerializedProperty propName = sp?.FindPropertyRelative("first");
                    if (propName == null) continue;

                    SerializedProperty propChildren = sp.FindPropertyRelative("second");
                    if (propChildren == null) continue;

                    SerializedProperty propTex = propChildren.FindPropertyRelative("m_Texture");
                    SerializedProperty propScale = propChildren.FindPropertyRelative("m_Scale");
                    SerializedProperty propOffset = propChildren.FindPropertyRelative("m_Offset");
                    var tex = propTex.objectReferenceValue as Texture;
                    string texPath = AssetDatabase.GetAssetPath(tex);

                    line +=
                        $",{propName.stringValue}: {texPath}" +
                        $" | scale: {propScale?.vector2Value.ToString().CsvSafe()}" +
                        $" | offset: {propOffset?.vector2Value.ToString().CsvSafe()}";
                }

            if (so.FindProperty("m_SavedProperties.m_Ints") is { } intProps)
                for (var p = 0; p < intProps.arraySize; ++p)
                {
                    SerializedProperty sp = intProps.GetArrayElementAtIndex(p);
                    SerializedProperty f = sp?.FindPropertyRelative("first");
                    SerializedProperty s = sp?.FindPropertyRelative("second");
                    if (f != null && s != null && r.Mat.HasInt(f.stringValue))
                        line += $",{f.stringValue}: {s.intValue}";
                }
            else if (so.FindProperty("m_SavedProperties.m_Floats") is { } floatProps)
                for (var p = 0; p < floatProps.arraySize; ++p)
                {
                    SerializedProperty sp = floatProps.GetArrayElementAtIndex(p);
                    SerializedProperty f = sp?.FindPropertyRelative("first");
                    SerializedProperty s = sp?.FindPropertyRelative("second");
                    if (f != null && s != null && r.Mat.HasInt(f.stringValue))
                        line += $",{f.stringValue}: {s.floatValue}";
                }

            //float, colors, ???

            file.WriteLine(line);
        }
    }

    protected virtual void SaveKeywords()
    {
        List<MaterialBrowserTreeView.TreeViewItem> rows = treeView.AllItems;
        using StreamWriter file = new(outPath);
        foreach (MaterialBrowserTreeView.TreeViewItem r in rows.Where(r => treeView.DoesItemMatchSearch(r)))
            file.WriteLine($"{r.AssetPath},{string.Join(',', r.Mat.enabledKeywords)}");
    }
}

public class MaterialBrowserTreeView : AssetBrowserTreeView<MaterialBrowserTreeView.TreeViewItem>
{
    public class TreeViewItem : AssetBrowserTreeViewItem<Material>
    {
        public Material Mat => TypedAsset;

        [SerializeReference] public readonly List<ShaderMessage> Errors = new();
        [SerializeReference] public readonly List<ShaderMessage> Warnings = new();

        public TreeViewItem(int id, string guid, string path, Material asset) : base(id, guid, path, asset) { }

        protected sealed override void Rebuild()
        {
            Errors.Clear();
            Warnings.Clear();

#pragma warning disable CS0618 // No modern equivalent for shader message APIs
            int n = ShaderUtil.GetShaderMessageCount(TypedAsset.shader);
            ShaderMessage[] msgs = n > 0 ? ShaderUtil.GetShaderMessages(TypedAsset.shader) : null;
#pragma warning restore CS0618
            for (var i = 0; i < n; ++i)
                (msgs[i].severity == ShaderCompilerMessageSeverity.Error ? Errors : Warnings).Add(msgs[i]);

            base.Rebuild();
        }
    }

    public MaterialBrowserTreeView(TreeViewState<int> state) : base(state) { }

    protected override string[] GatherGuids(bool sceneOnly, string path)
    {
        if (!sceneOnly) return FindAssetGuids("Material", path);

        HashSet<Material> materials = new();
        Renderer[] renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        foreach (Renderer renderer in renderers)
        foreach (Material material in renderer.sharedMaterials)
            if (material)
                materials.Add(material);
        return AssetGuidsFromObjects(materials);
    }

    protected override void AddAsset(ref int id, string guid, string path) =>
        AllItems.Add(new TreeViewItem(id++, guid, path, AssetDatabase.LoadAssetAtPath<Material>(path)));

    protected override MultiColumnHeaderState CreateHeaderState() => new(new MultiColumnHeaderState.Column[]
    {
        CreateColumn("Object", ColumnType.Object, t => t.Mat, type: typeof(Material)),
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
    });

    public override void FindReferences()
    {
        //todo: test if this convoluted way is faster than the legit way
        EditorUtility.DisplayProgressBar(TitleReferences, "", 0f);

        string[] pathsUnity = Directory.GetFiles(Application.dataPath, "*.unity", SearchOption.AllDirectories);
        string[] pathsPrefab = Directory.GetFiles(Application.dataPath, "*.prefab", SearchOption.AllDirectories);
        var targetPaths = new List<string>(pathsUnity.Length + pathsPrefab.Length);
        targetPaths.AddRange(pathsUnity);
        targetPaths.AddRange(pathsPrefab);

        var guidStrings = new List<string>();
        for (var i = 0; i < targetPaths.Count; ++i)
        {
            string path = targetPaths[i];
            string pathShort = path.Remove(Application.dataPath);

            if (EditorUtility.DisplayCancelableProgressBar(TitleReferences, pathShort, i / (float)targetPaths.Count))
                break;

            guidStrings.Clear();

            try
            {
                using StreamReader fs = File.OpenText(path);
                while (fs.ReadLine() is { } line)
                {
                    if (line.Contains("objectReference"))
                        guidStrings.Add(line);
                    if (line.StartsWithFast("  m_Material"))
                        guidStrings.Add(fs.ReadLine());
                }
            }
            catch (Exception ex) { DLog.Log($"Error reading file: {path} - {ex.Message}"); }

            foreach (TreeViewItem row in AllItems)
            foreach (string gs in guidStrings)
            {
                if (!gs.Contains(row.Guid)) continue;
                row.Refs.Add(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets" + pathShort));
                break;
            }
        }

        EditorUtility.ClearProgressBar();
    }
}