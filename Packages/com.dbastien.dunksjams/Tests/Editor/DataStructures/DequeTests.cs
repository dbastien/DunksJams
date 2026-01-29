using NUnit.Framework;

public class DequeTests : TestBase
{
    [Test] public void AddFront_AddBack_Order()
    {
        var dq = new Deque<int>();
        dq.AddBack(2);
        dq.AddFront(1);
        dq.AddBack(3);
        Eq(1, dq.PeekFront());
        Eq(3, dq.PeekBack());
    }

    [Test] public void RemoveFront_RemoveBack()
    {
        var dq = new Deque<int>();
        dq.AddBack(1); dq.AddBack(2); dq.AddBack(3);
        Eq(1, dq.RemoveFront());
        Eq(3, dq.RemoveBack());
        Eq(1, dq.Count);
        Eq(2, dq.PeekFront());
    }

    [Test] public void Count_Accurate()
    {
        var dq = new Deque<int>();
        Eq(0, dq.Count);
        dq.AddBack(1);
        Eq(1, dq.Count);
        dq.AddFront(2);
        Eq(2, dq.Count);
    }

    [Test] public void Clear_EmptiesDeque()
    {
        var dq = new Deque<int>();
        dq.AddBack(1); dq.AddBack(2);
        dq.Clear();
        Eq(0, dq.Count);
    }

    [Test] public void Contains_FindsItem()
    {
        var dq = new Deque<int>();
        dq.AddBack(1); dq.AddBack(2);
        True(dq.Contains(1));
        False(dq.Contains(99));
    }
}
