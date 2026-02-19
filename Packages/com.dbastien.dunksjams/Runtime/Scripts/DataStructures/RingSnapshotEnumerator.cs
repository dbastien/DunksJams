using System;
using System.Collections;
using System.Collections.Generic;

public struct RingSnapshotEnumerator<T> : IEnumerator<T>
{
    private readonly T[] buffer;
    private readonly int head;
    private readonly int size;
    private readonly int capacity;

    private int index;
    private T current;

    public T Current => current;
    object IEnumerator.Current => current;

    public RingSnapshotEnumerator(T[] buffer, int head, int size)
    {
        this.buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        this.head = head;
        this.size = size;
        capacity = buffer.Length;

        index = -1;
        current = default;
    }

    public bool MoveNext()
    {
        int next = index + 1;
        if (next >= size)
        {
            current = default;
            return false;
        }

        index = next;
        current = buffer[(head + next) % capacity];
        return true;
    }

    public void Reset()
    {
        index = -1;
        current = default;
    }

    public void Dispose() { }
}