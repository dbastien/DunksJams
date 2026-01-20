using System.Collections.Generic;
using UnityEngine;

public class SpatialHash2D<T>
{
    readonly float _cellSize;
    readonly Dictionary<Vector2Int, List<(T obj, Vector2 pos)>> _cells = new();
    int _totalObjectCount = 0;
    
    public int CellCount => _cells.Count;
    public int Count => _totalObjectCount;

    public SpatialHash2D(float cellSize) => 
        _cellSize = cellSize > 0 ? cellSize : throw new System.ArgumentOutOfRangeException(nameof(cellSize));

    Vector2Int GetCell(Vector2 pos) =>
        new(Mathf.FloorToInt(pos.x / _cellSize), Mathf.FloorToInt(pos.y / _cellSize));

    public void Insert(Vector2 pos, T obj)
    {
        var cell = GetCell(pos);
        if (!_cells.TryGetValue(cell, out var objects))
        {
            objects = new List<(T, Vector2)>();
            _cells[cell] = objects;
        }
        objects.Add((obj, pos));
        ++_totalObjectCount;
    }

    public bool Remove(Vector2 pos, T obj)
    {
        var cell = GetCell(pos);
        if (_cells.TryGetValue(cell, out var objects) &&
            objects.RemoveAll(o => EqualityComparer<T>.Default.Equals(o.obj, obj)) > 0)
        {
            --_totalObjectCount;
            if (objects.Count == 0) _cells.Remove(cell);
            return true;
        }
        return false;
    }

    public void UpdatePosition(Vector2 oldPos, Vector2 newPos, T obj)
    {
        if (Remove(oldPos, obj))
            Insert(newPos, obj);
    }

    public List<T> Query(Vector2 pos, List<T> result = null)
    {
        result ??= new();
        result.Clear();

        if (_cells.TryGetValue(GetCell(pos), out var objects))
            ExtractObjects(objects, result);

        return result;
    }

    public List<T> QueryNeighbors(Vector2 pos, int maxDistInCells, List<T> result = null)
    {
        result ??= new();
        result.Clear();

        var centerCell = GetCell(pos);
        for (int x = -maxDistInCells; x <= maxDistInCells; ++x)
        {
            for (int y = -maxDistInCells; y <= maxDistInCells; ++y)
            {
                var neighborCell = new Vector2Int(centerCell.x + x, centerCell.y + y);
                if (_cells.TryGetValue(neighborCell, out var objects))
                    ExtractObjects(objects, result);
            }
        }
        return result;
    }

    public List<T> QueryInRadius(Vector2 pos, float radius, List<T> result = null)
    {
        result ??= new();
        result.Clear();

        var radiusSquared = radius * radius;
        var minCell = GetCell(pos - new Vector2(radius, radius));
        var maxCell = GetCell(pos + new Vector2(radius, radius));

        for (int y = minCell.y; y <= maxCell.y; ++y)
        {
            for (int x = minCell.x; x <= maxCell.x; ++x)
            {
                var cell = new Vector2Int(x, y);
                if (_cells.TryGetValue(cell, out var objects))
                {
                    foreach (var (obj, objPos) in objects)
                    {
                        if ((objPos - pos).sqrMagnitude <= radiusSquared)
                            result.Add(obj);
                    }
                }
            }
        }
        return result;
    }

    public void Clear()
    {
        _cells.Clear();
        _totalObjectCount = 0;
    }
    
    public IEnumerable<T> AllObjects()
    {
        foreach (var objects in _cells.Values)
            foreach ((T obj, _) in objects)
                yield return obj;
    }

    void ExtractObjects(List<(T obj, Vector2 pos)> objects, List<T> result)
    {
        foreach ((T obj, _) in objects) result.Add(obj);
    }
}
