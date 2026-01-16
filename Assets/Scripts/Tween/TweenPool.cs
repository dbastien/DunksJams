using System.Collections.Generic;

public static class TweenPool<T> where T : TweenCore, new()
{
    static readonly Stack<T> _pool = new();

    public static T Rent() => _pool.Count > 0 ? _pool.Pop() : new T();

    public static void Return(T tween) => _pool.Push(tween);
}

public abstract class TweenCore
{
    public abstract void Reset();
}