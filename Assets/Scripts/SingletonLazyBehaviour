using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>Lazy: selects the canonical instance in Awake, but runs InitInternal() only on first Instance access.</summary>
[DisallowMultipleComponent]
public abstract class SingletonLazyBehaviour<T> : MonoBehaviour where T : SingletonLazyBehaviour<T>
{
    private static T _instance;
    private static bool _quitting;

    private bool _initialized;

    protected virtual bool PersistAcrossScenes => false;

    /// <summary>Override to auto-create a singleton if none exists when Instance is accessed.</summary>
    protected virtual bool AutoCreateWhenMissing => false;

    public static T Instance
    {
        get
        {
            if (_quitting) return null;

            if (_instance != null)
            {
                _instance.EnsureInit();
                return _instance;
            }

            _instance = FindFirstObjectByType<T>();
            if (_instance != null)
            {
                _instance.EnsureInit();
                return _instance;
            }

            // Same note as eager: Instance getter can't call virtuals safely.
            // Use CreateIfMissing() helper from derived classes if you want auto-create.
            return null;
        }
    }

    protected virtual void Awake()
    {
        // Lazy: do NOT run init here. Only establish canonical instance + handle dupes.
        if (_instance == null)
        {
            _instance = (T)this;

            if (PersistAcrossScenes)
                DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            DestroyDuplicate();
        }
    }

    protected virtual void OnApplicationQuit() => _quitting = true;

    protected virtual void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    // If you want strict support for AutoCreateWhenMissing, call this from derived class:
    protected static T CreateIfMissing()
    {
        if (_instance != null || _quitting) return _instance;

        var go = new GameObject(typeof(T).Name);
        _instance = go.AddComponent<T>();
        _instance.EnsureInit();

        if (_instance.PersistAcrossScenes)
            DontDestroyOnLoad(go);

        return _instance;
    }

    private void EnsureInit()
    {
        if (_initialized) return;
        _initialized = true;
        InitInternal();
    }

    private void DestroyDuplicate()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) DestroyImmediate(gameObject);
        else Destroy(gameObject);
#else
        Destroy(gameObject);
#endif
    }

    protected abstract void InitInternal();

#if UNITY_EDITOR
    [InitializeOnEnterPlayMode]
    private static void OnEnterPlayMode(EnterPlayModeOptions options)
    {
        if ((options & EnterPlayModeOptions.DisableDomainReload) != 0)
        {
            _instance = null;
            _quitting = false;
        }
    }
#endif
}
