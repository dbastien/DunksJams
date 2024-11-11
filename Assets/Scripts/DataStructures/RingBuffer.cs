using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingBuffer<T> : IEnumerable<T>
{
    private readonly T[] _buffer;
    private int _head;
    private int _tail;
    private int _size;

    public RingBuffer(int capacity)
    {
        Debug.Assert(capacity > 0);
        _buffer = new T[capacity];
    }

    public int Capacity => _buffer.Length;
    public int Count => _size;
    
    public T this[int index]
    {
        get
        {
            Debug.Assert(index >= 0 && index < _size);
            return _buffer[(_head + index) % Capacity];
        }
        set
        {
            Debug.Assert(index >= 0 && index < _size);
            _buffer[(_head + index) % Capacity] = value;
        }
    }

    public void Add(T item)
    {
        _buffer[_tail] = item;
        _tail = (_tail + 1) % Capacity;

        if (_size == Capacity)
            _head = (_head + 1) % Capacity;
        else
            ++_size;
    }
    
    public void AddRange(IEnumerable<T> items)
    {
        foreach (var item in items) Add(item);
    }

    public T First()
    {
        Debug.Assert(_size != 0);
        return _buffer[_head];
    }

    public T Last()
    {
        Debug.Assert(_size != 0);
        return _buffer[(_tail - 1 + Capacity) % Capacity];
    }
    
    public void Skip(int count = 1)
    {
        Debug.Assert(count <= _size);
        _head = (_head + count) % Capacity;
        _size -= count;
    }

    public void Clear(bool clearData = false)
    {
        if (clearData) Array.Clear(_buffer, 0, Capacity);
        _head = 0;
        _tail = 0;
        _size = 0;
    }
    
    public T GetRecent(int index)
    {
        Debug.Assert(index >= 0 && index < _size);
        return _buffer[(_head + _size - 1 - index) % Capacity];
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < _size; ++i)
            yield return _buffer[(_head + i) % Capacity];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}