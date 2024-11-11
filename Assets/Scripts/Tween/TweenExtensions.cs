using System;
using UnityEngine;

public static class TweenExtensions
{
    public static Tween<Vector3> MoveTo(this Transform t, Vector3 target, float d, EaseType e) =>
        AddTween(new Tween<Vector3>(t.position, target, d, Ease.GetEasingFunction(e), v => t.position = v, Vector3.Lerp));

    public static Tween<Vector3> MoveTo(this Transform t, Vector3 target, float d, Func<float, float> e) =>
        AddTween(new Tween<Vector3>(t.position, target, d, e, v => t.position = v, Vector3.Lerp));

    public static Tween<Quaternion> RotateTo(this Transform t, Quaternion target, float d, EaseType e) =>
        AddTween(new Tween<Quaternion>(t.rotation, target, d, Ease.GetEasingFunction(e), v => t.rotation = v, Quaternion.Lerp));

    public static Tween<float> FadeTo(this CanvasGroup cg, float target, float d, EaseType e) =>
        AddTween(new Tween<float>(cg.alpha, target, d, Ease.GetEasingFunction(e), v => cg.alpha = v, Mathf.Lerp));

    public static Tween<Color> ColorTo(this SpriteRenderer sr, Color target, float d, EaseType e) =>
        AddTween(new Tween<Color>(sr.color, target, d, Ease.GetEasingFunction(e), v => sr.color = v, Color.Lerp));

    static Tween<T> AddTween<T>(Tween<T> tween)
    {
        TweenManager.Instance.Add(tween);
        return tween;
    }
}