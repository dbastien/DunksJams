using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fade(in|out, duration)
/// </summary>
public class SequencerCommandFade : SequencerCommand
{
    private static Canvas _fadeCanvas;
    private static Image _fadeImage;

    protected override void Start()
    {
        bool fadeIn = GetParameter(0).ToLower() == "in";
        float duration = GetParameterFloat(1, 1f);

        EnsureFadeElements();
        StartCoroutine(FadeRoutine(fadeIn, duration));
    }

    private void EnsureFadeElements()
    {
        if (_fadeCanvas == null)
        {
            var go = new GameObject("SequencerFadeCanvas");
            _fadeCanvas = go.AddComponent<Canvas>();
            _fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _fadeCanvas.sortingOrder = 9999;
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(go);

            var imgGo = new GameObject("FadeImage");
            imgGo.transform.SetParent(go.transform);
            _fadeImage = imgGo.AddComponent<Image>();
            _fadeImage.color = Color.black;
            RectTransform rect = _fadeImage.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
        }
    }

    private IEnumerator FadeRoutine(bool fadeIn, float duration)
    {
        float start = fadeIn ? 1f : 0f;
        float end = fadeIn ? 0f : 1f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(start, end, elapsed / duration);
            Color c = _fadeImage.color;
            c.a = alpha;
            _fadeImage.color = c;
            yield return null;
        }

        Color finalC = _fadeImage.color;
        finalC.a = end;
        _fadeImage.color = finalC;

        Stop();
    }
}