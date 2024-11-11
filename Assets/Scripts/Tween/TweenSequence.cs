using System.Collections.Generic;
using UnityEngine;

public class TweenSequence : ITween
{
    public float Duration => _tweens.Count > 0 ? _tweens[^1].atPosition + _tweens[^1].tween.Duration : 0f;
    
    readonly List<(float atPosition, ITween tween)> _tweens = new();
    float _sequenceTime;
    bool _isPaused;
    bool _isCancelled;

    public bool IsComplete { get; private set; }
    public string Id { get; private set; }
    public string Tag { get; private set; }
    public bool IgnoreTimeScale { get; private set; }
    public float TimeScale { get; private set; } = 1f;

    public TweenSequence SetId(string id)
    {
        Id = id;
        return this;
    }

    public TweenSequence SetTag(string tag)
    {
        Tag = tag;
        return this;
    }

    public TweenSequence SetIgnoreTimeScale(bool ignoreTimeScale)
    {
        IgnoreTimeScale = ignoreTimeScale;
        return this;
    }

    public TweenSequence SetTimeScale(float timeScale)
    {
        TimeScale = timeScale;
        return this;
    }

    public TweenSequence Insert(float atPosition, ITween tween)
    {
        _tweens.Add((atPosition, tween));
        return this;
    }

    public TweenSequence Append(ITween tween)
    {
        float atPosition = _tweens.Count > 0 ? _tweens[^1].atPosition + _tweens[^1].tween.Duration : 0f;
        _tweens.Add((atPosition, tween));
        return this;
    }
    
    public void Update(float deltaTime)
    {
        if (IsComplete || _isPaused || _isCancelled) return;

        deltaTime *= TimeScale;
        deltaTime = IgnoreTimeScale ? Time.unscaledDeltaTime * TimeScale : deltaTime;
        _sequenceTime += deltaTime;

        foreach ((float atPosition, ITween tween) in _tweens)
            if (_sequenceTime >= atPosition && !tween.IsComplete) tween.Update(deltaTime);

        IsComplete = _tweens.TrueForAll(t => t.tween.IsComplete);
    }

    public void Pause()
    {
        _isPaused = true;
        foreach ((_, ITween tween) in _tweens) tween.Pause();
    }

    public void Resume()
    {
        _isPaused = false;
        foreach ((_, ITween tween) in _tweens) tween.Resume();
    }

    public void Rewind()
    {
        _sequenceTime = 0f;
        IsComplete = false;
        foreach ((_, ITween tween) in _tweens) tween.Rewind();
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
        foreach ((_, ITween tween) in _tweens) tween.Kill();
    }
}
