using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

public class FavoritesManager : EditorWindow
{
    const string PrefsKey = "FavoritesManager.Guids";

    static readonly string[] ScriptExtensions = { ".cs", ".compute", ".shader", ".cginc", ".hlsl", ".json" };

    List<string> _guids = new();
    readonly Dictionary<string, Texture2D> _iconCache = new();
    Vector2 _scrollPos;
    string _filter = "";
    double _lastClickTime;
    string _lastClickGuid;

    // drag-to-reorder state
    int _dragFromIndex = -1;
    int _dragToIndex = -1;
    bool _reorderDragging;

    [MenuItem("‽/Favorites Manager")]
    static void Open() => GetWindow<FavoritesManager>("Favorites").Show();

    void OnEnable() => Load();

    void OnGUI()
    {
        DrawToolbar();

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        DrawRows();
        EditorGUILayout.EndScrollView();

        HandleExternalDragDrop();
    }

    // ── toolbar ──────────────────────────────────────────────────────────
    void DrawToolbar()
    {
        using (new GUIHorizontalScope())
        {
            _filter = EditorGUILayout.TextField("Search", _filter);
            if (GUILayout.Button("+ Selection", GUILayout.Width(80)))
                AddFromSelection();
        }
    }

    // ── rows ─────────────────────────────────────────────────────────────
    void DrawRows()
    {
        string removeGuid = null;

        for (int i = 0; i < _guids.Count; i++)
        {
            string guid = _guids[i];
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (string.IsNullOrEmpty(path)) { removeGuid = guid; continue; }

            string assetName = System.IO.Path.GetFileNameWithoutExtension(path);
            if (!string.IsNullOrEmpty(_filter) &&
                !assetName.Contains(_filter, StringComparison.OrdinalIgnoreCase))
                continue;

            var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (!obj) { removeGuid = guid; continue; }

            // reorder drop target indicator
            if (_reorderDragging && _dragToIndex == i)
                DrawDropIndicator();

            DrawRow(i, guid, path, obj);
        }

        // indicator after last row
        if (_reorderDragging && _dragToIndex == _guids.Count)
            DrawDropIndicator();

        if (removeGuid != null)
        {
            _guids.Remove(removeGuid);
            Save();
        }

        HandleReorderDragEnd();
    }

    void DrawRow(int index, string guid, string path, Object obj)
    {
        var rect = EditorGUILayout.BeginHorizontal();

        // icon
        var icon = GetIcon(guid, obj);
        if (icon)
            GUILayout.Label(new GUIContent(icon), GUILayout.Width(22), GUILayout.Height(22));

        // label
        using (new GUIColorScope(obj.GetTypeColor()))
        {
            if (GUILayout.Button(obj.name, EditorStyles.label, GUILayout.ExpandWidth(true)))
                HandleClick(guid, path);
        }

        // remove button
        if (GUILayout.Button("X", GUILayout.Width(20)))
        {
            _guids.Remove(guid);
            Save();
            GUIUtility.ExitGUI();
        }

        EditorGUILayout.EndHorizontal();

        // reorder drag source
        HandleReorderDragStart(rect, index);
        HandleReorderDragUpdate(rect, index);
    }

    static void DrawDropIndicator()
    {
        var r = GUILayoutUtility.GetRect(0, 2, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(r, new Color(0.3f, 0.6f, 1f, 0.9f));
    }

    // ── click ────────────────────────────────────────────────────────────
    void HandleClick(string guid, string path)
    {
        bool isDoubleClick = guid == _lastClickGuid &&
                             EditorApplication.timeSinceStartup - _lastClickTime < 0.3;
        _lastClickGuid = guid;
        _lastClickTime = EditorApplication.timeSinceStartup;

        string ext = System.IO.Path.GetExtension(path).ToLowerInvariant();

        if (AssetDatabase.IsValidFolder(path))
        {
            RevealInProjectBrowser(path);
        }
        else if (isDoubleClick && ScriptExtensions.Contains(ext))
        {
            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<Object>(path));
        }
        else
        {
            RevealInProjectBrowser(System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/'));
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(path));
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(path);
        }
    }

    static void RevealInProjectBrowser(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath)) return;
        var folder = AssetDatabase.LoadAssetAtPath<Object>(folderPath);
        if (folder) Selection.activeObject = folder;
    }

    // ── icon cache ───────────────────────────────────────────────────────
    Texture2D GetIcon(string guid, Object obj)
    {
        if (_iconCache.TryGetValue(guid, out var cached) && cached) return cached;

        var tex = AssetPreview.GetAssetPreview(obj)
               ?? AssetPreview.GetMiniThumbnail(obj)
               ?? (obj ? AssetPreview.GetMiniTypeThumbnail(obj.GetType()) : null);

        if (tex) _iconCache[guid] = tex;
        return tex;
    }

    // ── external drag-and-drop (add from Project window) ─────────────────
    void HandleExternalDragDrop()
    {
        if (_reorderDragging) return;

        var evt = Event.current;
        if (evt.type is not (EventType.DragUpdated or EventType.DragPerform)) return;

        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

        if (evt.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();
            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (!obj) continue;
                string path = AssetDatabase.GetAssetPath(obj);
                string guid = AssetDatabase.AssetPathToGUID(path);
                if (!string.IsNullOrEmpty(guid) && !_guids.Contains(guid))
                {
                    _guids.Add(guid);
                    Save();
                }
            }
        }

        evt.Use();
    }

    // ── reorder drag-and-drop ────────────────────────────────────────────
    void HandleReorderDragStart(Rect rowRect, int index)
    {
        var evt = Event.current;
        if (evt.type != EventType.MouseDrag || !rowRect.Contains(evt.mousePosition)) return;
        if (_reorderDragging) return;

        _reorderDragging = true;
        _dragFromIndex = index;
        _dragToIndex = index;

        DragAndDrop.PrepareStartDrag();
        DragAndDrop.objectReferences = Array.Empty<Object>();
        DragAndDrop.paths = Array.Empty<string>();
        DragAndDrop.StartDrag($"Reorder {_guids[index]}");
        evt.Use();
    }

    void HandleReorderDragUpdate(Rect rowRect, int index)
    {
        if (!_reorderDragging) return;

        var evt = Event.current;
        if (evt.type != EventType.DragUpdated) return;

        DragAndDrop.visualMode = DragAndDropVisualMode.Move;

        float midY = rowRect.y + rowRect.height * 0.5f;
        _dragToIndex = evt.mousePosition.y < midY ? index : index + 1;

        Repaint();
        evt.Use();
    }

    void HandleReorderDragEnd()
    {
        if (!_reorderDragging) return;

        var evt = Event.current;
        if (evt.type != EventType.DragPerform && evt.type != EventType.DragExited) return;

        if (evt.type == EventType.DragPerform && _dragFromIndex >= 0 && _dragToIndex >= 0 &&
            _dragFromIndex != _dragToIndex && _dragToIndex != _dragFromIndex + 1)
        {
            DragAndDrop.AcceptDrag();
            string guid = _guids[_dragFromIndex];
            _guids.RemoveAt(_dragFromIndex);
            int insertAt = _dragToIndex > _dragFromIndex ? _dragToIndex - 1 : _dragToIndex;
            _guids.Insert(insertAt, guid);
            Save();
        }

        _reorderDragging = false;
        _dragFromIndex = -1;
        _dragToIndex = -1;
        evt.Use();
        Repaint();
    }

    // ── add / persistence ────────────────────────────────────────────────
    void AddFromSelection()
    {
        if (!Selection.activeObject) return;
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        string guid = AssetDatabase.AssetPathToGUID(path);
        if (!string.IsNullOrEmpty(guid) && !_guids.Contains(guid))
        {
            _guids.Add(guid);
            Save();
        }
    }

    void Load()
    {
        string raw = EditorPrefs.GetString(PrefsKey, "");
        _guids = string.IsNullOrEmpty(raw)
            ? new List<string>()
            : raw.Split(',').Where(g => !string.IsNullOrEmpty(g)).ToList();
    }

    void Save() => EditorPrefs.SetString(PrefsKey, string.Join(",", _guids));
}
