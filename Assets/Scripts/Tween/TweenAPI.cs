using System;
using UnityEngine;

public static class TweenAPI
{
    // ===== OBJECT POOLING =====

    static readonly ObjectPoolEx<Tween<float>> _floatPool = new(initialCap: 16, maxCap: 256);
    static readonly ObjectPoolEx<Tween<Vector2>> _vector2Pool = new(initialCap: 16, maxCap: 256);
    static readonly ObjectPoolEx<Tween<Vector3>> _vector3Pool = new(initialCap: 16, maxCap: 256);
    static readonly ObjectPoolEx<Tween<Color>> _colorPool = new(initialCap: 16, maxCap: 256);
    static readonly ObjectPoolEx<Tween<int>> _intPool = new(initialCap: 16, maxCap: 256);
    static readonly ObjectPoolEx<Tween<Rect>> _rectPool = new(initialCap: 16, maxCap: 256);
    static readonly ObjectPoolEx<Tween<Quaternion>> _quaternionPool = new(initialCap: 16, maxCap: 256);

    // ===== GENERIC TYPES =====

    // Float tweening
    public static Tween<float> TweenTo(float startValue, float endValue, float duration, Action<float> setter, EaseType easeType)
    {
        var tween = _floatPool.Get();
        tween.Initialize(startValue, endValue, duration, null, setter, Mathf.Lerp);
        tween.SetEase(easeType);
        return AddTween(tween);
    }

    public static Tween<float> TweenTo(float startValue, float endValue, float duration, Action<float> setter, Func<float, float> customEase)
    {
        var tween = _floatPool.Get();
        tween.Initialize(startValue, endValue, duration, customEase, setter, Mathf.Lerp);
        return AddTween(tween);
    }

    public static Tween<float> TweenFrom(float currentValue, float startValue, float duration, Action<float> setter, EaseType easeType) =>
        AddTween(new Tween<float>(startValue, currentValue, duration, null, setter, Mathf.Lerp).SetEase(easeType));

    public static Tween<float> TweenFrom(float currentValue, float startValue, float duration, Action<float> setter, Func<float, float> customEase) =>
        AddTween(new Tween<float>(startValue, currentValue, duration, customEase, setter, Mathf.Lerp));

    public static Tween<float> TweenBy(float currentValue, float offset, float duration, Action<float> setter, EaseType easeType) =>
        AddTween(new Tween<float>(currentValue, currentValue + offset, duration, null, setter, Mathf.Lerp).SetEase(easeType));

    public static Tween<float> TweenBy(float currentValue, float offset, float duration, Action<float> setter, Func<float, float> customEase) =>
        AddTween(new Tween<float>(currentValue, currentValue + offset, duration, customEase, setter, Mathf.Lerp));

    // Vector2 tweening
    public static Tween<Vector2> TweenTo(Vector2 startValue, Vector2 endValue, float duration, Action<Vector2> setter, EaseType easeType)
    {
        var tween = _vector2Pool.Get();
        tween.Initialize(startValue, endValue, duration, null, setter, Vector2.Lerp);
        tween.SetEase(easeType);
        return AddTween(tween);
    }

    public static Tween<Vector2> TweenTo(Vector2 startValue, Vector2 endValue, float duration, Action<Vector2> setter, Func<float, float> customEase)
    {
        var tween = _vector2Pool.Get();
        tween.Initialize(startValue, endValue, duration, customEase, setter, Vector2.Lerp);
        return AddTween(tween);
    }

    public static Tween<Vector2> TweenFrom(Vector2 currentValue, Vector2 startValue, float duration, Action<Vector2> setter, EaseType easeType)
    {
        var tween = _vector2Pool.Get();
        tween.Initialize(startValue, currentValue, duration, null, setter, Vector2.Lerp);
        tween.SetEase(easeType);
        return AddTween(tween);
    }

    public static Tween<Vector2> TweenFrom(Vector2 currentValue, Vector2 startValue, float duration, Action<Vector2> setter, Func<float, float> customEase)
    {
        var tween = _vector2Pool.Get();
        tween.Initialize(startValue, currentValue, duration, customEase, setter, Vector2.Lerp);
        return AddTween(tween);
    }

    public static Tween<Vector2> TweenBy(Vector2 currentValue, Vector2 offset, float duration, Action<Vector2> setter, EaseType easeType)
    {
        var tween = _vector2Pool.Get();
        tween.Initialize(currentValue, currentValue + offset, duration, null, setter, Vector2.Lerp);
        tween.SetEase(easeType);
        return AddTween(tween);
    }

    public static Tween<Vector2> TweenBy(Vector2 currentValue, Vector2 offset, float duration, Action<Vector2> setter, Func<float, float> customEase)
    {
        var tween = _vector2Pool.Get();
        tween.Initialize(currentValue, currentValue + offset, duration, customEase, setter, Vector2.Lerp);
        return AddTween(tween);
    }

    // Color tweening
    public static Tween<Color> TweenTo(Color startValue, Color endValue, float duration, Action<Color> setter, EaseType easeType)
    {
        var tween = _colorPool.Get();
        tween.Initialize(startValue, endValue, duration, null, setter, Color.Lerp);
        tween.SetEase(easeType);
        return AddTween(tween);
    }

    public static Tween<Color> TweenTo(Color startValue, Color endValue, float duration, Action<Color> setter, Func<float, float> customEase)
    {
        var tween = _colorPool.Get();
        tween.Initialize(startValue, endValue, duration, customEase, setter, Color.Lerp);
        return AddTween(tween);
    }

    public static Tween<Color> TweenFrom(Color currentValue, Color startValue, float duration, Action<Color> setter, EaseType easeType)
    {
        var tween = _colorPool.Get();
        tween.Initialize(startValue, currentValue, duration, null, setter, Color.Lerp);
        tween.SetEase(easeType);
        return AddTween(tween);
    }

    public static Tween<Color> TweenFrom(Color currentValue, Color startValue, float duration, Action<Color> setter, Func<float, float> customEase)
    {
        var tween = _colorPool.Get();
        tween.Initialize(startValue, currentValue, duration, customEase, setter, Color.Lerp);
        return AddTween(tween);
    }

    public static Tween<Color> TweenBy(Color currentValue, Color offset, float duration, Action<Color> setter, EaseType easeType)
    {
        var tween = _colorPool.Get();
        tween.Initialize(currentValue, currentValue + offset, duration, null, setter, Color.Lerp);
        tween.SetEase(easeType);
        return AddTween(tween);
    }

    public static Tween<Color> TweenBy(Color currentValue, Color offset, float duration, Action<Color> setter, Func<float, float> customEase)
    {
        var tween = _colorPool.Get();
        tween.Initialize(currentValue, currentValue + offset, duration, customEase, setter, Color.Lerp);
        return AddTween(tween);
    }

    // Int tweening
    public static Tween<int> TweenTo(int startValue, int endValue, float duration, Action<int> setter, EaseType easeType)
    {
        var tween = _intPool.Get();
        tween.Initialize(startValue, endValue, duration, null, setter, IntExtensions.Lerp);
        tween.SetEase(easeType);
        return AddTween(tween);
    }

    public static Tween<int> TweenTo(int startValue, int endValue, float duration, Action<int> setter, Func<float, float> customEase)
    {
        var tween = _intPool.Get();
        tween.Initialize(startValue, endValue, duration, customEase, setter, IntExtensions.Lerp);
        return AddTween(tween);
    }

    public static Tween<int> TweenFrom(int currentValue, int startValue, float duration, Action<int> setter, EaseType easeType)
    {
        var tween = _intPool.Get();
        tween.Initialize(startValue, currentValue, duration, null, setter, IntExtensions.Lerp);
        tween.SetEase(easeType);
        return AddTween(tween);
    }

    public static Tween<int> TweenFrom(int currentValue, int startValue, float duration, Action<int> setter, Func<float, float> customEase)
    {
        var tween = _intPool.Get();
        tween.Initialize(startValue, currentValue, duration, customEase, setter, IntExtensions.Lerp);
        return AddTween(tween);
    }

    public static Tween<int> TweenBy(int currentValue, int offset, float duration, Action<int> setter, EaseType easeType)
    {
        var tween = _intPool.Get();
        tween.Initialize(currentValue, currentValue + offset, duration, null, setter, IntExtensions.Lerp);
        tween.SetEase(easeType);
        return AddTween(tween);
    }

    public static Tween<int> TweenBy(int currentValue, int offset, float duration, Action<int> setter, Func<float, float> customEase)
    {
        var tween = _intPool.Get();
        tween.Initialize(currentValue, currentValue + offset, duration, customEase, setter, IntExtensions.Lerp);
        return AddTween(tween);
    }

    // Quaternion tweening
    public static Tween<Quaternion> TweenTo(Quaternion startValue, Quaternion endValue, float duration, Action<Quaternion> setter, EaseType easeType)
    {
        var tween = _quaternionPool.Get();
        tween.Initialize(startValue, endValue, duration, null, setter, Quaternion.Slerp);
        tween.SetEase(easeType);
        return AddTween(tween);
    }

    public static Tween<Quaternion> TweenTo(Quaternion startValue, Quaternion endValue, float duration, Action<Quaternion> setter, Func<float, float> customEase)
    {
        var tween = _quaternionPool.Get();
        tween.Initialize(startValue, endValue, duration, customEase, setter, Quaternion.Slerp);
        return AddTween(tween);
    }

    public static Tween<Quaternion> TweenFrom(Quaternion currentValue, Quaternion startValue, float duration, Action<Quaternion> setter, EaseType easeType)
    {
        var tween = _quaternionPool.Get();
        tween.Initialize(startValue, currentValue, duration, null, setter, Quaternion.Slerp);
        tween.SetEase(easeType);
        return AddTween(tween);
    }

    public static Tween<Quaternion> TweenFrom(Quaternion currentValue, Quaternion startValue, float duration, Action<Quaternion> setter, Func<float, float> customEase)
    {
        var tween = _quaternionPool.Get();
        tween.Initialize(startValue, currentValue, duration, customEase, setter, Quaternion.Slerp);
        return AddTween(tween);
    }

    public static Tween<Quaternion> TweenBy(Quaternion currentValue, Quaternion offset, float duration, Action<Quaternion> setter, EaseType easeType)
    {
        var tween = _quaternionPool.Get();
        var targetValue = currentValue * offset;
        tween.Initialize(currentValue, targetValue, duration, null, setter, Quaternion.Slerp);
        tween.SetEase(easeType);
        return AddTween(tween);
    }

    public static Tween<Quaternion> TweenBy(Quaternion currentValue, Quaternion offset, float duration, Action<Quaternion> setter, Func<float, float> customEase)
    {
        var tween = _quaternionPool.Get();
        var targetValue = currentValue * offset;
        tween.Initialize(currentValue, targetValue, duration, customEase, setter, Quaternion.Slerp);
        return AddTween(tween);
    }

    // Rect tweening
    public static Tween<Rect> TweenTo(Rect startValue, Rect endValue, float duration, Action<Rect> setter, EaseType easeType)
    {
        var tween = _rectPool.Get();
        tween.Initialize(startValue, endValue, duration, null, setter, RectExtensions.Lerp);
        tween.SetEase(easeType);
        return AddTween(tween);
    }

    public static Tween<Rect> TweenTo(Rect startValue, Rect endValue, float duration, Action<Rect> setter, Func<float, float> customEase)
    {
        var tween = _rectPool.Get();
        tween.Initialize(startValue, endValue, duration, customEase, setter, RectExtensions.Lerp);
        return AddTween(tween);
    }

    public static Tween<Rect> TweenFrom(Rect currentValue, Rect startValue, float duration, Action<Rect> setter, EaseType easeType)
    {
        var tween = _rectPool.Get();
        tween.Initialize(startValue, currentValue, duration, null, setter, RectExtensions.Lerp);
        tween.SetEase(easeType);
        return AddTween(tween);
    }

    public static Tween<Rect> TweenFrom(Rect currentValue, Rect startValue, float duration, Action<Rect> setter, Func<float, float> customEase)
    {
        var tween = _rectPool.Get();
        tween.Initialize(startValue, currentValue, duration, customEase, setter, RectExtensions.Lerp);
        return AddTween(tween);
    }

    public static Tween<Rect> TweenBy(Rect currentValue, Rect offset, float duration, Action<Rect> setter, EaseType easeType)
    {
        var tween = _rectPool.Get();
        tween.Initialize(currentValue, new Rect(currentValue.x + offset.x, currentValue.y + offset.y, currentValue.width + offset.width, currentValue.height + offset.height), duration, null, setter, RectExtensions.Lerp);
        tween.SetEase(easeType);
        return AddTween(tween);
    }

    public static Tween<Rect> TweenBy(Rect currentValue, Rect offset, float duration, Action<Rect> setter, Func<float, float> customEase)
    {
        var tween = _rectPool.Get();
        tween.Initialize(currentValue, new Rect(currentValue.x + offset.x, currentValue.y + offset.y, currentValue.width + offset.width, currentValue.height + offset.height), duration, customEase, setter, RectExtensions.Lerp);
        return AddTween(tween);
    }

    // ===== POOL MANAGEMENT =====

    public static void ReturnToFloatPool(Tween<float> tween) => _floatPool.Release(tween);
    public static void ReturnToVector2Pool(Tween<Vector2> tween) => _vector2Pool.Release(tween);
    public static void ReturnToVector3Pool(Tween<Vector3> tween) => _vector3Pool.Release(tween);
    public static void ReturnToColorPool(Tween<Color> tween) => _colorPool.Release(tween);
    public static void ReturnToIntPool(Tween<int> tween) => _intPool.Release(tween);
    public static void ReturnToRectPool(Tween<Rect> tween) => _rectPool.Release(tween);
    public static void ReturnToQuaternionPool(Tween<Quaternion> tween) => _quaternionPool.Release(tween);

    // ===== INTERPOLATION HELPERS =====

    static Tween<T> AddTween<T>(Tween<T> tween)
    {
        TweenManager.Instance.Add(tween);
        return tween;
    }
}