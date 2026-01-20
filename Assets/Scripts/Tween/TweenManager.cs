using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TweenManager : SingletonEagerBehaviour<TweenManager>
{
    readonly List<ITween> _tweens = new();
    readonly Dictionary<string, ITween> _tweensById = new();
    readonly Dictionary<string, List<ITween>> _tweensByTag = new();

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
            {
                RemoveTween(tween);
                _tweens.RemoveAt(i);
            }
        }
    }

    void RemoveTween(ITween tween)
    {
        // Remove from ID dictionary
        if (!string.IsNullOrEmpty(tween.Id))
            _tweensById.Remove(tween.Id);

        // Remove from tag dictionary
        if (!string.IsNullOrEmpty(tween.Tag) && _tweensByTag.TryGetValue(tween.Tag, out var tagList))
        {
            tagList.Remove(tween);
            if (tagList.Count == 0)
                _tweensByTag.Remove(tween.Tag);
        }
    }

    public void Add(ITween tween)
    {
        _tweens.Add(tween);

        // Add to ID dictionary if ID is set
        if (!string.IsNullOrEmpty(tween.Id))
            _tweensById[tween.Id] = tween;

        // Add to tag dictionary if tag is set
        if (!string.IsNullOrEmpty(tween.Tag))
        {
            if (!_tweensByTag.TryGetValue(tween.Tag, out var tagList))
            {
                tagList = new List<ITween>();
                _tweensByTag[tween.Tag] = tagList;
            }
            tagList.Add(tween);
        }
    }

    public void PauseAll() { foreach (ITween tween in _tweens) tween.Pause(); }
    public void ResumeAll() { foreach (ITween tween in _tweens) tween.Resume(); }
    public void RewindAll() { foreach (ITween tween in _tweens) tween.Rewind(); }

    public void KillAll()
    {
        foreach (ITween tween in _tweens) tween.Kill();
        _tweens.Clear();
        _tweensById.Clear();
        _tweensByTag.Clear();
    }

    public void PauseById(string id)
    {
        foreach (ITween tween in _tweens)
            if (tween.Id == id) tween.Pause();
    }

    public void KillByTag(string tag)
    {
        if (_tweensByTag.TryGetValue(tag, out var tagList))
        {
            foreach (ITween tween in tagList)
                tween.Kill();
            _tweens.RemoveAll(t => t.Tag == tag);
            _tweensByTag.Remove(tag);
        }
    }

    public ITween GetById(string id)
    {
        return _tweensById.TryGetValue(id, out var tween) ? tween : null;
    }

    public List<ITween> GetByTag(string tag, List<ITween> result = null)
    {
        if (_tweensByTag.TryGetValue(tag, out var tagList))
        {
            result ??= new List<ITween>();
            result.Clear();
            result.AddRange(tagList);
            return result;
        }
        return result ?? new List<ITween>();
    }
}