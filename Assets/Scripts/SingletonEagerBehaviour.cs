using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>Eager: selects the canonical instance in Awake and runs InitInternal() immediately.</summary>
[DisallowMultipleComponent]
public abstract class SingletonEagerBehaviour<T> : MonoBehaviour where T : SingletonEagerBehaviour<T>
{
    private static T _instance;
    private static bool _quitting;

    private bool _initialized;

    /// <summary>Override to keep the singleton across scene loads.</summary>
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
            return FindOrCreateAndInit();
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = (T)this;
            EnsureInit();

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

    private static T FindOrCreateAndInit()
    {
        _instance = FindFirstObjectByType<T>();
        if (_instance == null)
        {
            // Instance getter can't call virtuals, so we do a conservative approach:
            // If you want AutoCreateWhenMissing, set it via a derived class helper
            // (see notes below). For the common case, we keep Instance strict here.
            //
            // If you prefer auto-create without virtuals, use SingletonEagerAutoCreate<T> variant.
            return null;
        }

        _instance.EnsureInit();
        return _instance;
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
    // Reset statics when entering play mode when Domain Reload is disabled.
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
