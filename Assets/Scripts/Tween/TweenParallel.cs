using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TweenParallel : ITween
{
    public float Duration => _tweens.Count > 0 ? _tweens.Max(t => t.Duration) : 0f;    
    public bool IsComplete { get; private set; }
    public string Id { get; private set; }
    public string Tag { get; private set; }
    public bool IgnoreTimeScale { get; private set; }
    public float TimeScale { get; private set; } = 1f;

    readonly List<ITween> _tweens = new();
    bool _isPaused;
    bool _isCancelled;
    
    public TweenParallel SetId(string id)
    {
        Id = id;
        return this;
    }

    public TweenParallel SetTag(string tag)
    {
        Tag = tag;
        return this;
    }

    public TweenParallel SetIgnoreTimeScale(bool ignoreTimeScale)
    {
        IgnoreTimeScale = ignoreTimeScale;
        return this;
    }

    public TweenParallel SetTimeScale(float timeScale)
    {
        TimeScale = timeScale;
        return this;
    }

    public TweenParallel Join(ITween tween)
    {
        _tweens.Add(tween);
        return this;
    }

    public void Update(float deltaTime)
    {
        if (IsComplete || _isPaused || _isCancelled) return;

        deltaTime *= TimeScale;
        deltaTime = IgnoreTimeScale ? Time.unscaledDeltaTime * TimeScale : deltaTime;

        for (int i = _tweens.Count - 1; i >= 0; --i)
        {
            ITween tween = _tweens[i];
            tween.Update(deltaTime);
            if (tween.IsComplete) _tweens.RemoveAt(i);
        }

        if (_tweens.Count == 0) IsComplete = true;
    }

    public void Pause()
    {
        _isPaused = true;
        foreach (ITween tween in _tweens) tween.Pause();
    }

    public void Resume()
    {
        _isPaused = false;
        foreach (ITween tween in _tweens) tween.Resume();
    }

    public void Rewind()
    {
        IsComplete = false;
        foreach (ITween tween in _tweens) tween.Rewind();
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
        foreach (ITween tween in _tweens) tween.Kill();
        _tweens.Clear();
    }
}
