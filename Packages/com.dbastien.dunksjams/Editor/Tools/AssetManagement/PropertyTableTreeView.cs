#if UNITY_EDITOR
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;
    using UnityEngine;
    using Object = UnityEngine.Object;

    enum ApplyMode
    {
        None,
        SelectedRows,
        FilteredRows,
        AllRows
    }

    sealed class PropertyTableModel : IDisposable
    {
        public Func<Object[]> Gather;
        public Func<Object, int> RowKey;
        public Func<Object, int> SelectionKey;

        public readonly List<TableColumn> Columns = new();

        readonly Dictionary<int, TableRow> _rowsByKey = new();
        readonly List<TableRow> _rows = new();

        public IReadOnlyList<TableRow> Rows => _rows;

        public PropertyTableModel(Func<Object[]> gather)
        {
            Gather = gather ?? throw new ArgumentNullException(nameof(gather));

            // Default identity: unique per row.
            RowKey = o => o != null ? o.GetInstanceID() : 0;

            // Default selection mapping: components map to their GameObject (so hierarchy selection works),
            // otherwise object maps to itself.
            SelectionKey = o =>
            {
                if (o == null) return 0;
                if (o is Component c && c != null && c.gameObject != null)
                    return c.gameObject.GetInstanceID();
                return o.GetInstanceID();
            };
        }

        public bool Refresh()
        {
            var gathered = Gather?.Invoke() ?? Array.Empty<Object>();

            // Filter destroyed/null objects, preserve order.
            var objs = new List<Object>(gathered.Length);
            for (var i = 0; i < gathered.Length; i++)
            {
                var o = gathered[i];
                if (o != null) objs.Add(o);
            }

            // Compute new key list (order-sensitive).
            var newKeys = new List<int>(objs.Count);
            for (var i = 0; i < objs.Count; i++)
                newKeys.Add(RowKey(objs[i]));

            // Remove rows that no longer exist.
            var changed = false;
            if (_rows.Count > 0)
            {
                var newKeySet = new HashSet<int>(newKeys);
                for (var i = _rows.Count - 1; i >= 0; i--)
                {
                    var r = _rows[i];
                    if (!newKeySet.Contains(r.RowId))
                    {
                        _rows.RemoveAt(i);
                        _rowsByKey.Remove(r.RowId);
                        r.Dispose();
                        changed = true;
                    }
                }
            }

            // Rebuild ordered list, reusing existing rows where possible.
            var newRows = new List<TableRow>(objs.Count);
            for (var i = 0; i < objs.Count; i++)
            {
                var obj = objs[i];
                var key = RowKey(obj);

                if (!_rowsByKey.TryGetValue(key, out var row) || row == null || row.Target != obj)
                {
                    // If the key maps to a different object (rare but possible), replace.
                    row?.Dispose();
                    row = new TableRow(obj, key, SelectionKey(obj));
                    _rowsByKey[key] = row;
                    changed = true;
                }
                else
                {
                    // Keep selection key updated (in case the mapping logic changes for same target).
                    row.SelectionId = SelectionKey(obj);
                }

                newRows.Add(row);
            }

            if (!changed)
            {
                // Detect reorder without churn.
                if (_rows.Count != newRows.Count) changed = true;
                else
                    for (var i = 0; i < newRows.Count; i++)
                    {
                        if (!ReferenceEquals(_rows[i], newRows[i]))
                        {
                            changed = true;
                            break;
                        }
                    }
            }

            if (changed)
            {
                _rows.Clear();
                _rows.AddRange(newRows);
            }

            return changed;
        }

        public void Dispose()
        {
            for (var i = 0; i < _rows.Count; i++)
                _rows[i]?.Dispose();

            _rows.Clear();
            _rowsByKey.Clear();
        }
    }

    sealed class PropertyTable : IDisposable
    {
        public readonly PropertyTableModel Model;

        public bool ShowToolbar = true;
        public bool ShowSearch = true;
        public bool ResizableHeight = true;

        public bool FilterToUnitySelection = false;
        public ApplyMode MultiApplyMode = ApplyMode.SelectedRows;

        public float Height
        {
            get => _height;
            set => _height = Mathf.Max(value, GetMinHeight());
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value ?? string.Empty;
                if (_tree != null) _tree.searchString = _searchText;
            }
        }

        readonly string _uid;

        TreeViewState<int> _treeState;
        MultiColumnHeaderState _headerState;
        MultiColumnHeader _header;
        PropertyTableTreeView _tree;

        SearchField _searchField;
        string _searchText = string.Empty;

        float _height = 240f;

        // Resizer interaction
        const float kResizerH = 18f;
        bool _resizing;
        float _resizeStartMouseY;
        float _resizeStartHeight;

        public PropertyTable(string uid, PropertyTableModel model)
        {
            _uid = string.IsNullOrEmpty(uid)
                ? throw new ArgumentException("uid must be stable and non-empty", nameof(uid))
                : uid;
            Model = model ?? throw new ArgumentNullException(nameof(model));

            OnEnable();
        }

        public void OnEnable()
        {
            _height = SessionState.GetFloat(_uid + ":Height", _height);
            _searchText = SessionState.GetString(_uid + ":Search", _searchText);
        }

        public void OnDisable()
        {
            SaveState();
        }

        public void Dispose()
        {
            SaveState();
            // Model is not owned unless you decide it is; leaving it to caller is typical.
        }

        public void Rebuild()
        {
            // Call when you change Model.Columns schema.
            _treeState = _treeState ?? new TreeViewState<int>();

            _headerState = new MultiColumnHeaderState(Model.Columns.Select(c => c.BuildHeaderColumn()).ToArray());
            _header = new MultiColumnHeader(_headerState);
            _header.sortingChanged += _ => _tree?.Reload();
            _header.visibleColumnsChanged += _ => _tree?.Reload();

            _tree = new PropertyTableTreeView(_treeState, _header, this);
            _searchField ??= new SearchField();

            DeserializeState();

            _tree.searchString = _searchText;
            _tree.Reload();
        }

        public void OnInspectorUpdate()
        {
            EnsureInitialized();

            var modelChanged = Model.Refresh();
            if (modelChanged)
                _tree.FullReload();

            // Keep visible rows SO fresh (cheap), TreeView can request repaint
            if (_tree.UpdateVisibleSerializedObjects())
                _tree.Repaint();
        }

        public void OnHierarchyChange()
        {
            EnsureInitialized();
            if (Model.Refresh())
                _tree.FullReload();
        }

        public void OnSelectionChange()
        {
            EnsureInitialized();
            _tree.SetUnitySelection(Selection.objects.Select(o => o.GetInstanceID()).ToArray());

            if (FilterToUnitySelection)
                _tree.SetUnitySelectionFilter(Selection.objects.Select(o => o.GetInstanceID()).ToArray());
        }

        public void OnGUI()
        {
            EnsureInitialized();

            Rect hostRect;
            if (ResizableHeight)
                hostRect = GUILayoutUtility.GetRect(
                    0f, 10000f,
                    _height, _height,
                    GUILayout.ExpandWidth(true));
            else
                hostRect = GUILayoutUtility.GetRect(
                    0f, 10000f,
                    0f, float.MaxValue,
                    GUILayout.ExpandWidth(true),
                    GUILayout.ExpandHeight(true));

            if (Event.current.type == EventType.Layout)
                return;

            OnGUI(hostRect);
        }

        public void OnGUI(Rect hostRect)
        {
            EnsureInitialized();

            // Layout:
            // [Toolbar (optional)]
            // [Tree]
            // [Resizer (optional)]
            var toolbarH = ShowToolbar ? EditorGUIUtility.singleLineHeight + 4f : 0f;
            var resizerH = ResizableHeight ? kResizerH : 0f;

            var toolbarRect = new Rect(hostRect.x, hostRect.y, hostRect.width, toolbarH);
            var treeRect = new Rect(hostRect.x, hostRect.y + toolbarH, hostRect.width,
                Mathf.Max(0f, hostRect.height - toolbarH - resizerH));
            var resizerRect = new Rect(hostRect.x, hostRect.yMax - resizerH, hostRect.width, resizerH);

            if (ShowToolbar)
                DrawToolbar(toolbarRect);

            _tree.OnGUI(treeRect);

            if (ResizableHeight)
                HandleResizer(resizerRect);

            if (_tree.IsFilterDirty())
                _tree.Reload();
        }

        void DrawToolbar(Rect r)
        {
            // Compact toolbar style.
            r.height = EditorGUIUtility.singleLineHeight;
            r.y += 2f;

            var left = r;
            left.width = 160f;

            // ApplyMode dropdown
            EditorGUI.BeginChangeCheck();
            var newApply = (ApplyMode)EditorGUI.EnumPopup(left, MultiApplyMode);
            if (EditorGUI.EndChangeCheck())
                MultiApplyMode = newApply;

            // Filter-to-selection toggle
            var mid = r;
            mid.x += left.width + 6f;
            mid.width = 160f;

            EditorGUI.BeginChangeCheck();
            var newFilter = EditorGUI.ToggleLeft(mid, "Filter Selection", FilterToUnitySelection);
            if (EditorGUI.EndChangeCheck())
            {
                FilterToUnitySelection = newFilter;
                if (FilterToUnitySelection)
                    _tree.SetUnitySelectionFilter(Selection.objects.Select(o => o.GetInstanceID()).ToArray());
                else
                    _tree.SetUnitySelectionFilter(null);

                _tree.Reload();
            }

            // Search on the right
            if (ShowSearch)
            {
                var searchRect = r;
                searchRect.xMin = mid.xMax + 6f;

                EditorGUI.BeginChangeCheck();
                var s = _searchField.OnGUI(searchRect, _searchText);
                if (EditorGUI.EndChangeCheck())
                {
                    _searchText = s ?? string.Empty;
                    _tree.searchString = _searchText;
                    _tree.Reload();
                }
            }
        }

        void HandleResizer(Rect r)
        {
            // A centered grip.
            var grip = r;
            grip.height = 10f;
            grip.y += (r.height - grip.height) * 0.5f;
            grip.width = 32f;
            grip.x += (r.width - grip.width) * 0.5f;

            EditorGUIUtility.AddCursorRect(r, MouseCursor.ResizeVertical);

            var id = GUIUtility.GetControlID(FocusType.Passive);
            var e = Event.current;

            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0 && r.Contains(e.mousePosition))
                    {
                        GUIUtility.hotControl = id;
                        _resizing = true;
                        _resizeStartMouseY = e.mousePosition.y;
                        _resizeStartHeight = _height;
                        e.Use();
                    }

                    break;

                case EventType.MouseDrag:
                    if (_resizing && GUIUtility.hotControl == id)
                    {
                        var dy = e.mousePosition.y - _resizeStartMouseY;
                        _height = Mathf.Max(GetMinHeight(), _resizeStartHeight + dy);
                        e.Use();
                    }

                    break;

                case EventType.MouseUp:
                    if (_resizing && GUIUtility.hotControl == id)
                    {
                        _resizing = false;
                        GUIUtility.hotControl = 0;
                        e.Use();
                    }

                    break;

                case EventType.Repaint:
                    GUIStyle gripStyle = "RL DragHandle";
                    gripStyle.Draw(grip, false, false, false, false);
                    break;
            }
        }

        float GetMinHeight()
        {
            var rowH = EditorGUIUtility.singleLineHeight;
            var toolbarH = ShowToolbar ? EditorGUIUtility.singleLineHeight + 4f : 0f;
            var resizerH = ResizableHeight ? kResizerH : 0f;

            var headerH = _header != null ? _header.height : 18f;
            return toolbarH + headerH + rowH * 3f + resizerH;
        }

        void EnsureInitialized()
        {
            if (_tree != null)
                return;

            Rebuild();
        }

        void SaveState()
        {
            if (_tree == null || _header == null)
                return;

            SessionState.SetFloat(_uid + ":Height", _height);
            SessionState.SetString(_uid + ":Search", _searchText ?? string.Empty);

            SessionState.SetString(_uid + ":TreeState", JsonUtility.ToJson(_tree.state));
            SessionState.SetString(_uid + ":HeaderState", JsonUtility.ToJson(_header.state));
        }

        void DeserializeState()
        {
            var treeJson = SessionState.GetString(_uid + ":TreeState", "");
            var headerJson = SessionState.GetString(_uid + ":HeaderState", "");

            if (!string.IsNullOrEmpty(treeJson))
                JsonUtility.FromJsonOverwrite(treeJson, _tree.state);

            if (!string.IsNullOrEmpty(headerJson))
                JsonUtility.FromJsonOverwrite(headerJson, _header.state);
        }

        // Used by the TreeView to get apply policy + columns
        internal IReadOnlyList<TableColumn> GetColumns() => Model.Columns;
        internal ApplyMode GetApplyMode() => MultiApplyMode;
        internal IReadOnlyList<TableRow> GetAllRows() => Model.Rows;
    }

    sealed class TableRow : IDisposable
    {
        public Object Target { get; private set; }
        public int RowId { get; private set; } // unique per row
        public int SelectionId { get; set; } // mapping against Unity selection

        public SerializedObject SerializedObject => _so;

        SerializedObject _so;

        public TableRow(Object target, int rowId, int selectionId)
        {
            Target = target;
            RowId = rowId;
            SelectionId = selectionId;
            _so = target != null ? new SerializedObject(target) : null;
        }

        public string DisplayName => Target != null ? Target.name : string.Empty;

        public bool IsValid => Target != null;

        public bool UpdateIfRequired()
        {
            if (_so == null || Target == null)
                return false;

            return _so.UpdateIfRequiredOrScript();
        }

        public void Apply()
        {
            if (_so == null || Target == null)
                return;

            _so.ApplyModifiedProperties();
        }

        public SerializedProperty Find(string propertyPath)
        {
            if (_so == null || Target == null || string.IsNullOrEmpty(propertyPath))
                return null;

            return _so.FindProperty(propertyPath);
        }

        public void Dispose()
        {
            SerializedObjectUtil.TryDispose(_so);
            _so = null;
            Target = null;
        }
    }

    abstract class TableColumn
    {
        public string Id;
        public GUIContent Header;
        public float Width = 100f;
        public float MinWidth = 40f;
        public float MaxWidth = 100000f;
        public bool Sortable = true;

        protected TableColumn(string id, string header)
        {
            Id = id;
            Header = new GUIContent(header);
        }

        internal virtual MultiColumnHeaderState.Column BuildHeaderColumn() =>
            new()
            {
                headerContent = Header,
                width = Width,
                minWidth = MinWidth,
                maxWidth = MaxWidth,
                autoResize = true,
                allowToggleVisibility = true,
                canSort = Sortable,
                sortingArrowAlignment = TextAlignment.Right
            };

        /// <summary>Draws the cell; return true if it changed and should be applied/propagated.</summary>
        public abstract bool DrawCell(Rect r, TableRow row);

        /// <summary>Compare two rows for sorting (only used if Sortable).</summary>
        public virtual int Compare(TableRow a, TableRow b) => 0;

        /// <summary>Apply value from source row into target row (for multi-apply).</summary>
        public virtual void Apply(TableRow target, TableRow source)
        {
        }
    }

    sealed class NameColumn : TableColumn
    {
        public NameColumn(string header = "Name") : base("name", header)
        {
            Sortable = true;
            Width = 220f;
            MinWidth = 120f;
        }

        public override bool DrawCell(Rect r, TableRow row)
        {
            if (row == null) return false;
            EditorGUI.LabelField(r, row.DisplayName);
            return false;
        }

        public override int Compare(TableRow a, TableRow b) =>
            string.Compare(a?.DisplayName, b?.DisplayName, StringComparison.OrdinalIgnoreCase);
    }

    sealed class PropertyPathColumn : TableColumn
    {
        public readonly string PropertyPath;

        public bool CenterToggle = false;

        public PropertyPathColumn(string id, string header, string propertyPath) : base(id, header)
        {
            PropertyPath = propertyPath;
            Sortable = true;
        }

        public override bool DrawCell(Rect r, TableRow row)
        {
            if (row == null || !row.IsValid)
                return false;

            var prop = row.Find(PropertyPath);
            if (prop == null)
            {
                EditorGUI.LabelField(r, "-");
                return false;
            }

            if (CenterToggle && prop.propertyType == SerializedPropertyType.Boolean)
            {
                var off = r.width * 0.5f - 8f;
                if (off > 0f) r.x += off;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(r, prop, GUIContent.none, true);
            return EditorGUI.EndChangeCheck();
        }

        public override void Apply(TableRow target, TableRow source)
        {
            if (target == null || source == null || !target.IsValid || !source.IsValid)
                return;

            var src = source.Find(PropertyPath);
            var dst = target.Find(PropertyPath);
            if (src == null || dst == null)
                return;

            PropertyCopyUtil.TryCopy(dst, src);
        }

        public override int Compare(TableRow a, TableRow b)
        {
            if (a == null || b == null) return 0;

            var pa = a.Find(PropertyPath);
            var pb = b.Find(PropertyPath);
            return PropertyCopyUtil.Compare(pa, pb);
        }
    }

    sealed class PropertyTableTreeView : TreeView<int>
    {
        readonly PropertyTable _table;

        List<TreeViewItem<int>> _allItems;
        List<TreeViewItem<int>> _visibleItems;

        int[] _unitySelectionFilter; // selection IDs (typically GameObject IDs)
        int _lastFilterHash;
        string _lastSearchUsed;

        sealed class RowItem : TreeViewItem<int>
        {
            public readonly TableRow Row;
            public RowItem(TableRow row) : base(row.RowId, 0, row.DisplayName) => Row = row;
        }

        public PropertyTableTreeView(TreeViewState<int> state, MultiColumnHeader header, PropertyTable table)
            : base(state, header)
        {
            _table = table ?? throw new ArgumentNullException(nameof(table));

            showAlternatingRowBackgrounds = true;
            showBorder = true;
            rowHeight = EditorGUIUtility.singleLineHeight;

            header.sortingChanged += _ => Reload();
            header.visibleColumnsChanged += _ => Reload();
        }

        public void FullReload()
        {
            _allItems = null;
            Reload();
        }

        public void SetUnitySelection(int[] unitySelectionIds)
        {
            unitySelectionIds ??= Array.Empty<int>();
            EnsureItemsBuilt();

            var selSet = new HashSet<int>(unitySelectionIds);
            var rowIds = new List<int>();

            if (_allItems != null)
                for (var i = 0; i < _allItems.Count; i++)
                {
                    var it = (RowItem)_allItems[i];
                    if (it.Row != null && selSet.Contains(it.Row.SelectionId))
                        rowIds.Add(it.id);
                }

            SetSelection(rowIds, TreeViewSelectionOptions.RevealAndFrame);
        }

        public void SetUnitySelectionFilter(int[] unitySelectionIds)
        {
            _unitySelectionFilter = unitySelectionIds != null ? (int[])unitySelectionIds.Clone() : null;
            _lastFilterHash = HashIds(_unitySelectionFilter);
        }

        public bool UpdateVisibleSerializedObjects()
        {
            if (_visibleItems == null || _visibleItems.Count == 0)
                return false;

            var changed = false;

            GetFirstAndLastVisibleRows(out var first, out var last);
            if (last < 0) return false;

            first = Mathf.Clamp(first, 0, _visibleItems.Count - 1);
            last = Mathf.Clamp(last, 0, _visibleItems.Count - 1);

            for (var i = first; i <= last; i++)
            {
                var row = ((RowItem)_visibleItems[i]).Row;
                if (row != null)
                    changed |= row.UpdateIfRequired();
            }

            // Keep selected rows fresh too (even if scrolled off)
            var selected = FindRows(GetSelection());
            if (selected != null)
                for (var i = 0; i < selected.Count; i++)
                {
                    if (selected[i] is RowItem ri && ri.Row != null)
                        changed |= ri.Row.UpdateIfRequired();
                }

            return changed;
        }

        public bool IsFilterDirty()
        {
            if (EditorGUIUtility.editingTextField)
                return false;

            if (!string.Equals(_lastSearchUsed ?? "", searchString ?? "", StringComparison.Ordinal))
                return true;

            var h = HashIds(_unitySelectionFilter);
            if (_unitySelectionFilter != null && h != _lastFilterHash)
                return true;

            return false;
        }

        protected override TreeViewItem<int> BuildRoot() => new(-1, -1, "root");

        protected override IList<TreeViewItem<int>> BuildRows(TreeViewItem<int> root)
        {
            EnsureItemsBuilt();

            var rows = new List<TreeViewItem<int>>(_allItems?.Count ?? 0);

            HashSet<int> selFilter = null;
            if (_table.FilterToUnitySelection && _unitySelectionFilter != null)
                selFilter = new HashSet<int>(_unitySelectionFilter);

            var tokens = Tokenize(searchString);

            if (_allItems != null)
                for (var i = 0; i < _allItems.Count; i++)
                {
                    var it = (RowItem)_allItems[i];
                    var row = it.Row;
                    if (row == null || !row.IsValid)
                        continue;

                    if (selFilter != null && !selFilter.Contains(row.SelectionId))
                        continue;

                    if (tokens != null && tokens.Length > 0 && !MatchesAllTokens(row.DisplayName, tokens))
                        continue;

                    it.displayName = row.DisplayName;
                    rows.Add(it);
                }

            ApplySorting(rows);

            _visibleItems = rows;
            _lastSearchUsed = searchString ?? "";
            _lastFilterHash = HashIds(_unitySelectionFilter);

            SetupParentsAndChildrenFromDepths(root, rows);
            return rows;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item as RowItem;
            if (item == null || item.Row == null)
                return;

            var cols = _table.GetColumns();
            for (var vis = 0; vis < args.GetNumVisibleColumns(); vis++)
            {
                var colIndex = args.GetColumn(vis);
                var col = cols[colIndex];

                var cell = args.GetCellRect(vis);
                CenterRectSingleLine(ref cell);

                var changed = false;

                EditorGUI.BeginDisabledGroup(!IsEditable(item.Row.Target));
                try
                {
                    changed = col.DrawCell(cell, item.Row);
                }
                finally
                {
                    EditorGUI.EndDisabledGroup();
                }

                if (changed) HandleCellEdited(item.Row, colIndex);
            }
        }

        void HandleCellEdited(TableRow editedRow, int columnIndex)
        {
            if (editedRow == null || !editedRow.IsValid)
                return;

            var cols = _table.GetColumns();
            if (columnIndex < 0 || columnIndex >= cols.Count)
                return;

            var col = cols[columnIndex];
            var mode = _table.GetApplyMode();

            // Determine targets to apply to (including edited row for Undo).
            var targets = new List<TableRow>();

            // Always include edited row (Undo + Apply).
            targets.Add(editedRow);

            if (mode != ApplyMode.None)
            {
                var others = Enumerable.Empty<TableRow>();

                switch (mode)
                {
                    case ApplyMode.SelectedRows:
                    {
                        var selIds = GetSelection();
                        if (selIds != null && selIds.Count > 0)
                        {
                            var selRows = FindRows(selIds);
                            if (selRows != null)
                                others = selRows.OfType<RowItem>().Select(ri => ri.Row);
                        }

                        break;
                    }

                    case ApplyMode.FilteredRows:
                    {
                        // Current visible rows
                        if (_visibleItems != null)
                            others = _visibleItems.OfType<RowItem>().Select(ri => ri.Row);
                        break;
                    }

                    case ApplyMode.AllRows:
                        others = _table.GetAllRows();
                        break;
                }

                foreach (var r in others)
                {
                    if (r == null || !r.IsValid) continue;
                    if (ReferenceEquals(r, editedRow)) continue;
                    if (!IsEditable(r.Target)) continue;

                    targets.Add(r);
                }
            }

            // Undo should be recorded BEFORE ApplyModifiedProperties.
            var undoTargets = targets
                .Select(t => t.Target)
                .Where(o => o != null)
                .Distinct()
                .ToArray();

            if (undoTargets.Length > 0)
                Undo.RecordObjects(undoTargets, $"Edit {col.Header.text}");

            // Apply edited row first (commits the change).
            editedRow.Apply();

            // Propagate to others.
            for (var i = 1; i < targets.Count; i++)
            {
                var t = targets[i];
                t.UpdateIfRequired();
                col.Apply(t, editedRow);
                t.Apply();
            }

            // Keep display names current (rename edge cases).
            Reload();
        }

        void EnsureItemsBuilt()
        {
            if (_allItems != null)
                return;

            // Make sure model is fresh at least once.
            _table.Model.Refresh();

            var rows = _table.GetAllRows();
            _allItems = new List<TreeViewItem<int>>(rows.Count);

            for (var i = 0; i < rows.Count; i++)
            {
                var r = rows[i];
                if (r == null || !r.IsValid) continue;
                _allItems.Add(new RowItem(r));
            }
        }

        void ApplySorting(List<TreeViewItem<int>> rows)
        {
            if (rows == null || rows.Count <= 1)
                return;

            var sortedCol = multiColumnHeader.sortedColumnIndex;
            if (sortedCol < 0)
                return;

            var cols = _table.GetColumns();
            if (sortedCol >= cols.Count)
                return;

            var col = cols[sortedCol];
            if (!col.Sortable)
                return;

            var asc = multiColumnHeader.IsSortedAscending(sortedCol);

            rows.Sort((a, b) =>
            {
                var ra = (a as RowItem)?.Row;
                var rb = (b as RowItem)?.Row;
                var cmp = col.Compare(ra, rb);
                return asc ? cmp : -cmp;
            });
        }

        static void CenterRectSingleLine(ref Rect r)
        {
            var h = EditorGUIUtility.singleLineHeight;
            r.y += (r.height - h) * 0.5f;
            r.height = h;
        }

        static bool IsEditable(Object target) => target != null && (target.hideFlags & HideFlags.NotEditable) == 0;

        static string[] Tokenize(string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            var parts = s.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 ? parts : null;
        }

        static bool MatchesAllTokens(string hay, string[] tokens)
        {
            if (tokens == null || tokens.Length == 0) return true;
            if (string.IsNullOrEmpty(hay)) return false;

            for (var i = 0; i < tokens.Length; i++)
            {
                if (hay.IndexOf(tokens[i], StringComparison.OrdinalIgnoreCase) < 0)
                    return false;
            }

            return true;
        }

        static int HashIds(int[] ids)
        {
            if (ids == null || ids.Length == 0) return 0;
            unchecked
            {
                var h = 17;
                for (var i = 0; i < ids.Length; i++)
                    h = h * 31 + ids[i];
                return h;
            }
        }
    }

    static class SerializedObjectUtil
    {
        static bool _checked;
        static MethodInfo _disposeMI;

        public static void TryDispose(SerializedObject so)
        {
            if (so == null) return;

            if (!_checked)
            {
                _checked = true;
                _disposeMI = typeof(SerializedObject).GetMethod("Dispose",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }

            if (_disposeMI != null)
                try
                {
                    _disposeMI.Invoke(so, null);
                }
                catch
                {
                    /* ignore */
                }
        }
    }

    static class PropertyCopyUtil
    {
        static bool _copyChecked;
        static Action<SerializedProperty, SerializedProperty> _copyFromProp; // open instance delegate

        static void EnsureCopyDelegate()
        {
            if (_copyChecked) return;
            _copyChecked = true;

            // SerializedProperty.CopyFromSerializedProperty(SerializedProperty other) exists in many Unity versions.
            var mi = typeof(SerializedProperty).GetMethod(
                "CopyFromSerializedProperty",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] { typeof(SerializedProperty) },
                null);

            if (mi != null)
                try
                {
                    _copyFromProp = (Action<SerializedProperty, SerializedProperty>)
                        Delegate.CreateDelegate(typeof(Action<SerializedProperty, SerializedProperty>), null, mi);
                }
                catch
                {
                    _copyFromProp = null;
                }
        }

        public static bool TryCopy(SerializedProperty dst, SerializedProperty src)
        {
            if (dst == null || src == null)
                return false;

            if (dst.propertyType != src.propertyType)
                return false;

            EnsureCopyDelegate();

            // Best path: let Unity do it (handles structs/arrays nicely).
            if (_copyFromProp != null)
                try
                {
                    _copyFromProp(dst, src);
                    return true;
                }
                catch
                {
                    // fall through
                }

            // Fallback: common types.
            switch (dst.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    dst.boolValue = src.boolValue;
                    return true;

                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Character:
                case SerializedPropertyType.LayerMask:
                    dst.intValue = src.intValue;
                    return true;

                case SerializedPropertyType.Enum:
                    dst.enumValueIndex = src.enumValueIndex;
                    return true;

                case SerializedPropertyType.Float:
                    dst.floatValue = src.floatValue;
                    return true;

                case SerializedPropertyType.String:
                    dst.stringValue = src.stringValue;
                    return true;

                case SerializedPropertyType.Color:
                    dst.colorValue = src.colorValue;
                    return true;

                case SerializedPropertyType.ObjectReference:
                case SerializedPropertyType.ExposedReference:
                    dst.objectReferenceValue = src.objectReferenceValue;
                    return true;

                case SerializedPropertyType.Vector2:
                    dst.vector2Value = src.vector2Value;
                    return true;

                case SerializedPropertyType.Vector3:
                    dst.vector3Value = src.vector3Value;
                    return true;

                case SerializedPropertyType.Vector4:
                    dst.vector4Value = src.vector4Value;
                    return true;

                case SerializedPropertyType.Rect:
                    dst.rectValue = src.rectValue;
                    return true;

                case SerializedPropertyType.Bounds:
                    dst.boundsValue = src.boundsValue;
                    return true;

                case SerializedPropertyType.Quaternion:
                    dst.quaternionValue = src.quaternionValue;
                    return true;

                case SerializedPropertyType.AnimationCurve:
                    dst.animationCurveValue = src.animationCurveValue;
                    return true;

                case SerializedPropertyType.Generic:
                default:
                    break;
            }

            // Array fallback (primitive-ish arrays only; complex structs want Unity copy delegate).
            if (dst.isArray && dst.propertyType != SerializedPropertyType.String)
                try
                {
                    dst.arraySize = src.arraySize;
                    for (var i = 0; i < src.arraySize; i++)
                        TryCopy(dst.GetArrayElementAtIndex(i), src.GetArrayElementAtIndex(i));
                    return true;
                }
                catch
                {
                    /* ignore */
                }

            return false;
        }

        public static int Compare(SerializedProperty a, SerializedProperty b)
        {
            if (a == null && b == null) return 0;
            if (a == null) return -1;
            if (b == null) return 1;

            if (a.propertyType != b.propertyType)
                return string.Compare(a.propertyType.ToString(), b.propertyType.ToString(), StringComparison.Ordinal);

            switch (a.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    return a.boolValue.CompareTo(b.boolValue);

                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Character:
                case SerializedPropertyType.LayerMask:
                    return a.intValue.CompareTo(b.intValue);

                case SerializedPropertyType.Enum:
                    return a.enumValueIndex.CompareTo(b.enumValueIndex);

                case SerializedPropertyType.Float:
                    return a.floatValue.CompareTo(b.floatValue);

                case SerializedPropertyType.String:
                    return string.Compare(a.stringValue, b.stringValue, StringComparison.OrdinalIgnoreCase);

                case SerializedPropertyType.ObjectReference:
                case SerializedPropertyType.ExposedReference:
                    return a.objectReferenceInstanceIDValue.CompareTo(b.objectReferenceInstanceIDValue);

                default:
                    // cheap fallback; not perfect but stable-ish
                    return string.Compare(a.displayName, b.displayName, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
#endif