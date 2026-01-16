using System;
using UnityEngine;

public static class Tweening
{
    public static Tween<float> To(Func<float> getter, Action<float> setter, float endValue, float duration, EaseType easeType)
    {
        float startValue = getter();
        var tween = new Tween<float>(
            startValue,
            endValue,
            duration,
            null,
            setter,
            Mathf.Lerp
        ).SetEase(easeType);
        TweenManager.Instance.Add(tween);
        return tween;
    }

    public static Tween<Vector3> To(Func<Vector3> getter, Action<Vector3> setter, Vector3 endValue, float duration, EaseType easeType)
    {
        Vector3 startValue = getter();
        var tween = new Tween<Vector3>(
            startValue,
            endValue,
            duration,
            null,
            setter,
            Vector3.Lerp
        ).SetEase(easeType);
        TweenManager.Instance.Add(tween);
        return tween;
    }

    public static Tween<Color> To(Func<Color> getter, Action<Color> setter, Color endValue, float duration, EaseType easeType)
    {
        Color startValue = getter();
        var tween = new Tween<Color>(
            startValue,
            endValue,
            duration,
            null,
            setter,
            Color.Lerp
        ).SetEase(easeType);
        TweenManager.Instance.Add(tween);
        return tween;
    }

    public static Tween<Quaternion> To(Func<Quaternion> getter, Action<Quaternion> setter, Quaternion endValue, float duration, EaseType easeType)
    {
        Quaternion startValue = getter();
        var tween = new Tween<Quaternion>(
            startValue,
            endValue,
            duration,
            null,
            setter,
            Quaternion.Lerp
        ).SetEase(easeType);
        TweenManager.Instance.Add(tween);
        return tween;
    }

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

    public static Tween<Vector2> To(Func<Vector2> getter, Action<Vector2> setter, Vector2 endValue, float duration, EaseType easeType)
    {
        Vector2 startValue = getter();
        var tween = new Tween<Vector2>(
            startValue,
            endValue,
            duration,
            null,
            setter,
            LerpVector2
        ).SetEase(easeType);
        TweenManager.Instance.Add(tween);
        return tween;
    }

    public static Tween<int> To(Func<int> getter, Action<int> setter, int endValue, float duration, EaseType easeType)
    {
        int startValue = getter();
        var tween = new Tween<int>(
            startValue,
            endValue,
            duration,
            null,
            setter,
            LerpInt
        ).SetEase(easeType);
        TweenManager.Instance.Add(tween);
        return tween;
    }

    public static Tween<Rect> To(Func<Rect> getter, Action<Rect> setter, Rect endValue, float duration, EaseType easeType)
    {
        Rect startValue = getter();
        var tween = new Tween<Rect>(
            startValue,
            endValue,
            duration,
            null,
            setter,
            LerpRect
        ).SetEase(easeType);
        TweenManager.Instance.Add(tween);
        return tween;
    }
}
