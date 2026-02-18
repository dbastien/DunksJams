using UnityEngine;
using UnityEngine.UI;

public class GameOverScreen : MenuScreenBase
{
    private Text _summaryText;

    public GameOverScreen(Transform canvas) : base(canvas) { }

    public override void Setup()
    {
        base.Setup();

        GameObject summaryObj = UIBuilder.CreateUIElement("SummaryText", Panel.transform, typeof(Text));
        UIBuilder.SetupRectTransform(summaryObj.GetComponent<RectTransform>(), new Vector2(600, 140),
            new Vector2(0, 120));
        _summaryText = UIBuilder.InitText(summaryObj, "Game Over", Resources.GetBuiltinResource<Font>("Arial.ttf"),
            Color.white);
    }

    protected override void OnShow()
    {
        GameFlowManager flow = GameFlowManager.Instance;
        GameDefinition def = flow?.ActiveDefinition;
        _summaryText.text = def?.BuildGameOverSummary(flow) ?? "Game Over";
        RebuildMenu(def?.EndScreen ?? ScreenSpec.DefaultEnd(), "Game Over");
        _summaryText.transform.SetAsLastSibling();
    }
}