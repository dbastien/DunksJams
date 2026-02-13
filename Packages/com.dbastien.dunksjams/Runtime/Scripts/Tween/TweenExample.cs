using UnityEngine;

public class TweenExample : MonoBehaviour
{
    public Transform targetTransform;

    void Start()
    {
        if (!TweenManager.Instance) new GameObject("TweenManager").AddComponent<TweenManager>();

        // Tweening a Transform's position with method chaining and custom ease function
        TweenAPI.TweenTo(targetTransform.position, new Vector3(5f, 0f, 0f), 2f, pos => targetTransform.position = pos,
                EaseType.CubicInOut)
            .SetDelay(1f)
            .SetLoops(2, TweenLoopType.PingPong)
            .SetId("MoveTween")
            .OnStart(() => DLog.Log("Movement Tween Started"))
            .OnUpdate(() => DLog.Log("Movement Tween Updating"))
            .OnComplete(() => DLog.Log("Movement Tween Completed"));

        // Tweening any property using generic TweenTo
        var myFloat = 0f;
        TweenAPI.TweenTo(myFloat, 10f, 5f, x => myFloat = x, EaseType.Linear)
            .OnUpdate(() => DLog.Log($"myFloat value: {myFloat}"));

        // Using a custom easing function
        float CustomEase(float t) => t * t * t;
        TweenAPI.TweenTo(targetTransform.position, new Vector3(-5f, 0f, 0f), 2f, pos => targetTransform.position = pos,
            CustomEase).SetId("CustomEaseTween");

        // For complex sequences, use callback chaining instead
        TweenAPI.TweenTo(targetTransform.position, new Vector3(0f, 5f, 0f), 2f, pos => targetTransform.position = pos,
                EaseType.SineInOut)
            .OnComplete(() =>
                TweenAPI.TweenTo(targetTransform.rotation, Quaternion.Euler(0f, 180f, 0f), 2f,
                    rot => targetTransform.rotation = rot, EaseType.SineInOut));

        // Test DLog clickable hyperlinks
        DLog.Log("Test info message with clickable caller info");
        DLog.LogW("Test warning - click the file:line to jump to code!");
        DLog.LogE("Test error - demonstrates clickable navigation");

        // Test hyperlink helpers
        DLog.Log($"Check out our {DLog.UrlLink("https://unity.com", "Unity website")} for more info");

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