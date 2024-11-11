using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TweenManager : SingletonBehavior<TweenManager>
{
    readonly List<ITween> _tweens = new();

    protected override void InitInternal()
    {
        // Uncomment to persist across scenes
        // DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        float deltaTime = Time.deltaTime;
        for (int i = _tweens.Count - 1; i >= 0; --i)
        {
            ITween tween = _tweens[i];
            tween.Update(deltaTime);
            if (tween.IsComplete)
                _tweens.RemoveAt(i);
        }
    }

    public void Add(ITween tween) => _tweens.Add(tween);

    public void PauseAll() { foreach (ITween tween in _tweens) tween.Pause(); }
    public void ResumeAll() { foreach (ITween tween in _tweens) tween.Resume(); }
    public void RewindAll() { foreach (ITween tween in _tweens) tween.Rewind(); }

    public void KillAll()
    {
        foreach (ITween tween in _tweens) tween.Kill();
        _tweens.Clear();
    }

    public void PauseById(string id)
    {
        foreach (ITween tween in _tweens)
            if (tween.Id == id) tween.Pause();
    }

    public void KillByTag(string tag)
    {
        foreach (ITween tween in _tweens)
            if (tween.Tag == tag) tween.Kill();
        _tweens.RemoveAll(t => t.Tag == tag);
    }

    public ITween GetById(string id)
    {
        foreach (var tween in _tweens)
            if (tween.Id == id) return tween;
        return null;
    }
    
    public List<ITween> GetByTag(string tag) => _tweens.Where(t => t.Tag == tag).ToList();
}