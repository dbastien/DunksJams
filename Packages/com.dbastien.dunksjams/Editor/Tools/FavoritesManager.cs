using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

public class FavoritesManager : EditorWindow
{
    private const string PrefsKey = "FavoritesManager.Guids";
    private const float RowHeight = 22f;

    private static readonly string[] ScriptExtensions =
    {
        ".cs", ".compute", ".shader", ".cginc", ".hlsl", ".json"
    };

    private readonly Dictionary<string, Texture2D> _iconCache = new();
    private readonly List<string> _pendingAdd = new();

    private List<string> _guids = new();
    private Vector2 _scrollPos;

    private SearchField _searchField;
    private string _filter = "";

    private double _lastClickTime;
    private string _lastClickGuid;

    private string _pendingRemoveGuid;

    private bool _reorderDragging;
    private int _dragFromIndex = -1;
    private int _dragToIndex = -1;
    private Vector2 _dragStartMouse;
    private const float DragStartThreshold = 6f;

    [MenuItem("‽/Favorites Manager")]
    private static void Open() => GetWindow<FavoritesManager>("Favorites").Show();

    private void OnEnable()
    {
        _searchField ??= new SearchField();
        Load();
    }

    private void OnGUI()
    {
        ProcessPendingOperations();

        DrawToolbar();

        HandleExternalDragDrop(new Rect(0, 0, position.width, position.height));

        using (var scroll = new EditorGUILayout.ScrollViewScope(_scrollPos))
        {
            _scrollPos = scroll.scrollPosition;
            DrawRows();
        }

        HandleReorderDragEnd();
    }

    private void DrawToolbar()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            GUILayout.Space(6);

            string next = _searchField.OnToolbarGUI(_filter);
            if (!string.Equals(next, _filter, StringComparison.Ordinal))
            {
                _filter = next ?? "";
                Repaint();
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("+ Selection", EditorStyles.toolbarButton, GUILayout.Width(95)))
                AddFromSelection();
        }
    }

    private void DrawRows()
    {
        int visibleIndex = 0;

        for (int i = 0; i < _guids.Count; i++)
        {
            string guid = _guids[i];
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (string.IsNullOrEmpty(path))
            {
                _pendingRemoveGuid = guid;
                continue;
            }

            Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (!obj)
            {
                _pendingRemoveGuid = guid;
                continue;
            }

            if (!PassesFilter(obj, path))
                continue;

            Rect rowRect = EditorGUILayout.GetControlRect(false, RowHeight);

            if (_reorderDragging)
                DrawDropIndicator(rowRect, _dragToIndex == i);

            DrawRow(rowRect, i, guid, path, obj, visibleIndex);
            visibleIndex++;
        }

        Rect tail = EditorGUILayout.GetControlRect(false, 2f);
        if (_reorderDragging)
            DrawDropIndicator(tail, _dragToIndex == _guids.Count);
    }

    private bool PassesFilter(Object obj, string path)
    {
        if (string.IsNullOrWhiteSpace(_filter)) return true;

        string needle = _filter.Trim();
        if (needle.Length == 0) return true;

        string haystack = $"{obj.name} {path}";
        return haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private void DrawRow(Rect rowRect, int index, string guid, string path, Object obj, int visibleIndex)
    {
        Event evt = Event.current;

        Color baseStripe = (visibleIndex & 1) == 0 ? new Color(0, 0, 0, 0.06f) : new Color(0, 0, 0, 0.02f);
        EditorGUI.DrawRect(rowRect, baseStripe);

        Color typeTint = GetTypeTint(obj, path);
        typeTint.a = 0.18f;
        EditorGUI.DrawRect(rowRect, typeTint);

        Rect contentRect = rowRect;
        contentRect.xMin += 6;
        contentRect.xMax -= 6;

        Rect iconRect = new Rect(contentRect.x, contentRect.y + 1, RowHeight - 2, RowHeight - 2);
        Rect xRect = new Rect(contentRect.xMax - 20, contentRect.y + 1, 20, RowHeight - 2);
        Rect labelRect = new Rect(iconRect.xMax + 6, contentRect.y + 1, xRect.xMin - (iconRect.xMax + 10), RowHeight - 2);

        Texture2D icon = GetIcon(guid, obj);
        if (icon)
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);

        bool hovered = rowRect.Contains(evt.mousePosition);
        if (hovered)
            EditorGUI.DrawRect(rowRect, new Color(1f, 1f, 1f, 0.05f));

        if (GUI.Button(xRect, "X", EditorStyles.miniButton))
        {
            _pendingRemoveGuid = guid;
            GUIUtility.ExitGUI();
            return;
        }

        if (GUI.Button(labelRect, obj.name, EditorStyles.label))
        {
            HandleClick(guid, path);
            evt.Use();
        }

        HandleReorderDragStart(rowRect, index);
        HandleReorderDragUpdate(rowRect, index);
    }

    private static void DrawDropIndicator(Rect rowRect, bool active)
    {
        Rect r = new Rect(rowRect.xMin + 8, rowRect.yMin - 1, rowRect.width - 16, 2);
        if (active && Event.current.type == EventType.Repaint)
            EditorGUI.DrawRect(r, new Color(0.3f, 0.6f, 1f, 0.9f));
    }

    private void HandleClick(string guid, string path)
    {
        bool isDoubleClick = guid == _lastClickGuid &&
                             EditorApplication.timeSinceStartup - _lastClickTime < 0.3;

        _lastClickGuid = guid;
        _lastClickTime = EditorApplication.timeSinceStartup;

        string ext = System.IO.Path.GetExtension(path).ToLowerInvariant();

        if (AssetDatabase.IsValidFolder(path))
        {
            RevealInProjectBrowser(path);
            return;
        }

        if (isDoubleClick && ScriptExtensions.Contains(ext))
        {
            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<Object>(path));
            return;
        }

        string folder = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
        RevealInProjectBrowser(folder);

        var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
        EditorGUIUtility.PingObject(asset);
        Selection.activeObject = asset;
    }

    private static void RevealInProjectBrowser(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath)) return;
        var folder = AssetDatabase.LoadAssetAtPath<Object>(folderPath);
        if (folder) Selection.activeObject = folder;
    }

    private Texture2D GetIcon(string guid, Object obj)
    {
        if (_iconCache.TryGetValue(guid, out Texture2D cached) && cached)
            return cached;

        Texture2D tex =
            AssetPreview.GetAssetPreview(obj) ??
            AssetPreview.GetMiniThumbnail(obj) ??
            (obj ? AssetPreview.GetMiniTypeThumbnail(obj.GetType()) : null);

        if (tex)
            _iconCache[guid] = tex;

        return tex;
    }

    private static Color GetTypeTint(Object obj, string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return new Color(0.25f, 0.55f, 1f, 1f);

        string ext = System.IO.Path.GetExtension(path).ToLowerInvariant();
        if (ext == ".cs") return new Color(0.35f, 1f, 0.55f, 1f);
        if (ext == ".shader" || ext == ".hlsl" || ext == ".cginc" || ext == ".compute") return new Color(1f, 0.55f, 0.25f, 1f);

        Type t = obj.GetType();
        if (typeof(Material).IsAssignableFrom(t)) return new Color(0.95f, 0.65f, 0.2f, 1f);
        if (typeof(Texture).IsAssignableFrom(t)) return new Color(0.6f, 0.85f, 1f, 1f);
        if (typeof(SceneAsset).IsAssignableFrom(t)) return new Color(0.75f, 0.65f, 1f, 1f);
        if (typeof(GameObject).IsAssignableFrom(t)) return new Color(1f, 0.45f, 0.65f, 1f);

        return new Color(0.7f, 0.7f, 0.7f, 1f);
    }

    private void HandleExternalDragDrop(Rect windowRect)
    {
        if (_reorderDragging) return;

        Event evt = Event.current;
        if (!windowRect.Contains(evt.mousePosition)) return;

        if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
            return;

        bool hasRefs = DragAndDrop.objectReferences != null && DragAndDrop.objectReferences.Length > 0;
        DragAndDrop.visualMode = hasRefs ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;

        if (evt.type == EventType.DragPerform && hasRefs)
        {
            DragAndDrop.AcceptDrag();

            foreach (Object o in DragAndDrop.objectReferences)
            {
                if (!o) continue;
                string path = AssetDatabase.GetAssetPath(o);
                string guid = AssetDatabase.AssetPathToGUID(path);
                if (!string.IsNullOrEmpty(guid))
                    _pendingAdd.Add(guid);
            }

            ProcessPendingOperations();
            Repaint();
        }

        evt.Use();
    }

    private void HandleReorderDragStart(Rect rowRect, int index)
    {
        Event evt = Event.current;
        if (evt.type != EventType.MouseDown) return;
        if (evt.button != 0) return;
        if (!rowRect.Contains(evt.mousePosition)) return;
        if (_reorderDragging) return;

        _dragFromIndex = index;
        _dragToIndex = index;
        _dragStartMouse = evt.mousePosition;

        evt.Use();
    }

    private void HandleReorderDragUpdate(Rect rowRect, int index)
    {
        Event evt = Event.current;

        if (_dragFromIndex < 0) return;

        if (!_reorderDragging && evt.type == EventType.MouseDrag)
        {
            if (Vector2.Distance(evt.mousePosition, _dragStartMouse) >= DragStartThreshold)
            {
                _reorderDragging = true;
                evt.Use();
            }
        }

        if (!_reorderDragging) return;

        if (evt.type == EventType.MouseDrag || evt.type == EventType.MouseMove || evt.type == EventType.Repaint || evt.type == EventType.Layout)
        {
            float midY = rowRect.y + rowRect.height * 0.5f;
            if (evt.mousePosition.y < midY) _dragToIndex = index;
            else _dragToIndex = index + 1;

            _dragToIndex = Mathf.Clamp(_dragToIndex, 0, _guids.Count);
            Repaint();
        }
    }

    private void HandleReorderDragEnd()
    {
        Event evt = Event.current;

        if (_reorderDragging && (evt.type == EventType.MouseUp || evt.type == EventType.MouseLeaveWindow))
        {
            if (_dragFromIndex >= 0 &&
                _dragToIndex >= 0 &&
                _dragFromIndex != _dragToIndex &&
                _dragToIndex != _dragFromIndex + 1)
            {
                string guid = _guids[_dragFromIndex];
                _guids.RemoveAt(_dragFromIndex);

                int insertAt = _dragToIndex > _dragFromIndex ? _dragToIndex - 1 : _dragToIndex;
                insertAt = Mathf.Clamp(insertAt, 0, _guids.Count);

                _guids.Insert(insertAt, guid);
                Save();
            }

            _reorderDragging = false;
            _dragFromIndex = -1;
            _dragToIndex = -1;
            evt.Use();
            Repaint();
        }

        if (!_reorderDragging && _dragFromIndex >= 0 && evt.type == EventType.MouseUp)
        {
            _dragFromIndex = -1;
            _dragToIndex = -1;
        }
    }

    private void AddFromSelection()
    {
        if (!Selection.activeObject) return;
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        string guid = AssetDatabase.AssetPathToGUID(path);
        if (!string.IsNullOrEmpty(guid))
            _pendingAdd.Add(guid);

        ProcessPendingOperations();
        Repaint();
    }

    private void ProcessPendingOperations()
    {
        bool modified = false;

        if (_pendingRemoveGuid != null)
        {
            _guids.Remove(_pendingRemoveGuid);
            _pendingRemoveGuid = null;
            modified = true;
        }

        if (_pendingAdd.Count > 0)
        {
            foreach (string guid in _pendingAdd)
            {
                if (!_guids.Contains(guid))
                    _guids.Add(guid);
            }
            _pendingAdd.Clear();
            modified = true;
        }

        if (modified)
            Save();
    }

    private void Load()
    {
        string raw = EditorPrefs.GetString(PrefsKey, "");
        _guids = string.IsNullOrEmpty(raw)
            ? new List<string>()
            : raw.Split(',').Where(g => !string.IsNullOrEmpty(g)).ToList();
    }

    private void Save() => EditorPrefs.SetString(PrefsKey, string.Join(",", _guids));
}