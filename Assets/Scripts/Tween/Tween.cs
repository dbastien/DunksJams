using System;
using UnityEngine;

public class Tween<T> : ITween, IPoolable
{
    public bool IsComplete { get; private set; }
    public float Delay { get; private set; }
    public int Loops { get; private set; }
    public TweenLoopType LoopType { get; private set; } = TweenLoopType.Loop;
    public string Id { get; private set; }
    public string Tag { get; private set; }
    public bool IgnoreTimeScale { get; private set; }
    public float TimeScale { get; private set; } = 1f;
    
    //todo: not sure?
    public float Duration => _duration + Delay;
    
    T _startValue;
    T _endValue;
    float _duration;
    EaseType _easeType = EaseType.Linear;
    Func<float, float> _customEase;
    Action<T> _onUpdateValue;
    Func<T, T, float, T> _interpolator;

    float _elapsedTime;
    bool _isPaused;
    bool _isCancelled;

    // Control variables
    bool _hasStarted;
    int _completedLoops;
    float _delayElapsed;

    // Callbacks
    Action _onStart;
    Action _onUpdate;
    Action _onComplete;
    Action _onStepComplete;
    Action _onRewind;

    public Tween()
    {
        // Default constructor for pooling
    }

    public Tween(
        T startValue,
        T endValue,
        float duration,
        Func<float, float> easingFunction,
        Action<T> onUpdateValue,
        Func<T, T, float, T> interpolator)
    {
        Initialize(startValue, endValue, duration, easingFunction, onUpdateValue, interpolator);
    }

    public void Initialize(
        T startValue,
        T endValue,
        float duration,
        Func<float, float> easingFunction,
        Action<T> onUpdateValue,
        Func<T, T, float, T> interpolator)
    {
        _startValue = startValue;
        _endValue = endValue;
        _duration = duration;
        _customEase = easingFunction;
        _onUpdateValue = onUpdateValue;
        _interpolator = interpolator;
        _elapsedTime = 0f;
        IsComplete = false;
        _isPaused = false;
        _isCancelled = false;
        _hasStarted = false;
        _completedLoops = 0;
        _delayElapsed = 0f;

        // Reset callbacks
        _onStart = null;
        _onUpdate = null;
        _onComplete = null;
        _onStepComplete = null;
        _onRewind = null;

        // Reset configuration
        Delay = 0f;
        Loops = 1;
        LoopType = TweenLoopType.Loop;
        Id = null;
        Tag = null;
        IgnoreTimeScale = false;
        TimeScale = 1f;
        _easeType = EaseType.Linear;
    }

    // Method chaining for setting options
    public Tween<T> SetDelay(float delay)
    {
        Delay = delay;
        return this;
    }

    public Tween<T> SetLoops(int loops, TweenLoopType loopType = TweenLoopType.Loop)
    {
        Loops = loops;
        LoopType = loopType;
        return this;
    }

    public Tween<T> SetId(string id)
    {
        Id = id;
        return this;
    }

    public Tween<T> SetTag(string tag)
    {
        Tag = tag;
        return this;
    }

    public Tween<T> SetIgnoreTimeScale(bool ignoreTimeScale)
    {
        IgnoreTimeScale = ignoreTimeScale;
        return this;
    }

    public Tween<T> SetTimeScale(float timeScale)
    {
        TimeScale = timeScale;
        return this;
    }

    public Tween<T> SetEase(EaseType easeType)
    {
        _easeType = easeType;
        _customEase = null;
        return this;
    }

    public Tween<T> SetEase(AnimationCurve animCurve)
    {
        _customEase = animCurve.Evaluate;
        return this;
    }

    public Tween<T> SetEase(Func<float, float> customEase)
    {
        _customEase = customEase;
        return this;
    }

    public float Evaluate(float t)
    {
        if (_customEase != null)
            return _customEase(t);
        return Ease.Evaluate(_easeType, t);
    }

    public void Reset()
    {
        _elapsedTime = 0f;
        _isPaused = false;
        _isCancelled = false;
        _hasStarted = false;
        _completedLoops = 0;
        _delayElapsed = 0f;
        IsComplete = false;
    }

    public Tween<T> OnStart(Action callback)
    {
        _onStart = callback;
        return this;
    }

    public Tween<T> OnUpdate(Action callback)
    {
        _onUpdate = callback;
        return this;
    }

    public Tween<T> OnComplete(Action callback)
    {
        _onComplete = callback;
        return this;
    }

    public Tween<T> OnStepComplete(Action callback)
    {
        _onStepComplete = callback;
        return this;
    }

    public Tween<T> OnRewind(Action callback)
    {
        _onRewind = callback;
        return this;
    }

    public void Update(float deltaTime)
    {
        if (IsComplete || _isPaused || _isCancelled) return;

        deltaTime = IgnoreTimeScale ? Time.unscaledDeltaTime * TimeScale : deltaTime * TimeScale;

        if (!_hasStarted)
        {
            _delayElapsed += deltaTime;
            if (_delayElapsed < Delay) return;
            _hasStarted = true;
            _onStart?.Invoke();
        }

        _elapsedTime += deltaTime;
        float t = Mathf.Clamp01(_elapsedTime / _duration);
        float easedT = Evaluate(t);

        T currentValue = _interpolator(_startValue, _endValue, easedT);
        _onUpdate?.Invoke();
        _onUpdateValue(currentValue);

        if (!(_elapsedTime >= _duration)) return;
        ++_completedLoops;
        _onStepComplete?.Invoke();

        if (Loops == -1 || _completedLoops < Loops)
        {
            _elapsedTime = 0f;
            switch (LoopType)
            {
                //todo: add vector2, vector4, other shit
                case TweenLoopType.Loop:
                    break;
                case TweenLoopType.PingPong:
                    (_startValue, _endValue) = (_endValue, _startValue);
                    break;
                case TweenLoopType.Incremental:
                    _startValue = _endValue;
                    _endValue = typeof(T) switch
                    {
                        var tt when tt == typeof(float) => (T)(object)((float)(object)_endValue + (float)(object)_endValue),
                        var tt when tt == typeof(Vector3) => (T)(object)((Vector3)(object)_endValue + (Vector3)(object)_endValue),
                        var tt when tt == typeof(Quaternion) => (T)(object)Quaternion.Euler(((Quaternion)(object)_endValue).eulerAngles * 2),
                        var tt when tt == typeof(Color) => (T)(object)new Color(
                            Mathf.Clamp01(((Color)(object)_endValue).r * 2),
                            Mathf.Clamp01(((Color)(object)_endValue).g * 2),
                            Mathf.Clamp01(((Color)(object)_endValue).b * 2),
                            Mathf.Clamp01(((Color)(object)_endValue).a * 2)
                        ),
                        _ => throw new InvalidOperationException($"Unsupported type {typeof(T)} for TweenLoopType.Incremental")
                    };
                    break;
            }
        }
        else
        {
            IsComplete = true;
            _onComplete?.Invoke();
            ReturnToPool();
        }
    }

    public void Pause() => _isPaused = true;
    public void Resume() => _isPaused = false;

    public void Rewind()
    {
        _elapsedTime = 0f;
        _completedLoops = 0;
        _hasStarted = false;
        IsComplete = false;
        _onRewind?.Invoke();
    }

    public void Restart()
    {
        Rewind();
        Resume();
    }

    public void Kill()
    {
        _isCancelled = true;
        IsComplete = true;
        ReturnToPool();
    }

    // IPoolable implementation
    public void OnPoolGet() { }
    public void OnPoolRelease() { }

    // Pool management
    void ReturnToPool()
    {
        // Return to appropriate pool based on type
        if (this is Tween<float>) TweenAPI.ReturnToFloatPool(this as Tween<float>);
        else if (this is Tween<Vector2>) TweenAPI.ReturnToVector2Pool(this as Tween<Vector2>);
        else if (this is Tween<Vector3>) TweenAPI.ReturnToVector3Pool(this as Tween<Vector3>);
        else if (this is Tween<Color>) TweenAPI.ReturnToColorPool(this as Tween<Color>);
        else if (this is Tween<int>) TweenAPI.ReturnToIntPool(this as Tween<int>);
        else if (this is Tween<Rect>) TweenAPI.ReturnToRectPool(this as Tween<Rect>);
        else if (this is Tween<Quaternion>) TweenAPI.ReturnToQuaternionPool(this as Tween<Quaternion>);
    }
}
