using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class ScreenSafeArea : MonoBehaviour
{
    [SerializeField] private bool conformX = true;
    [SerializeField] private bool conformY = true;

    private RectTransform rectTransform;
    private Rect lastSafeArea;

    private void Awake() => rectTransform = GetComponent<RectTransform>();

    private void OnEnable() => Refresh();

    private void Update()
    {
        Rect safeArea = Screen.safeArea;
        if (lastSafeArea != safeArea)
        {
            lastSafeArea = safeArea;
            Refresh();
        }
    }

    public void Refresh()
    {
        Rect safeArea = Screen.safeArea;

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
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
        }
    }
}