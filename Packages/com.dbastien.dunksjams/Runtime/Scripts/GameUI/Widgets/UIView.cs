using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIView : MonoBehaviour
{
    [SerializeField] private float defaultFadeDuration = 0.3f;

    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;

    public CanvasGroup CanvasGroup => canvasGroup;

    private void Awake() => canvasGroup = GetComponent<CanvasGroup>();

    public virtual void Show(float duration = -1f) { SetViewEnabled(duration < 0 ? defaultFadeDuration : duration); }

    public virtual void Hide(float duration = -1f) { SetViewDisabled(duration < 0 ? defaultFadeDuration : duration); }

    public virtual void SetViewEnabled(float transitionTime)
    {
        SetCanvasAlpha(1f, transitionTime);
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }

    public virtual void SetViewDisabled(float transitionTime)
    {
        SetCanvasAlpha(0f, transitionTime);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    private void SetCanvasAlpha(float targetAlpha, float duration)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeAlpha(targetAlpha, duration));
    }

    private IEnumerator FadeAlpha(float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        var elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        fadeCoroutine = null;
    }
}