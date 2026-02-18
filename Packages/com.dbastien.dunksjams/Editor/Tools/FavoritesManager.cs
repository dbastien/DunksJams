using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

public class FavoritesManager : EditorWindow
{
    private const string PrefsKey = "FavoritesManager.Guids";

    private static readonly string[] ScriptExtensions = { ".cs", ".compute", ".shader", ".cginc", ".hlsl", ".json" };

    private List<string> _guids = new();
    private readonly Dictionary<string, Texture2D> _iconCache = new();
    private Vector2 _scrollPos;
    private string _filter = "";
    private double _lastClickTime;
    private string _lastClickGuid;

    // drag-to-reorder state
    private int _dragFromIndex = -1;
    private int _dragToIndex = -1;
    private bool _reorderDragging;

    [MenuItem("‽/Favorites Manager")] private static void Open() => GetWindow<FavoritesManager>("Favorites").Show();

    private void OnEnable() => Load();

    private void OnGUI()
    {
        DrawToolbar();

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        DrawRows();
        EditorGUILayout.EndScrollView();

        HandleExternalDragDrop();
    }

    // ── toolbar ──────────────────────────────────────────────────────────
    private void DrawToolbar()
    {
        using (new GUIHorizontalScope())
        {
            _filter = EditorGUILayout.TextField("Search", _filter);
            if (GUILayout.Button("+ Selection", GUILayout.Width(80)))
                AddFromSelection();
        }
    }

    // ── rows ─────────────────────────────────────────────────────────────
    private void DrawRows()
    {
        string removeGuid = null;

        for (var i = 0; i < _guids.Count; i++)
        {
            string guid = _guids[i];
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (string.IsNullOrEmpty(path))
            {
                removeGuid = guid;
                continue;
            }

            string assetName = System.IO.Path.GetFileNameWithoutExtension(path);
            if (!string.IsNullOrEmpty(_filter) &&
                !assetName.Contains(_filter, StringComparison.OrdinalIgnoreCase))
                continue;

            var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (!obj)
            {
                removeGuid = guid;
                continue;
            }

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

    private void DrawRow(int index, string guid, string path, Object obj)
    {
        Rect rect = EditorGUILayout.BeginHorizontal();

        // icon
        Texture2D icon = GetIcon(guid, obj);
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

    private static void DrawDropIndicator()
    {
        Rect r = GUILayoutUtility.GetRect(0, 2, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(r, new Color(0.3f, 0.6f, 1f, 0.9f));
    }

    // ── click ────────────────────────────────────────────────────────────
    private void HandleClick(string guid, string path)
    {
        bool isDoubleClick = guid == _lastClickGuid &&
                             EditorApplication.timeSinceStartup - _lastClickTime < 0.3;
        _lastClickGuid = guid;
        _lastClickTime = EditorApplication.timeSinceStartup;

        string ext = System.IO.Path.GetExtension(path).ToLowerInvariant();

        if (AssetDatabase.IsValidFolder(path)) { RevealInProjectBrowser(path); }
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

    private static void RevealInProjectBrowser(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath)) return;
        var folder = AssetDatabase.LoadAssetAtPath<Object>(folderPath);
        if (folder) Selection.activeObject = folder;
    }

    // ── icon cache ───────────────────────────────────────────────────────
    private Texture2D GetIcon(string guid, Object obj)
    {
        if (_iconCache.TryGetValue(guid, out Texture2D cached) && cached) return cached;

        Texture2D tex = AssetPreview.GetAssetPreview(obj) ??
                        AssetPreview.GetMiniThumbnail(obj) ??
                        (obj ? AssetPreview.GetMiniTypeThumbnail(obj.GetType()) : null);

        if (tex) _iconCache[guid] = tex;
        return tex;
    }

    // ── external drag-and-drop (add from Project window) ─────────────────
    private void HandleExternalDragDrop()
    {
        if (_reorderDragging) return;

        Event evt = Event.current;
        if (evt.type is not (EventType.DragUpdated or EventType.DragPerform)) return;

        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

        if (evt.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();
            foreach (Object obj in DragAndDrop.objectReferences)
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
    private void HandleReorderDragStart(Rect rowRect, int index)
    {
        Event evt = Event.current;
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

    private void HandleReorderDragUpdate(Rect rowRect, int index)
    {
        if (!_reorderDragging) return;

        Event evt = Event.current;
        if (evt.type != EventType.DragUpdated) return;

        DragAndDrop.visualMode = DragAndDropVisualMode.Move;

        float midY = rowRect.y + rowRect.height * 0.5f;
        _dragToIndex = evt.mousePosition.y < midY ? index : index + 1;

        Repaint();
        evt.Use();
    }

    private void HandleReorderDragEnd()
    {
        if (!_reorderDragging) return;

        Event evt = Event.current;
        if (evt.type != EventType.DragPerform && evt.type != EventType.DragExited) return;

        if (evt.type == EventType.DragPerform &&
            _dragFromIndex >= 0 &&
            _dragToIndex >= 0 &&
            _dragFromIndex != _dragToIndex &&
            _dragToIndex != _dragFromIndex + 1)
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
    private void AddFromSelection()
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

    private void Load()
    {
        string raw = EditorPrefs.GetString(PrefsKey, "");
        _guids = string.IsNullOrEmpty(raw)
            ? new List<string>()
            : raw.Split(',').Where(g => !string.IsNullOrEmpty(g)).ToList();
    }

    private void Save() => EditorPrefs.SetString(PrefsKey, string.Join(",", _guids));
}