using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

public class FavoritesManager : EditorWindow
{
    static readonly HashSet<Object> Favorites = new();
    readonly Dictionary<Object, Texture2D> _iconCache = new();
    Vector2 scrollPos;
    string filter = "";
    const string Key = "FavoritesManager.Favorites";

    [MenuItem("‽/Favorites Manager")]
    static void Init() => GetWindow<FavoritesManager>("Favorites").Load();

    void OnGUI()
    {
        filter = EditorGUILayout.TextField("Search", filter);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        List<Object> toRemove = new();
        foreach (var obj in Favorites)
        {
            if (obj && obj.name.Contains(filter, System.StringComparison.OrdinalIgnoreCase))
            {
                EditorGUILayout.BeginHorizontal();
                DrawIconAndLabel(obj);
                if (GUILayout.Button("X", GUILayout.Width(20))) toRemove.Add(obj);
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.EndScrollView();

        foreach (var obj in toRemove) RemoveFavorite(obj);

        if (GUILayout.Button("Add Selection")) AddFavorite(Selection.activeObject);
        HandleDragDrop();
    }

    void DrawIconAndLabel(Object obj)
    {
        var icon = GetIcon(obj);
        if (icon) GUILayout.Label(icon, GUILayout.Width(20), GUILayout.Height(20));

        using (new GUIColorScope(obj.GetTypeColor()))
        {
            if (GUILayout.Button(obj.name, GUILayout.ExpandWidth(true)))
                Selection.activeObject = obj;
        }
    }

    Texture2D GetIcon(Object obj)
    {
        if (!_iconCache.TryGetValue(obj, out var icon))
            _iconCache[obj] = icon = AssetPreview.GetMiniThumbnail(obj);
        return icon;
    }

    void AddFavorite(Object obj)
    {
        if (obj && Favorites.Add(obj)) Save();
    }

    void RemoveFavorite(Object obj)
    {
        if (Favorites.Remove(obj)) Save();
    }

    void Load()
    {
        Favorites.Clear();
        foreach (var id in EditorPrefs.GetString(Key).Split(','))
        {
            if (int.TryParse(id, out var instanceID))
                if (EditorUtility.EntityIdToObject(instanceID) is { } obj)
                    Favorites.Add(obj);
        }
    }

    void Save() => EditorPrefs.SetString(Key, string.Join(",", Favorites.Select(o => o.GetInstanceID())));

    void HandleDragDrop()
    {
        var evt = Event.current;
        if (evt.type is not (EventType.DragUpdated or EventType.DragPerform)) return;

        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        if (evt.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();
            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (obj)
                    AddFavorite(obj);
            }
        }

        evt.Use();
    }
}