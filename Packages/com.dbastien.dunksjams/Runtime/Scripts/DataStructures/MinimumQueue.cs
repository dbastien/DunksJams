using System;

public class MinimumQueue<T>
{
    private const int MinimumGrow = 4;
    private const double GrowFactor = 2.0;
    private T[] _array;
    private int _head, _tail, _size;

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
        T item = _array[_head];
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

    private void GrowIfNeeded()
    {
        if (_size < _array.Length) return;
        int newCapacity = Math.Max(_array.Length + MinimumGrow, (int)(_array.Length * GrowFactor));
        SetCapacity(newCapacity);
    }

    private void SetCapacity(int capacity)
    {
        var newArray = new T[capacity];
        if (_size > 0)
        {
            if (_head < _tail) { Array.Copy(_array, _head, newArray, 0, _size); }
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

    private void ThrowIfEmpty()
    {
        if (_size == 0) throw new InvalidOperationException("Queue is empty.");
    }
}