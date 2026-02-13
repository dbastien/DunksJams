using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class ScreenSafeArea : MonoBehaviour
{
    [SerializeField] bool conformX = true;
    [SerializeField] bool conformY = true;

    RectTransform rectTransform;
    Rect lastSafeArea;

    void Awake() => rectTransform = GetComponent<RectTransform>();

    void OnEnable() => Refresh();

    void Update()
    {
        var safeArea = Screen.safeArea;
        if (lastSafeArea != safeArea)
        {
            lastSafeArea = safeArea;
            Refresh();
        }
    }

    public void Refresh()
    {
        var safeArea = Screen.safeArea;

        if (!conformX)
        {
            safeArea.x = 0;
            safeArea.width = Screen.width;
        }

        if (!conformY)
        {
            safeArea.y = 0;
            safeArea.height = Screen.height;
        }

        if (Screen.width > 0 && Screen.height > 0)
        {
            var anchorMin = safeArea.position;
            var anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
        }
    }
}