using System.Diagnostics.CodeAnalysis;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class SingletonBehavior<T> : MonoBehaviour where T : SingletonBehavior<T>
{
    bool _initialized;
    readonly object _initializedLock = new();

    [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Static instance required for singleton pattern")]
    static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance != null) return _instance;
            _instance = FindFirstObjectByType<T>();
            _instance?.Init();
            return _instance;
        }
    }

    void Init()
    {
        lock (_initializedLock)
        {
            if (_initialized) return;
            _initialized = true;
            InitInternal();
        }
    }
    
    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _ = Instance;
            //DontDestroyOnLoad(gameObject); // persist across scenes
        }
        else if (_instance != this)
        {
            Destroy(gameObject); // destroy dupes
        }
    }
    
    // ensure static ref cleared if object destroyed
    protected virtual void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    // Must be implemented in subclass
    protected abstract void InitInternal();
}