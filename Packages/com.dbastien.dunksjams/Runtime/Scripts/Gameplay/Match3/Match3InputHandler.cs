using System;
using System.Collections.Generic;
using UnityEngine;

public class Match3InputHandler : MonoBehaviour
{
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private float _lineWidth = 0.1f;
    [SerializeField] private Color _lineColor = Color.white;

    private Match3Grid _grid;
    private int _minMatchSize = 3;
    private bool _inputLocked;

    private readonly List<Match3Tile> _selectedTiles = new();
    private Match3Tile.ColorType _selectedColor = Match3Tile.ColorType.None;

    private Camera _mainCamera;
    private bool _isDragging;

    public event Action<List<Vector2Int>> OnMatchConfirmed;
    public event Action<Match3Tile> OnTileSelected;
    public event Action OnSelectionCleared;

    public bool IsInputLocked => _inputLocked;
    public int SelectedCount => _selectedTiles.Count;
    public IReadOnlyList<Match3Tile> SelectedTiles => _selectedTiles;

    private void Awake()
    {
        _mainCamera = Camera.main;

        // Create line renderer if not assigned
        if (_lineRenderer == null)
        {
            var lineObj = new GameObject("SelectionLine");
            lineObj.transform.SetParent(transform);
            _lineRenderer = lineObj.AddComponent<LineRenderer>();
            _lineRenderer.startWidth = _lineWidth;
            _lineRenderer.endWidth = _lineWidth;
            _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            _lineRenderer.startColor = _lineColor;
            _lineRenderer.endColor = _lineColor;
            _lineRenderer.sortingOrder = 10;
            _lineRenderer.positionCount = 0;
        }
    }

    public void Initialize(Match3Grid grid, int minMatchSize = 3)
    {
        _grid = grid ?? throw new ArgumentNullException(nameof(grid));
        _minMatchSize = minMatchSize;
        _inputLocked = false;
        ClearSelection();
    }

    public void LockInput()
    {
        _inputLocked = true;
        ClearSelection();
    }

    public void UnlockInput()
    {
        _inputLocked = false;
    }

    private void Update()
    {
        if (_inputLocked || _grid == null) return;

        if (Input.GetMouseButtonDown(0))
            OnMouseDown();
        else if (Input.GetMouseButton(0) && _isDragging)
            OnMouseDrag();
        else if (Input.GetMouseButtonUp(0))
            OnMouseUp();
    }

    private void OnMouseDown()
    {
        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0;

        Vector2Int gridPos = _grid.WorldToGrid(worldPos);
        Match3Tile tile = _grid.GetTile(gridPos);

        if (tile != null && tile.IsMatchable)
        {
            _selectedColor = tile.Color;
            AddTileToSelection(tile);
            _isDragging = true;
        }
    }

    private void OnMouseDrag()
    {
        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0;

        Vector2Int gridPos = _grid.WorldToGrid(worldPos);
        Match3Tile tile = _grid.GetTile(gridPos);

        if (tile == null || !tile.IsMatchable) return;

        // Check if this is a new adjacent tile of the same color
        if (tile.Color == _selectedColor && !_selectedTiles.Contains(tile))
        {
            // Must be adjacent to last selected tile
            if (_selectedTiles.Count > 0)
            {
                Match3Tile lastTile = _selectedTiles[^1];
                if (IsAdjacent(lastTile.GridPosition, tile.GridPosition))
                {
                    AddTileToSelection(tile);
                }
            }
        }
        // Check if going back to previous tile (undo selection)
        else if (_selectedTiles.Count > 1 && tile == _selectedTiles[^2])
        {
            RemoveLastTileFromSelection();
        }
    }

    private void OnMouseUp()
    {
        _isDragging = false;

        if (_selectedTiles.Count >= _minMatchSize)
        {
            // Valid match - confirm it
            var positions = new List<Vector2Int>();
            foreach (Match3Tile tile in _selectedTiles)
                positions.Add(tile.GridPosition);

            OnMatchConfirmed?.Invoke(positions);
        }
        else
        {
            // Not enough tiles selected
            OnSelectionCleared?.Invoke();
        }

        ClearSelection();
    }

    private void AddTileToSelection(Match3Tile tile)
    {
        if (tile == null || _selectedTiles.Contains(tile)) return;

        _selectedTiles.Add(tile);
        tile.Select();

        OnTileSelected?.Invoke(tile);

        UpdateLineRenderer();
    }

    private void RemoveLastTileFromSelection()
    {
        if (_selectedTiles.Count == 0) return;

        Match3Tile tile = _selectedTiles[^1];
        _selectedTiles.RemoveAt(_selectedTiles.Count - 1);
        tile.Deselect();

        UpdateLineRenderer();
    }

    public void ClearSelection()
    {
        foreach (Match3Tile tile in _selectedTiles)
        {
            if (tile != null)
                tile.Deselect();
        }

        _selectedTiles.Clear();
        _selectedColor = Match3Tile.ColorType.None;
        _isDragging = false;

        if (_lineRenderer != null)
            _lineRenderer.positionCount = 0;
    }

    private void UpdateLineRenderer()
    {
        if (_lineRenderer == null || _selectedTiles.Count == 0)
        {
            if (_lineRenderer != null)
                _lineRenderer.positionCount = 0;
            return;
        }

        _lineRenderer.positionCount = _selectedTiles.Count;

        for (var i = 0; i < _selectedTiles.Count; i++)
        {
            Match3Tile tile = _selectedTiles[i];
            Vector3 pos = tile.transform.position;
            pos.z = -1; // Draw in front
            _lineRenderer.SetPosition(i, pos);
        }
    }

    private bool IsAdjacent(Vector2Int pos1, Vector2Int pos2)
    {
        int dx = Mathf.Abs(pos1.x - pos2.x);
        int dy = Mathf.Abs(pos1.y - pos2.y);

        // Adjacent if exactly one step away in cardinal directions
        return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
    }

    public void SetLineColor(Color color)
    {
        _lineColor = color;
        if (_lineRenderer != null)
        {
            _lineRenderer.startColor = color;
            _lineRenderer.endColor = color;
        }
    }

    private void OnDisable()
    {
        ClearSelection();
    }
}
