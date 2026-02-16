using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class MenuScreenBase : UIScreen
{
    protected Text TitleText { get; private set; }
    GameObject _menu;

    protected MenuScreenBase(Transform canvas) : base(canvas)
    {
    }

    public override void Setup() => TitleText = CreateTitleText();

    protected void RebuildMenu(ScreenSpec spec, string fallbackTitle = "Menu")
    {
        if (TitleText == null) return;

        var title = spec != null && !string.IsNullOrWhiteSpace(spec.Title) ? spec.Title : fallbackTitle;
        TitleText.text = title;

        if (_menu != null)
        {
            Object.Destroy(_menu);
            _menu = null;
        }

        if (spec?.Buttons == null || spec.Buttons.Count == 0) return;

        var labels = new string[spec.Buttons.Count];
        var actions = new UnityAction[spec.Buttons.Count];

        for (var i = 0; i < spec.Buttons.Count; ++i)
        {
            var buttonDef = spec.Buttons[i];
            labels[i] = buttonDef.Label;
            var localDef = buttonDef; // capture per-iteration value for the lambda
            actions[i] = () => localDef.Execute(GameFlowManager.Instance);
        }

        _menu = UIComponents.CreateMenu(Panel.transform, labels, actions);
        TitleText.transform.SetAsLastSibling();
    }

    Text CreateTitleText()
    {
        var titleObj = UIBuilder.CreateUIElement("Title", Panel.transform, typeof(Text));
        var rt = titleObj.GetComponent<RectTransform>();
        UIBuilder.SetupRectTransform(rt, new Vector2(600, 80), new Vector2(0, 220));
        return UIBuilder.InitText(titleObj, "Menu", Resources.GetBuiltinResource<Font>("Arial.ttf"), Color.white);
    }
}