using System;
using System.Collections.Generic;
using UnityEngine;

public class SpatialHash3D<T>
{
    readonly float _cellSize;
    readonly Dictionary<Vector3Int, List<(T obj, Vector3 pos)>> _cells = new();
    int _totalObjectCount;
    
    public int CellCount => _cells.Count;
    public int Count => _totalObjectCount;
    
    public SpatialHash3D(float cellSize) =>
        _cellSize = cellSize > 0 ? cellSize : throw new ArgumentOutOfRangeException(nameof(cellSize));

    Vector3Int GetCell(Vector3 pos) =>
        new(
            Mathf.FloorToInt(pos.x / _cellSize),
            Mathf.FloorToInt(pos.y / _cellSize),
            Mathf.FloorToInt(pos.z / _cellSize)
        );

    public void Insert(Vector3 pos, T obj)
    {
        var cell = GetCell(pos);
        if (!_cells.TryGetValue(cell, out var objects))
        {
            objects = new List<(T, Vector3)>();
            _cells[cell] = objects;
        }
        objects.Add((obj, pos));
        ++_totalObjectCount;
    }

    public bool Remove(Vector3 pos, T obj)
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

    public void UpdatePosition(Vector3 oldPos, Vector3 newPos, T obj)
    {
        if (Remove(oldPos, obj))
            Insert(newPos, obj);
    }

    public List<T> Query(Vector3 pos, List<T> result = null)
    {
        result ??= new();
        result.Clear();

        if (_cells.TryGetValue(GetCell(pos), out var objects))
            ExtractObjects(objects, result);

        return result;
    }

    public List<T> QueryNeighbors(Vector3 pos, int maxDistInCells, List<T> result = null)
    {
        result ??= new();
        result.Clear();

        var centerCell = GetCell(pos);
        for (int x = -maxDistInCells; x <= maxDistInCells; ++x)
        {
            for (int y = -maxDistInCells; y <= maxDistInCells; ++y)
            {
                for (int z = -maxDistInCells; z <= maxDistInCells; ++z)
                {
                    var neighborCell = new Vector3Int(centerCell.x + x, centerCell.y + y, centerCell.z + z);
                    if (_cells.TryGetValue(neighborCell, out var objects))
                        ExtractObjects(objects, result);
                }
            }
        }
        return result;
    }

    public List<T> QueryInRadius(Vector3 pos, float radius, List<T> result = null)
    {
        result ??= new();
        result.Clear();

        var radiusSquared = radius * radius;
        var minCell = GetCell(pos - new Vector3(radius, radius, radius));
        var maxCell = GetCell(pos + new Vector3(radius, radius, radius));

        for (int z = minCell.z; z <= maxCell.z; ++z)
        {
            for (int y = minCell.y; y <= maxCell.y; ++y)
            {
                for (int x = minCell.x; x <= maxCell.x; ++x)
                {
                    var cell = new Vector3Int(x, y, z);
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

    void ExtractObjects(List<(T obj, Vector3 pos)> objects, List<T> result)
    {
        foreach ((T obj, _) in objects) result.Add(obj);
    }
}
