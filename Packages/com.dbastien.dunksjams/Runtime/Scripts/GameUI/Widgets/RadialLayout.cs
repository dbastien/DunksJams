using UnityEngine;
using UnityEngine.UI;

// Radial Layout Group by Just a Pixel (Danny Goodayle) - http://www.justapixel.co.uk
// Modified and simplified for DunksJams
public class RadialLayout : LayoutGroup
{
    [SerializeField] float distance = 100f;
    [SerializeField] [Range(0f, 360f)] float startAngle;
    [SerializeField] float offsetAngle = 30f;

    protected override void OnEnable()
    {
        base.OnEnable();
        CalculateRadial();
    }

    public override void SetLayoutHorizontal()
    {
    }

    public override void SetLayoutVertical()
    {
    }

    public override void CalculateLayoutInputVertical() => CalculateRadial();
    public override void CalculateLayoutInputHorizontal() => CalculateRadial();

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        CalculateRadial();
    }
#endif

    void CalculateRadial()
    {
        m_Tracker.Clear();
        var childCount = transform.childCount;
        var activeChildCount = 0;

        for (var i = 0; i < childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeSelf)
                activeChildCount++;
        }

        if (activeChildCount == 0) return;

        var angle = NormalizeAngle(startAngle - offsetAngle * (activeChildCount - 1) * 0.5f);

        for (var i = childCount - 1; i >= 0; i--)
        {
            if (!transform.GetChild(i).gameObject.activeSelf) continue;

            var child = (RectTransform)transform.GetChild(i);
            if (child == null) continue;

            m_Tracker.Add(this, child,
                DrivenTransformProperties.Anchors |
                DrivenTransformProperties.AnchoredPosition |
                DrivenTransformProperties.Pivot);

            var rad = angle * Mathf.Deg2Rad;
            Vector3 position = new(Mathf.Cos(rad), Mathf.Sin(rad), 0);
            child.localPosition = position * distance;
            child.anchorMin = child.anchorMax = child.pivot = new Vector2(0.5f, 0.5f);

            angle += offsetAngle;
        }
    }

    float NormalizeAngle(float angle)
    {
        angle %= 360f;
        return angle < 0f ? angle + 360f : angle;
    }
}