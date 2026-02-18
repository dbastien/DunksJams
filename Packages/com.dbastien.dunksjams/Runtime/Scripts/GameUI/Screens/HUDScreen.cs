using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDScreen : UIScreen
{
    private GameObject _hudRoot;
    private Text[] _scoreTexts;
    private string[] _scoreIds;
    private string[] _scoreLabels;

    public HUDScreen(Transform canvas) : base(canvas) { }

    public override void Setup()
    {
        var image = Panel.GetComponent<Image>();
        if (image != null) image.color = new Color(0f, 0f, 0f, 0f);
    }

    protected override void OnShow()
    {
        BuildHud();
        EventManager.AddListener<ScoreChangedEvent>(OnScoreChanged);
    }

    protected override void OnHide() => EventManager.RemoveListener<ScoreChangedEvent>(OnScoreChanged);

    public override void Destroy()
    {
        EventManager.RemoveListener<ScoreChangedEvent>(OnScoreChanged);
        base.Destroy();
    }

    private void BuildHud()
    {
        if (_hudRoot != null)
        {
            Object.Destroy(_hudRoot);
            _hudRoot = null;
        }

        GameFlowManager flow = GameFlowManager.Instance;
        ScoreboardSpec spec = flow?.ActiveDefinition?.Scoreboard;
        if (spec == null || spec.Count == 0) return;

        IReadOnlyList<ScoreFieldDef> fields = spec.Fields;
        _scoreTexts = new Text[fields.Count];
        _scoreIds = new string[fields.Count];
        _scoreLabels = new string[fields.Count];

        _hudRoot = UIBuilder.CreatePanel(Panel.transform, "HUDPanel", color: new Color(0f, 0f, 0f, 0f));
        AnchorTopLeft(_hudRoot.GetComponent<RectTransform>(), new Vector2(300, 200), new Vector2(20, -20));

        LayoutGroup layout = UIBuilder.CreateLayout(_hudRoot.transform, true, 6, TextAnchor.UpperLeft);
        if (layout is VerticalLayoutGroup vLayout) vLayout.childAlignment = TextAnchor.UpperLeft;
        var layoutRt = layout.GetComponent<RectTransform>();
        layoutRt.anchorMin = Vector2.zero;
        layoutRt.anchorMax = Vector2.one;
        layoutRt.offsetMin = layoutRt.offsetMax = Vector2.zero;

        for (var i = 0; i < fields.Count; ++i)
        {
            ScoreFieldDef field = fields[i];
            _scoreIds[i] = field.Id;
            _scoreLabels[i] = field.Label;

            GameObject textObj = UIBuilder.CreateUIElement(field.Label, layout.transform, typeof(Text));
            _scoreTexts[i] = UIBuilder.InitText(textObj, $"{field.Label}: {flow.GetScore(field.Id)}",
                Resources.GetBuiltinResource<Font>("Arial.ttf"), Color.white, TextAnchor.UpperLeft);
        }
    }

    private void OnScoreChanged(ScoreChangedEvent e)
    {
        if (_scoreIds == null || _scoreTexts == null) return;

        for (var i = 0; i < _scoreIds.Length; ++i)
        {
            if (_scoreIds[i] != e.Id) continue;
            _scoreTexts[i].text = $"{_scoreLabels[i]}: {e.Value}";
            return;
        }
    }

    private static void AnchorTopLeft(RectTransform rt, Vector2 size, Vector2 offset)
    {
        if (rt == null) return;
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.sizeDelta = size;
        rt.anchoredPosition = offset;
    }
}