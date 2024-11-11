using UnityEngine;

public class TweenExample : MonoBehaviour
{
    public Transform targetTransform;

    void Start()
    {
        if (!TweenManager.Instance) new GameObject("TweenManager").AddComponent<TweenManager>();

        // Tweening a Transform's position with method chaining and custom ease function
        targetTransform.MoveTo(new(5f, 0f, 0f), 2f, EaseType.CubicInOut)
            .SetDelay(1f)
            .SetLoops(2, TweenLoopType.PingPong)
            .SetId("MoveTween")
            .OnStart(() => DLog.Log("Movement Tween Started"))
            .OnUpdate(() => DLog.Log("Movement Tween Updating"))
            .OnComplete(() => DLog.Log("Movement Tween Completed"));

        // Tweening any property using Tweening.To
        float myFloat = 0f;
        Tweening.To(() => myFloat, x => myFloat = x, 10f, 5f, EaseType.Linear)
            .OnUpdate(() => DLog.Log($"myFloat value: {myFloat}"));

        // Using a custom easing function
        float CustomEase(float t) => t * t * t;
        targetTransform.MoveTo(new(-5f, 0f, 0f), 2f, CustomEase).SetId("CustomEaseTween");

        // Creating a sequence
        var sequence = new TweenSequence()
            .Append(targetTransform.MoveTo(new(0f, 5f, 0f), 2f, EaseType.SineInOut))
            .Append(targetTransform.RotateTo(Quaternion.Euler(0f, 180f, 0f), 2f, EaseType.SineInOut))
            .SetId("MySequence");

        TweenManager.Instance.Add(sequence);

        // Global controls
        // Pause all tweens with the ID "MoveTween" after 3 seconds
        Invoke(nameof(PauseMoveTween), 3f);
    }

    void PauseMoveTween()
    {
        TweenManager.Instance.PauseById("MoveTween");
        DLog.Log("Paused MoveTween");
    }
}