using System;
using UnityEngine.Pool;

public interface IPoolable
{
    void OnPoolGet();
    void OnPoolRelease();
}

public class ObjectPoolEx<T> where T : class, new()
{
    readonly ObjectPool<T> _pool;

    public ObjectPoolEx(int initialCap = 8, int maxCap = 128)
    {
        _pool = new ObjectPool<T>(
            () => new T(),
            obj => (obj as IPoolable)?.OnPoolGet(),
            obj => (obj as IPoolable)?.OnPoolRelease(),
            obj => (obj as IDisposable)?.Dispose(),
            defaultCapacity: initialCap,
            maxSize: maxCap
        );
    }

    public T Get() => _pool.Get();
    public void Release(T obj) => _pool.Release(obj);
}