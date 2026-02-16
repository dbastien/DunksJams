using UnityEngine;

public class DialogueActor : MonoBehaviour
{
    public string actorName;
    public Sprite portrait;
    public GameObject bubblePrefab;
    public Transform bubbleAnchor;

    private void Awake()
    {
        if (bubbleAnchor == null) bubbleAnchor = transform;
    }

    public void Bark(string text, float duration = 3f)
    {
        if (bubblePrefab == null) return;

        GameObject bubbleObj = Instantiate(bubblePrefab, bubbleAnchor.position, Quaternion.identity);
        var bubble = bubbleObj.GetComponent<DialogBubbleUI>();
        if (bubble != null) bubble.Setup(text, duration, this);
    }
}