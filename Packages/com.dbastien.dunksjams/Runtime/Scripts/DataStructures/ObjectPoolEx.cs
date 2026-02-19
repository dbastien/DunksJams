using System;
using UnityEngine.Pool;

public interface IPoolable
{
    void OnPoolGet();
    void OnPoolRelease();
}

public sealed class ObjectPoolEx<T> where T : class
{
    private readonly ObjectPool<T> pool;

    public ObjectPoolEx(
        Func<T> createFunc,
        int initialCap = 8,
        int maxCap = 128,
        bool callDisposeOnDestroy = true)
    {
        if (createFunc == null) throw new ArgumentNullException(nameof(createFunc));
        if (initialCap < 0) throw new ArgumentOutOfRangeException(nameof(initialCap));
        if (maxCap <= 0) throw new ArgumentOutOfRangeException(nameof(maxCap));

        var hasPoolable = typeof(IPoolable).IsAssignableFrom(typeof(T));
        var hasDisposable = callDisposeOnDestroy && typeof(IDisposable).IsAssignableFrom(typeof(T));

        Action<T> onGet = hasPoolable ? static obj => ((IPoolable)obj).OnPoolGet() : null;
        Action<T> onRelease = hasPoolable ? static obj => ((IPoolable)obj).OnPoolRelease() : null;
        Action<T> onDestroy = hasDisposable ? static obj => ((IDisposable)obj).Dispose() : null;

        pool = new ObjectPool<T>(
            createFunc,
            onGet,
            onRelease,
            onDestroy,
            collectionCheck: false,
            defaultCapacity: initialCap,
            maxSize: maxCap
        );
    }

    public ObjectPoolEx(int initialCap = 8, int maxCap = 128)
        : this(static () => Activator.CreateInstance<T>(), initialCap, maxCap) { }

    public int CountInactive => pool.CountInactive;

    public T Get() => pool.Get();
    public PooledObject<T> Get(out T value) => pool.Get(out value);

    public void Release(T obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        pool.Release(obj);
    }

    public void Clear() => pool.Clear();
}