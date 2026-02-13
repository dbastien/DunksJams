using System;
using NUnit.Framework;

public class PriorityQueueTests : TestBase
{
    [Test]
    public void Dequeue_ReturnsMinFirst()
    {
        var pq = new PriorityQueue<int>();
        pq.Enqueue(5);
        pq.Enqueue(1);
        pq.Enqueue(3);
        Eq(1, pq.Dequeue());
        Eq(3, pq.Dequeue());
        Eq(5, pq.Dequeue());
    }

    [Test]
    public void Peek_ReturnsMinWithoutRemove()
    {
        var pq = new PriorityQueue<int>();
        pq.Enqueue(5);
        pq.Enqueue(1);
        Eq(1, pq.Peek());
        Eq(2, pq.Count);
    }

    [Test]
    public void Empty_ThrowsOnDequeue()
    {
        var pq = new PriorityQueue<int>();
        Throws<InvalidOperationException>(() => pq.Dequeue());
    }

    [Test]
    public void Count_Accurate()
    {
        var pq = new PriorityQueue<int>();
        Eq(0, pq.Count);
        pq.Enqueue(1);
        Eq(1, pq.Count);
        pq.Dequeue();
        Eq(0, pq.Count);
    }

    [Test]
    public void TryDequeue_ReturnsFalseWhenEmpty()
    {
        var pq = new PriorityQueue<int>();
        False(pq.TryDequeue(out _));
    }

    [Test]
    public void Contains_FindsItem()
    {
        var pq = new PriorityQueue<int>();
        pq.Enqueue(5);
        pq.Enqueue(10);
        True(pq.Contains(5));
        False(pq.Contains(99));
    }
}