using System;
using UnityEngine;

public static class TweenExtensions
{
    public static Tween<Vector3> MoveTo(this Transform t, Vector3 target, float d, EaseType e) =>
        AddTween(new Tween<Vector3>(t.position, target, d, null, v => t.position = v, Vector3.Lerp).SetEase(e));

    public static Tween<Vector3> MoveTo(this Transform t, Vector3 target, float d, Func<float, float> e) =>
        AddTween(new Tween<Vector3>(t.position, target, d, e, v => t.position = v, Vector3.Lerp));

    public static Tween<Quaternion> RotateTo(this Transform t, Quaternion target, float d, EaseType e) =>
        AddTween(new Tween<Quaternion>(t.rotation, target, d, null, v => t.rotation = v, Quaternion.Lerp).SetEase(e));

    public static Tween<float> FadeTo(this CanvasGroup cg, float target, float d, EaseType e) =>
        AddTween(new Tween<float>(cg.alpha, target, d, null, v => cg.alpha = v, Mathf.Lerp).SetEase(e));

    public static Tween<Color> ColorTo(this SpriteRenderer sr, Color target, float d, EaseType e) =>
        AddTween(new Tween<Color>(sr.color, target, d, null, v => sr.color = v, Color.Lerp).SetEase(e));

    // From tweening methods (tween from start value to current value)
    public static Tween<Vector3> MoveFrom(this Transform t, Vector3 startPos, float d, EaseType e) =>
        AddTween(new Tween<Vector3>(startPos, t.position, d, null, v => t.position = v, Vector3.Lerp).SetEase(e));

    public static Tween<Vector3> MoveFrom(this Transform t, Vector3 startPos, float d, Func<float, float> e) =>
        AddTween(new Tween<Vector3>(startPos, t.position, d, e, v => t.position = v, Vector3.Lerp));

    public static Tween<Quaternion> RotateFrom(this Transform t, Quaternion startRot, float d, EaseType e) =>
        AddTween(new Tween<Quaternion>(startRot, t.rotation, d, null, v => t.rotation = v, Quaternion.Lerp).SetEase(e));

    public static Tween<Vector3> ScaleFrom(this Transform t, Vector3 startScale, float d, EaseType e) =>
        AddTween(new Tween<Vector3>(startScale, t.localScale, d, null, v => t.localScale = v, Vector3.Lerp).SetEase(e));

    public static Tween<float> FadeFrom(this CanvasGroup cg, float startAlpha, float d, EaseType e) =>
        AddTween(new Tween<float>(startAlpha, cg.alpha, d, null, v => cg.alpha = v, Mathf.Lerp).SetEase(e));

    public static Tween<Color> ColorFrom(this SpriteRenderer sr, Color startColor, float d, EaseType e) =>
        AddTween(new Tween<Color>(startColor, sr.color, d, null, v => sr.color = v, Color.Lerp).SetEase(e));

    // Relative tweening methods
    public static Tween<Vector3> MoveBy(this Transform t, Vector3 offset, float d, EaseType e) =>
        AddTween(new Tween<Vector3>(t.position, t.position + offset, d, null, v => t.position = v, Vector3.Lerp).SetEase(e));

    public static Tween<Vector3> MoveBy(this Transform t, Vector3 offset, float d, Func<float, float> e) =>
        AddTween(new Tween<Vector3>(t.position, t.position + offset, d, e, v => t.position = v, Vector3.Lerp));

    public static Tween<Vector3> ScaleTo(this Transform t, Vector3 target, float d, EaseType e) =>
        AddTween(new Tween<Vector3>(t.localScale, target, d, null, v => t.localScale = v, Vector3.Lerp).SetEase(e));

    public static Tween<Vector3> ScaleBy(this Transform t, Vector3 scaleOffset, float d, EaseType e) =>
        AddTween(new Tween<Vector3>(t.localScale, t.localScale + scaleOffset, d, null, v => t.localScale = v, Vector3.Lerp).SetEase(e));

    // Support for missing types: Vector2, int, Rect

    // Vector2 interpolation helper
    static Vector2 LerpVector2(Vector2 a, Vector2 b, float t) => Vector2.Lerp(a, b, t);

    // Rect interpolation helper (interpolate each component)
    static Rect LerpRect(Rect a, Rect b, float t) => new Rect(
        Mathf.Lerp(a.x, b.x, t),
        Mathf.Lerp(a.y, b.y, t),
        Mathf.Lerp(a.width, b.width, t),
        Mathf.Lerp(a.height, b.height, t)
    );

    // Int interpolation helper (lerp and round)
    static int LerpInt(int a, int b, float t) => Mathf.RoundToInt(Mathf.Lerp(a, b, t));

    // Vector2 tweening extensions
    public static Tween<Vector2> TweenAnchoredPosition(this RectTransform rt, Vector2 target, float d, EaseType e) =>
        AddTween(new Tween<Vector2>(rt.anchoredPosition, target, d, null, v => rt.anchoredPosition = v, LerpVector2).SetEase(e));

    public static Tween<Vector2> TweenSizeDelta(this RectTransform rt, Vector2 target, float d, EaseType e) =>
        AddTween(new Tween<Vector2>(rt.sizeDelta, target, d, null, v => rt.sizeDelta = v, LerpVector2).SetEase(e));

    // Int tweening extensions (useful for discrete values like score, health, etc.)
    public static Tween<int> TweenInt(this object target, int startValue, int endValue, float duration, Action<int> setter, EaseType easeType) =>
        AddTween(new Tween<int>(startValue, endValue, duration, null, setter, LerpInt).SetEase(easeType));

    // Rect tweening extensions (useful for UI layout changes)
    public static Tween<Rect> TweenRect(this object target, Rect startValue, Rect endValue, float duration, Action<Rect> setter, EaseType easeType) =>
        AddTween(new Tween<Rect>(startValue, endValue, duration, null, setter, LerpRect).SetEase(easeType));

    static Tween<T> AddTween<T>(Tween<T> tween)
    {
        TweenManager.Instance.Add(tween);
        return tween;
    }
}