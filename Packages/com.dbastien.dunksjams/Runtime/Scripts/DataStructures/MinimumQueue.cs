using System;

public class MinimumQueue<T>
{
    const int MinimumGrow = 4;
    const double GrowFactor = 2.0;
    T[] _array;
    int _head, _tail, _size;

    public MinimumQueue(int capacity) =>
        _array = capacity >= 0 ? new T[capacity] : throw new ArgumentOutOfRangeException(nameof(capacity));

    public int Count => _size;

    public void Enqueue(T item)
    {
        GrowIfNeeded();
        _array[_tail] = item;
        _tail = (_tail + 1) % _array.Length;
        ++_size;
    }

    public T Dequeue()
    {
        ThrowIfEmpty();
        var item = _array[_head];
        _array[_head] = default!;
        _head = (_head + 1) % _array.Length;
        --_size;
        return item;
    }

    public T Peek()
    {
        ThrowIfEmpty();
        return _array[_head];
    }

    void GrowIfNeeded()
    {
        if (_size < _array.Length) return;
        var newCapacity = Math.Max(_array.Length + MinimumGrow, (int)(_array.Length * GrowFactor));
        SetCapacity(newCapacity);
    }

    void SetCapacity(int capacity)
    {
        var newArray = new T[capacity];
        if (_size > 0)
        {
            if (_head < _tail)
            {
                Array.Copy(_array, _head, newArray, 0, _size);
            }
            else
            {
                Array.Copy(_array, _head, newArray, 0, _array.Length - _head);
                Array.Copy(_array, 0, newArray, _array.Length - _head, _tail);
            }
        }

        _array = newArray;
        _head = 0;
        _tail = _size;
    }

    void ThrowIfEmpty()
    {
        if (_size == 0) throw new InvalidOperationException("Queue is empty.");
    }
}