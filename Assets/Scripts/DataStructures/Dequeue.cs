using System.Collections.Generic;
using UnityEngine;

public class Deque<T> : ICollection<T>
{
    readonly LinkedList<T> _list = new();
    
    public int Count => _list.Count;
    public bool IsReadOnly => false;

    public void Add(T item) => AddBack(item);
    public void Clear() => _list.Clear();
    public bool Contains(T item) => _list.Contains(item);
    public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);
    public void AddFront(T item) => _list.AddFirst(item);
    public void AddBack(T item) => _list.AddLast(item);

    public T RemoveFront() => Remove(_list.First);
    public T RemoveBack() => Remove(_list.Last);
    
    public bool Remove(T item)
    {
        if (_list.First?.Value?.Equals(item) == true) { RemoveFront(); return true; }
        if (_list.Last?.Value?.Equals(item) == true) { RemoveBack(); return true; }
        return false;
    }

    T Remove(LinkedListNode<T> node)
    {
        Debug.Assert(node != null);
        _list.Remove(node);
        return node.Value;
    }

    public T PeekFront()
    {
        Debug.Assert(_list.First != null);
        return _list.First.Value;
    }

    public T PeekBack()
    {
        Debug.Assert(_list.Last != null);
        return _list.Last.Value;
    }

    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}