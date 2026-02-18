using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfigScreen : MenuScreenBase
{
    private GameObject _optionsRoot;
    private GameObject _footerRoot;
    private Text _emptyText;

    public ConfigScreen(Transform canvas) : base(canvas) { }

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
        GameDefinition def = GameFlowManager.Instance?.ActiveDefinition;
        ScreenSpec spec = def?.ConfigScreen ?? ScreenSpec.DefaultConfig();
        if (TitleText != null)
            TitleText.text = string.IsNullOrWhiteSpace(spec.Title) ? "Options" : spec.Title;
        BuildFooterButtons(spec);
    }

    private void BuildOptions()
    {
        _optionsRoot.transform.DestroyChildren();

        IReadOnlyList<ConfigOptionDef> options = GameFlowManager.Instance?.ActiveDefinition?.ConfigOptions;
        if (options == null || options.Count == 0)
        {
            _emptyText.gameObject.SetActive(true);
            return;
        }

        _emptyText.gameObject.SetActive(false);
        for (var i = 0; i < options.Count; ++i)
            options[i].BuildUI(_optionsRoot.transform);
    }

    private GameObject CreateOptionsRoot()
    {
        GameObject root = UIBuilder.CreateUIElement("OptionsRoot", Panel.transform, typeof(RectTransform),
            typeof(VerticalLayoutGroup));
        var rt = root.GetComponent<RectTransform>();
        UIBuilder.SetupRectTransform(rt, new Vector2(520, 300), new Vector2(0, 20));

        var layout = root.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 12;
        layout.childAlignment = TextAnchor.UpperCenter;
        return root;
    }

    private GameObject CreateFooterRoot()
    {
        GameObject root = UIBuilder.CreateUIElement("Footer", Panel.transform, typeof(RectTransform),
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

    private Text CreateEmptyText()
    {
        GameObject emptyObj = UIBuilder.CreateUIElement("EmptyText", Panel.transform, typeof(Text));
        UIBuilder.SetupRectTransform(emptyObj.GetComponent<RectTransform>(), new Vector2(520, 60),
            new Vector2(0, 20));
        return UIBuilder.InitText(emptyObj, "No options available.", Resources.GetBuiltinResource<Font>("Arial.ttf"),
            Color.white);
    }

    private void BuildFooterButtons(ScreenSpec spec)
    {
        _footerRoot.transform.DestroyChildren();
        if (spec?.Buttons == null || spec.Buttons.Count == 0) return;

        for (var i = 0; i < spec.Buttons.Count; ++i)
        {
            ScreenButtonDef buttonDef = spec.Buttons[i];
            ScreenButtonDef localDef = buttonDef;
            UIBuilder.CreateButton(_footerRoot.transform, localDef.Label,
                () => localDef.Execute(GameFlowManager.Instance), size: new Vector2(200, 40));
        }
    }
}