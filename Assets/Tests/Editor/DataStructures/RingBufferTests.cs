using NUnit.Framework;

public class RingBufferTests : TestBase
{
    [Test] public void Add_IncreasesCount()
    {
        var rb = new RingBuffer<int>(5);
        rb.Add(1);
        rb.Add(2);
        Eq(2, rb.Count);
    }

    [Test] public void Add_WrapsAtCapacity()
    {
        var rb = new RingBuffer<int>(3);
        rb.Add(1); rb.Add(2); rb.Add(3); rb.Add(4);
        Eq(3, rb.Count);
        Eq(2, rb.First()); // 1 was overwritten
        Eq(4, rb.Last());
    }

    [Test] public void Indexer_ReturnsCorrect()
    {
        var rb = new RingBuffer<int>(5);
        rb.Add(10); rb.Add(20); rb.Add(30);
        Eq(10, rb[0]);
        Eq(20, rb[1]);
        Eq(30, rb[2]);
    }

    [Test] public void Clear_ResetsCount()
    {
        var rb = new RingBuffer<int>(5);
        rb.Add(1); rb.Add(2);
        rb.Clear();
        Eq(0, rb.Count);
    }

    [Test] public void Skip_RemovesFromFront()
    {
        var rb = new RingBuffer<int>(5);
        rb.Add(1); rb.Add(2); rb.Add(3);
        rb.Skip(2);
        Eq(1, rb.Count);
        Eq(3, rb.First());
    }

    [Test] public void GetRecent_ReturnsFromEnd()
    {
        var rb = new RingBuffer<int>(5);
        rb.Add(1); rb.Add(2); rb.Add(3);
        Eq(3, rb.GetRecent(0));
        Eq(2, rb.GetRecent(1));
        Eq(1, rb.GetRecent(2));
    }
}
