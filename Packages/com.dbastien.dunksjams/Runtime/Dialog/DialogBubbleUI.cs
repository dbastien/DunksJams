using UnityEngine;
using TMPro;
using System.Collections;

public class DialogBubbleUI : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    public CanvasGroup canvasGroup;
    public Vector2 screenOffset = new(0, 100f);
    public float margin = 50f;

    private Transform _target;
    private Camera _mainCamera;
    private RectTransform _rectTransform;
    private Canvas _canvas;

    public void Setup(string text, float duration, DialogueActor actor)
    {
        if (textMesh != null) textMesh.text = text;
        _target = actor.bubbleAnchor;
        _mainCamera = Camera.main;
        _rectTransform = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();

        if (canvasGroup != null) canvasGroup.alpha = 0;

        StartCoroutine(BubbleLifecycle(duration));
    }

    private void LateUpdate()
    {
        if (_target == null || _mainCamera == null || _rectTransform == null || _canvas == null) return;

        // Convert world position to screen position
        Vector3 screenPos = _mainCamera.WorldToScreenPoint(_target.position);

        // Check if behind camera
        bool isBehind = Vector3.Dot(_target.position - _mainCamera.transform.position, _mainCamera.transform.forward) <
                        0;

        if (isBehind)
        {
            if (canvasGroup != null) canvasGroup.alpha = 0;
            return;
        }

        // Clamping to screen
        screenPos.x = Mathf.Clamp(screenPos.x, margin, Screen.width - margin);
        screenPos.y = Mathf.Clamp(screenPos.y, margin, Screen.height - margin);

        // Adjust for offset
        screenPos += (Vector3)screenOffset;

        // Set position
        if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay) { _rectTransform.position = screenPos; }
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)_canvas.transform, screenPos,
                _canvas.worldCamera, out Vector2 localPos);
            _rectTransform.anchoredPosition = localPos;
        }
    }

    private IEnumerator BubbleLifecycle(float duration)
    {
        // Fade in
        if (canvasGroup != null)
        {
            float t = 0;
            while (t < 0.2f)
            {
                canvasGroup.alpha = t / 0.2f;
                t += Time.deltaTime;
                yield return null;
            }

            canvasGroup.alpha = 1;
        }

        yield return new WaitForSeconds(duration);

        // Fade out
        if (canvasGroup != null)
        {
            float t = 0;
            while (t < 0.5f)
            {
                canvasGroup.alpha = 1 - t / 0.5f;
                t += Time.deltaTime;
                yield return null;
            }
        }

        Destroy(gameObject);
    }
}