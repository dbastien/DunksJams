using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Color = UnityEngine.Color;
using FontStyle = UnityEngine.FontStyle;
using Object = UnityEngine.Object;

[Serializable]
public class AssetBrowserTreeView<T> : TreeView<int> where T : AssetBrowserTreeViewItem
{
    [SerializeField] public AssetImporterPlatform Platform;

    public enum ColumnType
    {
        Object,
        Path,
        Bool,
        Int,
        FileSize,
        DateTime,
        Small,
        Medium,
        Large,
    }

    public static readonly Dictionary<ColumnType, Vector2Int> ColumnTypeSizes = new()
    {
        { ColumnType.Object, new(100, 150) },
        { ColumnType.Path, new(100, 150) },
        { ColumnType.Bool, new(40, 50) },
        { ColumnType.Int, new(20, 50) },
        { ColumnType.FileSize, new(50, 70) },
        { ColumnType.DateTime, new(90, 120) },
        { ColumnType.Small, new(20, 50) },
        { ColumnType.Medium, new(60, 80) },
        { ColumnType.Large, new(100, 150) }
    };

    static ColumnType InferColumnType(Type propertyType) => propertyType switch
    {
        _ when propertyType == typeof(bool) => ColumnType.Bool,
        _ when propertyType == typeof(int) => ColumnType.Int,
        _ when propertyType == typeof(float) => ColumnType.Int,
        _ when propertyType == typeof(string) => ColumnType.Path,
        _ when propertyType == typeof(DateTime) => ColumnType.DateTime,
        _ => ColumnType.Medium
    };

    public class Column : MultiColumnHeaderState.Column
    {
        public delegate void Draw(Rect rect, T t);
        public Draw draw;

        public delegate int Compare(T l, T r);
        public Compare compare;

        public delegate string Text(T t);
        public Text text;
        
        public string Header;

        void Setup(string header, int minW, int w, bool resize, Compare funcC, Draw funcD, Text funcT)
        {
            Header = header;
            headerContent = new(header);
            minWidth = minW;
            width = w;
            autoResize = resize;
            compare = funcC;
            draw = funcD;
            text = funcT;
        }

        void Setup(string header, ColumnType type, bool resize, Compare funcC, Draw funcD, Text funcT)
        {
            if (!ColumnTypeSizes.TryGetValue(type, out Vector2Int size)) size = new(60, 80);
            Setup(header, size.x, size.y, resize, funcC, funcD, funcT);
        }  
        
        public Column(string header, int minWidth, int width, Compare funcC, Draw funcD, Text funcT) => 
            Setup(header, minWidth, width, true, funcC, funcD, funcT);

        public Column(string header, ColumnType type, Compare funcC, Draw funcD, Text funcT) => 
            Setup(header, type, true, funcC, funcD, funcT);
    }

    [SerializeField] public List<T> AllItems;
    [SerializeReference] public TreeViewItem<int> Root;
    public int FilteredCount;

    protected static GUIStyle DropdownError;
    protected static GUIStyle DropdownWarn;
    protected static GUIStyle DropdownOK;

    protected static GUIStyle LabelError;
    protected static GUIStyle LabelWarn;
    protected static GUIStyle LabelOK;
    protected static GUIStyle LabelDiminish;

    protected const string TitleAssets = "Searching for Assets…";
    protected const string TitleReferences = "Searching for References…";
    protected const string TitleDependencies = "Searching for Dependencies…";

    public AssetBrowserTreeView(TreeViewState<int> state) : base(state) => Init();

    protected void InitHeader(MultiColumnHeader header)
    {
        header.sortingChanged += OnSortingChanged;
        header.ResizeToFit();
        header.SetSorting(2, false);
    }

    void Init()
    {
        GUIStyle ColorStyle(GUIStyle style, Color color) => new(style) { normal = { textColor = color } };
        
        Root = new() { id = -1, depth = -1, displayName = "Root" };
        AllItems = new(64);
        showAlternatingRowBackgrounds = true;
        
        DropdownError ??= ColorStyle(EditorStyles.miniPullDown, Color.red);
        DropdownWarn ??= ColorStyle(EditorStyles.miniPullDown, Color.yellow);
        DropdownOK ??= ColorStyle(EditorStyles.miniPullDown, Color.green);

        LabelError ??= ColorStyle(EditorStyles.label, Color.red);
        LabelWarn ??= ColorStyle(EditorStyles.label, Color.yellow);
        LabelOK ??= ColorStyle(EditorStyles.label, Color.green);

        if (LabelDiminish != null) return;
        LabelDiminish = ColorStyle(EditorStyles.label, Color.grey);
        LabelDiminish.fontStyle = FontStyle.Italic;
    }
    
    protected void OnSortingChanged(MultiColumnHeader colHeader) => Sort(GetRows());

    protected override IList<TreeViewItem<int>> BuildRows(TreeViewItem<int> root)
    {
        IList<TreeViewItem<int>> rows = base.BuildRows(root);
        Sort(rows);
        return rows;
    }

    protected override TreeViewItem<int> BuildRoot()
    {
        Root ??= new() { id = -1, depth = -1, displayName = "Root" };
        Root.children = AllItems.Cast<TreeViewItem<int>>().ToList();
        SetupDepthsFromParentsAndChildren(Root);
        return Root;
    }

    void Sort(IList<TreeViewItem<int>> rows)
    {
        if (rows == null) return;
        FilteredCount = rows.Count;
        if (FilteredCount <= 1) return;

        int sortIdx = multiColumnHeader.sortedColumnIndex;
        List<T> list = rows.Cast<T>().ToList();

        if (hasSearch) list.RemoveAll(item => !DoesItemMatchSearch(item, searchString));

        if (multiColumnHeader.sortedColumnIndex != -1)
        {
            var column = (Column)multiColumnHeader.GetColumn(sortIdx);
            int SortAscend(T l, T r) => column.compare(l, r);
            int SortDescend(T l, T r) => -column.compare(l, r);
            list.Sort(multiColumnHeader.IsSortedAscending(sortIdx) ? SortAscend : SortDescend);
        }

        rows.Clear();
        foreach (T t in list) rows.Add(t);
        Repaint();
    }

    protected override void RowGUI(RowGUIArgs args)
    {
        var t = args.item as T;
        for (var i = 0; i < args.GetNumVisibleColumns(); ++i)
            CellGUI(args.GetCellRect(i), args.GetColumn(i), t);
    }

    void CellGUI(Rect rect, int col, T t)
    {
        CenterRectUsingSingleLineHeight(ref rect);
        ((Column)multiColumnHeader.GetColumn(col)).draw?.Invoke(rect, t);
    }

    protected override void KeyEvent()
    {
        if (Event.current.keyCode == KeyCode.Delete &&
            Event.current.type == EventType.KeyUp)
            DeleteSelection();
    }
    
    protected Column CreateColumn<TValue>(string colName, ColumnType colType, 
        Func<T, TValue> valueGetter, Func<TValue, string> valueToString = null, Type type = null)
    {
        return new(colName, colType,
            (l, r) => Comparer<TValue>.Default.Compare(valueGetter(l), valueGetter(r)),
            (rect, item) =>
            {
                TValue value = valueGetter(item);
                if (type != null)
                    EditorGUI.ObjectField(rect, value as Object, type, false);
                else
                    EditorGUI.LabelField(rect, valueToString != null ? valueToString(value) : value.ToString());
            },
            item => valueToString != null ? valueToString(valueGetter(item)) : valueGetter(item)?.ToString());
    }

    public static Column CreateColumn<TValue>(string header, ColumnType colType, Expression<Func<T, TValue>> propertyExpression)
    {
        Func<T, TValue> compiled = propertyExpression.Compile();
    
        return new(
            header,
            colType,
            (l, r) => Comparer<TValue>.Default.Compare(compiled(l), compiled(r)),
            (rect, t) => EditorGUI.LabelField(rect, compiled(t)?.ToString() ?? ""),
            t => compiled(t)?.ToString() ?? ""
        );
    }

    protected static Column CreateReferencesColumn() => 
        CreateCollectionColumn<List<Object>, Object>("Refs", ColumnType.Int, null, t => t.Refs, AssetDatabase.GetAssetPath);

    protected static Column CreateDependenciesColumn() => 
        CreateCollectionColumn<List<Object>, Object>("Deps", ColumnType.Int, null, t => t.Deps, AssetDatabase.GetAssetPath);

    protected static Column CreateCollectionColumn<TCollection, TItem> 
    (
        string name,
        ColumnType type,
        GUIStyle style,
        Func<T, TCollection> getCollection,
        Func<TItem, string> formatter
    ) where TCollection : ICollection<TItem>
    {
        return new(name, type,
            (l, r) => getCollection(l)?.Count.CompareTo(getCollection(r)?.Count ?? 0) ?? 0,
            (rect, t) =>
            {
                TCollection collection = getCollection(t);
                if (collection == null) return;
                int count = collection.Count;
                if (count <= 0)
                {
                    EditorGUI.LabelField(rect, "0");
                    return;
                }

                bool press = style != null ? 
                    EditorGUI.DropdownButton(rect, new(count.ToString()), FocusType.Passive, style) :
                    EditorGUI.DropdownButton(rect, new(count.ToString()), FocusType.Passive);

                if (!press) return;
                var win = ScriptableObject.CreateInstance<SelectableLabelEditorWindow>();
                win.titleContent = new($"{t.AssetName} {name}");
                win.Init(collection.Aggregate("", (s, item) => $"{s}{formatter(item)}\n"));
                win.Show();
            },
            t => getCollection(t)?.Count.ToString());
    }
    
    protected static Column CreateGuidColumn() =>
        new("Guid", ColumnType.Int,
            (l, r) => String.Compare(l.Guid, r.Guid, StringComparison.Ordinal),
            (rect, t) => EditorGUI.LabelField(rect, t.Guid),
            t => t.Guid);

    protected static Column CreateWrittenColumn() =>
        new("Written", ColumnType.DateTime,
            (l, r) => l.WriteTime.CompareTo(r.WriteTime),
            (rect, t) => EditorGUI.LabelField(rect, t.WriteTimeAsString),
            t => t.WriteTimeAsString);
    
    //todo: cache string, watch out for things without importers
    protected static Column CreateTimestampColumn() =>
        new("Timestamp", ColumnType.DateTime,
            (l, r) => l.AssetImporter.assetTimeStamp.CompareTo(r.AssetImporter.assetTimeStamp),
            (rect, t) => EditorGUI.LabelField(rect, t.AssetImporter.assetTimeStamp.ToString()),
            t => t.AssetImporter.assetTimeStamp.ToString());

    protected static Column CreatePathColumn() =>
        new("Path", ColumnType.Path,
            (l, r) => string.CompareOrdinal(l.AssetPath, r.AssetPath),
            (rect, t) => EditorGUI.LabelField(rect, t.AssetPath), 
            rd => rd.AssetPath);

    protected static Column CreateRuntimeMemoryColumn() =>
        new("Runtime kB", ColumnType.FileSize,
            (l, r) => l.RuntimeMemory.CompareTo(r.RuntimeMemory),
            (rect, t) => EditorGUI.LabelField(rect, (t.RuntimeMemory / 1024).ToString()),
            t => (t.RuntimeMemory / 1024).ToString());

    protected static Column CreateStorageSizeColumn() =>
        new("Storage kB", ColumnType.FileSize,
            (l, r) => l.StorageSize.CompareTo(r.StorageSize),
            (rect, t) => EditorGUI.LabelField(rect, (t.StorageSize / 1024).ToString()),
            t => (t.StorageSize / 1024).ToString());

    protected override bool DoesItemMatchSearch(TreeViewItem<int> item, string search)
    {
        if (string.IsNullOrEmpty(search)) return true;
        if (multiColumnHeader?.state == null) return true;
        
        var t = item as T;
        for (var i = 0; i < multiColumnHeader.state.visibleColumns.Length; ++i)
        {
            var col = (Column)multiColumnHeader.GetColumn(i);
            if (col.text(t).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        }

        return false;
    }
    
    public bool DoesItemMatchSearch(TreeViewItem<int> item) => DoesItemMatchSearch(item, searchString);

    protected override void SelectionChanged(IList<int> selectedIds)
    {
        var selection = new Object[selectedIds.Count];
        for (var i = 0; i < selection.Length; ++i)
            selection[i] = (FindItem(selectedIds[i], Root) as T)?.Asset;
        Selection.objects = selection;
        Repaint();
    }
    
    IEnumerable<T> GetSelectionT() => GetSelection().Select(id => FindItem(id, Root) as T);

    void CopyRelativeAssetPathsToClipboard() => EditorGUIUtility.systemCopyBuffer = GetSelectionT().Select(a => a?.AssetPath).Join('\n');

    void CopyAssetPathsToClipboard()
    {
        string paths = GetSelectionT().Select(a => $"{IOUtils.ProjectRootFolder}/{a?.AssetPath}").Join('\n');
        EditorGUIUtility.systemCopyBuffer = paths;
    }

    void CopyReferencesToClipboard()
    {
        var refs = new List<Object>();
        foreach (T t in GetSelectionT()) refs.AddRange(t.Refs ?? new List<Object>());
        EditorGUIUtility.systemCopyBuffer = refs.Select(AssetDatabase.GetAssetPath).Join('\n');
    }

    void CopyDependenciesToClipboard()
    {
        var deps = new List<Object>();
        foreach (T s in GetSelectionT()) deps.AddRange(s.Deps ?? new List<Object>());
        EditorGUIUtility.systemCopyBuffer = deps.Select(AssetDatabase.GetAssetPath).Join('\n');
    }

    void CheckoutSelection()
    {
        foreach (T t in GetSelectionT()) AssetDatabase.MakeEditable(t.AssetPath);
    }

    void DeleteSelection()
    {
        bool any = false;
        foreach (T t in GetSelectionT())
        {
            any = true;
            AssetDatabase.DeleteAsset(t?.AssetPath);
            AllItems.Remove(t);
        }

        if (!any) return;
        Reload();
        Repaint();
    }
    
    protected override void ContextClickedItem(int id)
    {
        var menu = new GenericMenu();
        menu.AddItem(new("Checkout"), false, CheckoutSelection);
        menu.AddItem(new("Delete"), false, DeleteSelection);
        menu.AddSeparator("");
        menu.AddItem(new("Copy Path(s)"), false, CopyAssetPathsToClipboard);
        menu.AddSeparator("");
        menu.AddItem(new("Copy Refs to Clipboard"), false, CopyReferencesToClipboard);
        menu.AddItem(new("Copy Deps to Clipboard"), false, CopyDependenciesToClipboard);
        menu.ShowAsContext();
    }

    protected virtual string[] GatherGuids(bool sceneOnly, string path) => null;
    protected virtual void AddAsset(ref int id, string guid, string path) { }

    public void GatherAssets(bool sceneOnly, string path)
    {
        EditorUtility.DisplayProgressBar(TitleAssets, path, 0f);
        AllItems.Clear();

        string[] guids = GatherGuids(sceneOnly, path);
        if (guids == null)
        {
            Debug.LogWarning($"[AssetBrowser] GatherGuids returned null for path: {path}");
            EditorUtility.ClearProgressBar();
            Reload();
            return;
        }

        Debug.Log($"[AssetBrowser] Found {guids.Length} assets for path: {path}");

        var id = 1;
        for (var i = 0; i < guids.Length; ++i)
        {
            string guid = guids[i];
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (EditorUtility.DisplayCancelableProgressBar(TitleAssets, assetPath, i / (float)guids.Length))
                break;

            try
            {
                AddAsset(ref id, guid, assetPath);
            }
            catch (Exception e)
            {
                Debug.LogError($"[AssetBrowser] Error adding asset {assetPath}: {e.Message}\n{e.StackTrace}");
            }
        }

        EditorUtility.ClearProgressBar();
        Reload();
    }
    
    public virtual void FindReferences() {}

    public virtual void FindDependencies()
    {
        //uh use a real path i guess instead of ""
        EditorUtility.DisplayProgressBar(TitleDependencies, "", 0f);
        var countAsFloat = (float)AllItems.Count;
        
        for (var i = 0; i < AllItems.Count; ++i)
        {
            T row = AllItems[i];

            if (EditorUtility.DisplayCancelableProgressBar(
                    TitleDependencies,
                    row.AssetPath,
                    i / countAsFloat))
                break;

            if (!DoesItemMatchSearch(row)) continue;
            
            row.Deps.Clear();
            string[] deps = AssetDatabase.GetDependencies(row.AssetPath);
            foreach (string dep in deps)
            {
                if (row.AssetPath == dep) continue;
                row.Deps.Add(AssetDatabase.LoadAssetAtPath<Object>(dep));
            }
        }
        EditorUtility.ClearProgressBar();
    }
}