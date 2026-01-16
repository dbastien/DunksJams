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
}
