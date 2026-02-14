using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AsyncUtils
{
    static readonly WaitForEndOfFrame _endOfFrame = new();
    static readonly WaitForFixedUpdate _fixedUpdate = new();

    public static IEnumerator Delay(float seconds, CancellationToken token)
    {
        float elapsed = 0;
        while (elapsed < seconds)
        {
            if (token.IsCancellationRequested) yield break;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    public static IEnumerator NextFrame(CancellationToken token)
    {
        if (token.IsCancellationRequested) yield break;
        yield return _endOfFrame;
    }

    public static IEnumerator WaitForFixedUpdate(CancellationToken token)
    {
        if (token.IsCancellationRequested) yield break;
        yield return _fixedUpdate;
    }

    public static IEnumerator LoadSceneAsync(string sceneName, CancellationToken token) =>
        LoadSceneAsync(sceneName, LoadSceneMode.Single, token);

    public static IEnumerator LoadSceneAsync(string sceneName, LoadSceneMode mode, CancellationToken token)
    {
        var operation = SceneManager.LoadSceneAsync(sceneName, mode);
        while (!operation.isDone)
        {
            if (token.IsCancellationRequested) yield break;
            yield return null;
        }
    }

    public static void RunOnMainThread(Action action)
    {
        if (action == null) return;
        AsyncRunner.Instance.StartCoroutine(RunCoroutine(action));
    }

    static IEnumerator RunCoroutine(Action action)
    {
        action();
        yield break;
    }
}

[SingletonAutoCreate]
[DisallowMultipleComponent]
public class AsyncRunner : SingletonEagerBehaviour<AsyncRunner>
{
    protected override bool PersistAcrossScenes => true;

    protected override void InitInternal()
    {
    }
}