using System;
using System.Collections.Generic;
using UnityEngine;

public class Match3TilePool : MonoBehaviour
{
    [Header("Tile Prefabs")]
    [SerializeField] private GameObject _tilePrefab;

    [Header("Pool Settings")]
    [SerializeField] private int _initialPoolSize = 64;
    [SerializeField] private int _maxPoolSize = 128;
    [SerializeField] private Transform _poolContainer;

    private ObjectPoolEx<GameObject> _pool;
    private readonly Dictionary<Match3Tile.ColorType, Color> _colorMap = new();

    private void Awake()
    {
        InitializeColorMap();
        InitializePool();
    }

    private void InitializeColorMap()
    {
        _colorMap[Match3Tile.ColorType.Red] = new Color(0.9f, 0.2f, 0.2f);
        _colorMap[Match3Tile.ColorType.Blue] = new Color(0.2f, 0.4f, 0.9f);
        _colorMap[Match3Tile.ColorType.Green] = new Color(0.2f, 0.8f, 0.3f);
        _colorMap[Match3Tile.ColorType.Yellow] = new Color(0.95f, 0.9f, 0.2f);
        _colorMap[Match3Tile.ColorType.Purple] = new Color(0.7f, 0.3f, 0.8f);
        _colorMap[Match3Tile.ColorType.Orange] = new Color(0.95f, 0.6f, 0.2f);
    }

    private void InitializePool()
    {
        if (_poolContainer == null)
        {
            var containerObj = new GameObject("TilePool");
            containerObj.transform.SetParent(transform);
            _poolContainer = containerObj.transform;
        }

        _pool = new ObjectPoolEx<GameObject>(
            CreateTileInstance,
            _initialPoolSize,
            _maxPoolSize,
            callDisposeOnDestroy: false
        );
    }

    private GameObject CreateTileInstance()
    {
        GameObject tileObj;

        if (_tilePrefab != null)
        {
            tileObj = Instantiate(_tilePrefab, _poolContainer);
        }
        else
        {
            // Create a simple tile if no prefab is provided
            tileObj = CreateSimpleTile();
        }

        // Ensure it has a Match3Tile component
        if (!tileObj.TryGetComponent<Match3Tile>(out _))
        {
            tileObj.AddComponent<Match3Tile>();
        }

        tileObj.SetActive(false);
        return tileObj;
    }

    private GameObject CreateSimpleTile()
    {
        var tileObj = new GameObject("Tile");
        tileObj.transform.SetParent(_poolContainer);

        // Add sprite renderer
        var spriteRenderer = tileObj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateCircleSprite();
        spriteRenderer.sortingOrder = 1;

        // Add collider for input
        var collider = tileObj.AddComponent<CircleCollider2D>();
        collider.radius = 0.45f;

        // Add booster indicator child
        var boosterObj = new GameObject("BoosterIndicator");
        boosterObj.transform.SetParent(tileObj.transform);
        boosterObj.transform.localPosition = Vector3.zero;
        boosterObj.transform.localScale = Vector3.one * 0.5f;

        var boosterRenderer = boosterObj.AddComponent<SpriteRenderer>();
        boosterRenderer.sprite = CreateStarSprite();
        boosterRenderer.color = Color.white;
        boosterRenderer.sortingOrder = 2;
        boosterObj.SetActive(false);

        return tileObj;
    }

    private Sprite CreateCircleSprite()
    {
        // Create a simple circle texture
        int size = 64;
        var texture = new Texture2D(size, size);
        var center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 2;

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);
                Color color = distance <= radius ? Color.white : Color.clear;
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private Sprite CreateStarSprite()
    {
        // Simple diamond/star shape
        int size = 32;
        var texture = new Texture2D(size, size);
        var center = new Vector2(size / 2f, size / 2f);

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var pos = new Vector2(x, y);
                float dx = Mathf.Abs(pos.x - center.x);
                float dy = Mathf.Abs(pos.y - center.y);
                bool inDiamond = (dx + dy) <= size / 2f - 2;
                texture.SetPixel(x, y, inDiamond ? Color.white : Color.clear);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    public Match3Tile GetTile(Match3Tile.TileType type, Match3Tile.ColorType color)
    {
        GameObject tileObj = _pool.Get();
        if (tileObj == null)
        {
            DLog.LogE("Match3TilePool: Failed to get tile from pool");
            return null;
        }

        if (!tileObj.TryGetComponent<Match3Tile>(out var tile))
        {
            DLog.LogE("Match3TilePool: Tile GameObject missing Match3Tile component");
            return null;
        }

        // Configure the tile
        tile.Initialize(type, color, Vector2Int.zero);

        // Set visual color
        if (tileObj.TryGetComponent<SpriteRenderer>(out var renderer))
        {
            renderer.color = _colorMap.TryGetValue(color, out Color tileColor) ? tileColor : Color.white;
        }

        return tile;
    }

    public void ReturnTile(Match3Tile tile)
    {
        if (tile == null) return;

        tile.Deselect();
        tile.gameObject.SetActive(false);
        _pool.Release(tile.gameObject);
    }

    public void ReturnAllActiveTiles()
    {
        var activeTiles = GetComponentsInChildren<Match3Tile>(false);
        foreach (Match3Tile tile in activeTiles)
        {
            if (tile.gameObject.activeSelf)
                ReturnTile(tile);
        }
    }

    public void Clear()
    {
        ReturnAllActiveTiles();
        _pool.Clear();
    }

    public void SetTilePrefab(GameObject prefab)
    {
        _tilePrefab = prefab;
    }

    private void OnDestroy()
    {
        _pool?.Clear();
    }
}
