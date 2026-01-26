using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class SingletonAutoCreateAttribute : Attribute
{
}

[DisallowMultipleComponent]
public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
{
    static T _instance;
    static bool _quitting;
    static readonly bool _autoCreate = Attribute.IsDefined(typeof(T), typeof(SingletonAutoCreateAttribute), true);

    bool _initialized;

    protected virtual bool PersistAcrossScenes => false;
    protected virtual bool InitOnAwake => true;

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

            return _autoCreate ? CreateAndInit() : null;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = (T)this;

            if (InitOnAwake) EnsureInit();

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

    protected static T CreateIfMissing()
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

        return CreateAndInit();
    }

    static T CreateAndInit()
    {
        if (_quitting) return null;

        var go = new GameObject(typeof(T).Name);
        _instance = go.AddComponent<T>();
        _instance.EnsureInit();

        if (_instance.PersistAcrossScenes)
            DontDestroyOnLoad(go);

        return _instance;
    }

    void EnsureInit()
    {
        if (_initialized) return;
        _initialized = true;
        InitInternal();
    }

    void DestroyDuplicate()
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
    static void OnEnterPlayMode(EnterPlayModeOptions options)
    {
        if ((options & EnterPlayModeOptions.DisableDomainReload) != 0)
        {
            _instance = null;
            _quitting = false;
        }
    }
#endif
}

/// <summary>Eager: selects the canonical instance in Awake and runs InitInternal() immediately.</summary>
public abstract class SingletonEagerBehaviour<T> : SingletonBehaviour<T> where T : SingletonEagerBehaviour<T>
{
    protected sealed override bool InitOnAwake => true;
}
