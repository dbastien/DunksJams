using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum SceneTransitionState : byte { Idle, FadingOut, Loading, WaitingForInput, FadingIn }

[Serializable]
public class SceneTransitionSettings
{
    [Range(0f, 3f)] public float FadeOutDuration = 0.5f;
    [Range(0f, 3f)] public float FadeInDuration = 0.5f;
    [Range(0f, 10f)] public float MinLoadDuration;
    public bool WaitForInput;
    public bool FadeAudio = true;
    public Color OverlayColor = Color.black;
}

public class SceneTransitionStartEvent : GameEvent { public string SceneName; }
public class SceneTransitionCompleteEvent : GameEvent { public string SceneName; }

/// <summary>
/// Manages scene transitions with fade overlay, progress bar, optional audio fade, and event hooks.
/// Use <c>SceneTransitionManager.Instance.LoadScene("MyScene")</c> to trigger a transition.
/// Listen to <see cref="SceneTransitionStartEvent"/> / <see cref="SceneTransitionCompleteEvent"/>
/// or poll <see cref="DisplayProgress"/> for custom UI.
/// </summary>
[SingletonAutoCreate]
[DisallowMultipleComponent]
public class SceneTransitionManager : SingletonEagerBehaviour<SceneTransitionManager>
{
    [SerializeField] SceneTransitionSettings _defaultSettings = new();

    SceneTransitionState _state = SceneTransitionState.Idle;
    CanvasGroup _overlay;
    Image _overlayImage;
    RectTransform _fillRT;
    GameObject _barRoot;
    float _displayProgress;
    Coroutine _activeTransition;

    public SceneTransitionState State => _state;
    public float DisplayProgress => _displayProgress;
    public bool IsTransitioning => _state != SceneTransitionState.Idle;

    protected override bool PersistAcrossScenes => true;
    protected override void InitInternal() => BuildUI();

    // ========================================================================
    // Public API
    // ========================================================================

    public void LoadScene(string sceneName, SceneTransitionSettings settings = null) =>
        BeginTransition(sceneName, LoadSceneMode.Single, settings);

    public void LoadSceneAdditive(string sceneName, SceneTransitionSettings settings = null) =>
        BeginTransition(sceneName, LoadSceneMode.Additive, settings);

    public static void UnloadScene(string sceneName) =>
        SceneManager.UnloadSceneAsync(sceneName);

    // ========================================================================
    // Transition logic
    // ========================================================================

    void BeginTransition(string sceneName, LoadSceneMode mode, SceneTransitionSettings settings)
    {
        if (_state != SceneTransitionState.Idle)
        {
            DLog.LogW($"Scene transition already in progress, ignoring '{sceneName}'");
            return;
        }

        settings ??= _defaultSettings;
        _overlayImage.color = settings.OverlayColor;

        if (_activeTransition != null) StopCoroutine(_activeTransition);
        _activeTransition = StartCoroutine(RunTransition(sceneName, mode, settings));
    }

    IEnumerator RunTransition(string sceneName, LoadSceneMode mode, SceneTransitionSettings settings)
    {
        _displayProgress = 0f;
        _state = SceneTransitionState.FadingOut;
        EventManager.QueueEvent<SceneTransitionStartEvent>(e => e.SceneName = sceneName, true);

        if (settings.FadeAudio)
            AudioSystem.Instance?.StopMusic(fadeOut: true);

        yield return Fade(0f, 1f, settings.FadeOutDuration);
        SetBarVisible(true);

        // async load — Unity reports 0→0.9 while loading, then stalls until activation
        _state = SceneTransitionState.Loading;
        float loadStart = Time.unscaledTime;
        var op = SceneManager.LoadSceneAsync(sceneName, mode);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            _displayProgress = Mathf.MoveTowards(
                _displayProgress, op.progress / 0.9f, Time.unscaledDeltaTime * 2f
            );
            UpdateBar();
            yield return null;
        }

        // honor minimum loading duration (useful for fast-loading scenes)
        float elapsed = Time.unscaledTime - loadStart;
        if (elapsed < settings.MinLoadDuration)
        {
            float remaining = settings.MinLoadDuration - elapsed;
            float from = _displayProgress;
            for (float t = 0f; t < remaining; t += Time.unscaledDeltaTime)
            {
                _displayProgress = Mathf.Lerp(from, 1f, t / remaining);
                UpdateBar();
                yield return null;
            }
        }

        _displayProgress = 1f;
        UpdateBar();

        // optional "press any key" gate
        if (settings.WaitForInput)
        {
            _state = SceneTransitionState.WaitingForInput;
            yield return null; // skip frame so held keys don't pass through
            while (!Input.anyKeyDown) yield return null;
        }

        // activate & wait
        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;

        SetBarVisible(false);

        // fade back in
        _state = SceneTransitionState.FadingIn;
        yield return Fade(1f, 0f, settings.FadeInDuration);

        _state = SceneTransitionState.Idle;
        _activeTransition = null;
        EventManager.QueueEvent<SceneTransitionCompleteEvent>(e => e.SceneName = sceneName, true);
    }

    // ========================================================================
    // Fade helpers (uses unscaledDeltaTime so it works at timeScale 0)
    // ========================================================================

    IEnumerator Fade(float from, float to, float duration)
    {
        _overlay.blocksRaycasts = true;

        if (duration <= 0f)
        {
            _overlay.alpha = to;
        }
        else
        {
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                _overlay.alpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }
            _overlay.alpha = to;
        }

        _overlay.blocksRaycasts = to > 0.01f;
    }

    void UpdateBar()
    {
        if (_fillRT != null)
            _fillRT.anchorMax = new Vector2(_displayProgress, 1f);
    }

    void SetBarVisible(bool visible) => _barRoot?.SetActive(visible);

    // ========================================================================
    // UI construction (programmatic — no prefab required)
    // ========================================================================

    void BuildUI()
    {
        // canvas — highest sort order so it renders on top of everything
        var canvasGo = new GameObject("TransitionCanvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);

        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // full-screen overlay image
        var overlayGo = new GameObject("Overlay", typeof(Image), typeof(CanvasGroup));
        overlayGo.transform.SetParent(canvasGo.transform, false);
        StretchFull(overlayGo.GetComponent<RectTransform>());

        _overlayImage = overlayGo.GetComponent<Image>();
        _overlayImage.color = Color.black;

        _overlay = overlayGo.GetComponent<CanvasGroup>();
        _overlay.alpha = 0f;
        _overlay.blocksRaycasts = false;
        _overlay.interactable = false;

        // progress bar background — thin bar near the bottom of screen
        _barRoot = new GameObject("ProgressBar", typeof(Image));
        _barRoot.transform.SetParent(overlayGo.transform, false);
        var barRT = _barRoot.GetComponent<RectTransform>();
        barRT.anchorMin = new Vector2(0.15f, 0.08f);
        barRT.anchorMax = new Vector2(0.85f, 0.1f);
        barRT.offsetMin = barRT.offsetMax = Vector2.zero;
        _barRoot.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.15f);

        // progress bar fill — anchored left, width driven by _displayProgress
        var fillGo = new GameObject("Fill", typeof(Image));
        fillGo.transform.SetParent(_barRoot.transform, false);
        _fillRT = fillGo.GetComponent<RectTransform>();
        _fillRT.anchorMin = Vector2.zero;
        _fillRT.anchorMax = new Vector2(0f, 1f);
        _fillRT.offsetMin = _fillRT.offsetMax = Vector2.zero;
        fillGo.GetComponent<Image>().color = Color.white;

        SetBarVisible(false);
    }

    static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    // ========================================================================
    // Cleanup
    // ========================================================================

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (_activeTransition != null) StopCoroutine(_activeTransition);
        _state = SceneTransitionState.Idle;
    }
}
