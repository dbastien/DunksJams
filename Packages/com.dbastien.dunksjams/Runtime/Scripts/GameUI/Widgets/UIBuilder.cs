using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class UIBuilder
{
    static readonly Font defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
    static readonly Color colWhite = Color.white;
    static readonly Color colGray = new(0.5f, 0.5f, 0.5f);

    public static GameObject CreateCanvas(RenderMode mode = RenderMode.ScreenSpaceOverlay)
    {
        var canvas = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas.FindOrAddComponent<Canvas>().renderMode = mode;
        canvas.FindOrAddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        GameObjectUtils.FindOrCreate("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        return canvas;
    }

    public static GameObject CreateUIElement(string name, Transform parent, params Type[] components) =>
        new(name, components) { transform = { parent = parent } };

    public static GameObject CreatePanel(Transform parent, string name = "Panel", Vector2? size = null, Color? color = null)
    {
        var panel = CreateUIElement(name, parent, typeof(Image));
        SetupRectTransform(panel.FindOrAddComponent<RectTransform>(), size ?? new Vector2(800, 600));
        panel.FindOrAddComponent<Image>().color = color ?? new Color(0, 0, 0, 0.5f);
        return panel;
    }

    public static LayoutGroup CreateLayout(Transform parent, bool vertical = true, int spacing = 10, TextAnchor alignment = TextAnchor.MiddleCenter)
    {
        var layout = CreateUIElement("Layout", parent, typeof(RectTransform));
        LayoutGroup group = vertical
            ? layout.FindOrAddComponent<VerticalLayoutGroup>()
            : layout.FindOrAddComponent<HorizontalLayoutGroup>();

        group.childAlignment = alignment;

        if (group is HorizontalOrVerticalLayoutGroup hvGroup)
            hvGroup.spacing = spacing;

        return group;
    }

    public static Button CreateButton(Transform parent, string text, UnityAction onClick = null, Color? color = null, Font font = null, Vector2? size = null, Vector2? position = null)
    {
        var buttonObj = CreateUIElement(text, parent, typeof(Button), typeof(Image));
        var button = buttonObj.FindOrAddComponent<Button>();
        if (onClick != null) button.onClick.AddListener(onClick);

        var rectTransform = buttonObj.FindOrAddComponent<RectTransform>();
        SetupRectTransform(rectTransform, size ?? new Vector2(160, 40), position);

        InitText(CreateUIElement("Text", buttonObj.transform, typeof(Text)), text, font ?? defaultFont, color ?? colWhite);
        return button;
    }

    public static Slider CreateSlider(Transform parent, float min, float max, float value, UnityAction<float> onValChanged, Vector2? size = null)
    {
        var slider = CreateUIElement("Slider", parent, typeof(Slider)).FindOrAddComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = value;
        if (onValChanged != null) slider.onValueChanged.AddListener(onValChanged);

        SetupRectTransform(slider.GetComponent<RectTransform>(), size ?? new Vector2(200, 20));
        SetupSliderFill(slider);
        return slider;
    }

    public static InputField CreateInputField(Transform parent, string placeholder, UnityAction<string> onValChanged, Font font = null, Vector2? size = null)
    {
        var inputField = CreateUIElement("InputField", parent, typeof(InputField), typeof(Image)).FindOrAddComponent<InputField>();
        inputField.placeholder = InitText(CreateUIElement("Placeholder", inputField.transform, typeof(Text)), placeholder, font ?? defaultFont, colGray);
        inputField.textComponent = InitText(CreateUIElement("Text", inputField.transform, typeof(Text)), "", font ?? defaultFont, colWhite);
        if (onValChanged != null) inputField.onValueChanged.AddListener(onValChanged);

        SetupRectTransform(inputField.GetComponent<RectTransform>(), size ?? new Vector2(200, 40));
        return inputField;
    }

    public static Dropdown CreateDropdown(Transform parent, string[] options, UnityAction<int> onValChanged, Font font = null, Vector2? size = null)
    {
        var dropdown = CreateUIElement("Dropdown", parent, typeof(Dropdown), typeof(Image)).FindOrAddComponent<Dropdown>();
        dropdown.options.AddRange(Array.ConvertAll(options, o => new Dropdown.OptionData(o)));
        dropdown.captionText = InitText(CreateUIElement("Label", dropdown.transform, typeof(Text)), options.Length > 0 ? options[0] : "", font ?? defaultFont, colWhite);
        if (onValChanged != null) dropdown.onValueChanged.AddListener(onValChanged);

        SetupRectTransform(dropdown.GetComponent<RectTransform>(), size ?? new Vector2(160, 40));
        return dropdown;
    }

    public static ScrollRect CreateScrollView(Transform parent, Vector2 size)
    {
        var scrollView = CreateUIElement("ScrollView", parent, typeof(ScrollRect)).FindOrAddComponent<ScrollRect>();
        SetupRectTransform(scrollView.GetComponent<RectTransform>(), size);

        var viewport = CreateUIElement("Viewport", scrollView.transform, typeof(RectTransform), typeof(Mask), typeof(Image));
        scrollView.viewport = viewport.GetComponent<RectTransform>();

        var content = CreateUIElement("Content", viewport.transform, typeof(RectTransform));
        scrollView.content = content.GetComponent<RectTransform>();

        return scrollView;
    }

    public static Text InitText(GameObject go, string content, Font font, Color color, TextAnchor alignment = TextAnchor.MiddleCenter)
    {
        var text = go.FindOrAddComponent<Text>();
        text.text = content;
        text.font = font;
        text.color = color;
        text.alignment = alignment;
        return text;
    }

    public static void SetupRectTransform(RectTransform rt, Vector2 size, Vector2? anchoredPos = null)
    {
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPos ?? Vector2.zero;
    }

    public static void SetupSliderFill(Slider slider, Color? fillColor = null)
    {
        var fillArea = CreateUIElement("FillArea", slider.transform, typeof(RectTransform));
        var fill = CreateUIElement("Fill", fillArea.transform, typeof(Image));
        fill.GetComponent<Image>().color = fillColor ?? Color.green;
        slider.fillRect = fill.GetComponent<RectTransform>();
    }
}
