using UnityEngine.Pool;
using System;

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
            createFunc: () => new T(),
            actionOnGet: obj => (obj as IPoolable)?.OnPoolGet(),
            actionOnRelease: obj => (obj as IPoolable)?.OnPoolRelease(),
            actionOnDestroy: obj => (obj as IDisposable)?.Dispose(),
            defaultCapacity: initialCap,
            maxSize: maxCap
        );
    }

    public T Get() => _pool.Get();
    public void Release(T obj) => _pool.Release(obj);
}