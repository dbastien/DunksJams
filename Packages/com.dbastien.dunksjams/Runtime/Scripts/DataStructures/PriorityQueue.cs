using System;
using System.Collections;
using System.Collections.Generic;

public class PriorityQueue<T> : IEnumerable<T> where T : IComparable<T>
{
    private readonly List<T> _heap = new();

    public int Count => _heap.Count;
    public void Clear() => _heap.Clear();

    public void Enqueue(T item)
    {
        _heap.Add(item);
        HeapifyUp(_heap.Count - 1);
    }

    public T Dequeue()
    {
        ThrowIfEmpty();
        T root = _heap[0];
        _heap[0] = _heap[^1];
        _heap.RemoveAt(_heap.Count - 1);
        HeapifyDown(0);
        return root;
    }

    public bool TryDequeue(out T result)
    {
        if (_heap.Count == 0)
        {
            result = default!;
            return false;
        }

        result = Dequeue();
        return true;
    }

    public T Peek()
    {
        ThrowIfEmpty();
        return _heap[0];
    }

    public bool Contains(T item) => _heap.Contains(item);

    private void HeapifyUp(int i)
    {
        T item = _heap[i];
        int parent;
        while (i > 0 && item.CompareTo(_heap[parent = (i - 1) / 2]) < 0)
        {
            _heap[i] = _heap[parent];
            i = parent;
        }

        _heap[i] = item;
    }

    private void HeapifyDown(int i)
    {
        T item = _heap[i];
        int child;
        while ((child = 2 * i + 1) < _heap.Count)
        {
            if (child + 1 < _heap.Count && _heap[child + 1].CompareTo(_heap[child]) < 0) child++;
            if (item.CompareTo(_heap[child]) <= 0) break;
            _heap[i] = _heap[child];
            i = child;
        }

        _heap[i] = item;
    }

    private void ThrowIfEmpty()
    {
        if (_heap.Count == 0) throw new InvalidOperationException("Queue is empty.");
    }

    public IEnumerator<T> GetEnumerator() => _heap.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}