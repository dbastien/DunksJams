using UnityEngine;
using UnityEngine.UI;

public class ConfigScreen : MenuScreenBase
{
    GameObject _optionsRoot;
    GameObject _footerRoot;
    Text _emptyText;

    public ConfigScreen(Transform canvas) : base(canvas)
    {
    }

    public override void Setup()
    {
        base.Setup();
        _optionsRoot = CreateOptionsRoot();
        _footerRoot = CreateFooterRoot();
        _emptyText = CreateEmptyText();
    }

    protected override void OnShow()
    {
        BuildOptions();
        var def = GameFlowManager.Instance?.ActiveDefinition;
        var spec = def?.ConfigScreen ?? ScreenSpec.DefaultConfig();
        if (TitleText != null)
            TitleText.text = string.IsNullOrWhiteSpace(spec.Title) ? "Options" : spec.Title;
        BuildFooterButtons(spec);
    }

    void BuildOptions()
    {
        _optionsRoot.transform.DestroyChildren();

        var options = GameFlowManager.Instance?.ActiveDefinition?.ConfigOptions;
        if (options == null || options.Count == 0)
        {
            _emptyText.gameObject.SetActive(true);
            return;
        }

        _emptyText.gameObject.SetActive(false);
        for (var i = 0; i < options.Count; ++i)
            options[i].BuildUI(_optionsRoot.transform);
    }

    GameObject CreateOptionsRoot()
    {
        var root = UIBuilder.CreateUIElement("OptionsRoot", Panel.transform, typeof(RectTransform),
            typeof(VerticalLayoutGroup));
        var rt = root.GetComponent<RectTransform>();
        UIBuilder.SetupRectTransform(rt, new Vector2(520, 300), new Vector2(0, 20));

        var layout = root.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 12;
        layout.childAlignment = TextAnchor.UpperCenter;
        return root;
    }

    GameObject CreateFooterRoot()
    {
        var root = UIBuilder.CreateUIElement("Footer", Panel.transform, typeof(RectTransform),
            typeof(HorizontalLayoutGroup));
        var rt = root.GetComponent<RectTransform>();
        UIBuilder.SetupRectTransform(rt, new Vector2(520, 60), new Vector2(0, -220));

        var layout = root.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 12;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        return root;
    }

    Text CreateEmptyText()
    {
        var emptyObj = UIBuilder.CreateUIElement("EmptyText", Panel.transform, typeof(Text));
        UIBuilder.SetupRectTransform(emptyObj.GetComponent<RectTransform>(), new Vector2(520, 60),
            new Vector2(0, 20));
        return UIBuilder.InitText(emptyObj, "No options available.", Resources.GetBuiltinResource<Font>("Arial.ttf"),
            Color.white);
    }

    void BuildFooterButtons(ScreenSpec spec)
    {
        _footerRoot.transform.DestroyChildren();
        if (spec?.Buttons == null || spec.Buttons.Count == 0) return;

        for (var i = 0; i < spec.Buttons.Count; ++i)
        {
            var buttonDef = spec.Buttons[i];
            var localDef = buttonDef;
            UIBuilder.CreateButton(_footerRoot.transform, localDef.Label,
                () => localDef.Execute(GameFlowManager.Instance), size: new Vector2(200, 40));
        }
    }
}