/// <summary>Lazy: selects the canonical instance in Awake, but runs InitInternal() only on first Instance access.</summary>
public abstract class SingletonLazyBehaviour<T> : SingletonBehaviour<T> where T : SingletonLazyBehaviour<T>
{
    protected sealed override bool InitOnAwake => false;
}