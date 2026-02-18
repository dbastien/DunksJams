using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class ConfigOptionDef
{
    protected static readonly Font DefaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

    public string Label { get; }

    protected ConfigOptionDef(string label) => Label = label;

    public abstract void BuildUI(Transform parent);

    protected static GameObject CreateOptionRoot(Transform parent, string name)
    {
        GameObject root = UIBuilder.CreateUIElement(name, parent, typeof(RectTransform), typeof(VerticalLayoutGroup));
        var layout = root.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 4;
        layout.childAlignment = TextAnchor.MiddleCenter;
        return root;
    }

    protected static Text CreateLabel(Transform parent, string text)
    {
        GameObject labelObj = UIBuilder.CreateUIElement("Label", parent, typeof(Text));
        return UIBuilder.InitText(labelObj, text, DefaultFont, Color.white);
    }

    protected static void RaiseConfigChanged(string key, string value) =>
        EventManager.QueueEvent<GameConfigChangedEvent>(e =>
        {
            e.Key = key;
            e.Value = value;
        });
}

public sealed class IntSliderOption : ConfigOptionDef
{
    private readonly Func<int> _get;
    private readonly Action<int> _set;
    private readonly int _min;
    private readonly int _max;
    private readonly string _format;

    public IntSliderOption(string label, Func<int> get, Action<int> set, int min, int max, string format = "0")
        : base(label)
    {
        _get = get;
        _set = set;
        _min = min;
        _max = max;
        _format = format;
    }

    public override void BuildUI(Transform parent)
    {
        GameObject root = CreateOptionRoot(parent, $"{Label}_Option");
        Text label = CreateLabel(root.transform, BuildLabel(_get()));

        Slider slider = UIBuilder.CreateSlider(root.transform, _min, _max, _get(), value =>
        {
            int intValue = Mathf.RoundToInt(value);
            if (intValue == _get()) return;
            _set(intValue);
            label.text = BuildLabel(intValue);
            RaiseConfigChanged(Label, intValue.ToString());
        }, new Vector2(300, 20));

        slider.wholeNumbers = true;
    }

    private string BuildLabel(int value) => $"{Label}: {value.ToString(_format)}";
}

public sealed class FloatSliderOption : ConfigOptionDef
{
    private readonly Func<float> _get;
    private readonly Action<float> _set;
    private readonly float _min;
    private readonly float _max;
    private readonly string _format;

    public FloatSliderOption
    (
        string label, Func<float> get, Action<float> set, float min, float max,
        string format = "0.##"
    )
        : base(label)
    {
        _get = get;
        _set = set;
        _min = min;
        _max = max;
        _format = format;
    }

    public override void BuildUI(Transform parent)
    {
        GameObject root = CreateOptionRoot(parent, $"{Label}_Option");
        Text label = CreateLabel(root.transform, BuildLabel(_get()));

        UIBuilder.CreateSlider(root.transform, _min, _max, _get(), value =>
        {
            if (Mathf.Abs(value - _get()) < 0.0001f) return;
            _set(value);
            label.text = BuildLabel(value);
            RaiseConfigChanged(Label, value.ToString(_format));
        }, new Vector2(300, 20));
    }

    private string BuildLabel(float value) => $"{Label}: {value.ToString(_format)}";
}

public sealed class BoolOption : ConfigOptionDef
{
    private readonly Func<bool> _get;
    private readonly Action<bool> _set;

    public BoolOption(string label, Func<bool> get, Action<bool> set) : base(label)
    {
        _get = get;
        _set = set;
    }

    public override void BuildUI(Transform parent)
    {
        GameObject root = CreateOptionRoot(parent, $"{Label}_Option");
        Button button =
            UIBuilder.CreateButton(root.transform, BuildButtonText(_get()), null, size: new Vector2(220, 40));
        var text = button.GetComponentInChildren<Text>();

        button.onClick.AddListener(() =>
        {
            bool value = !_get();
            _set(value);
            if (text != null) text.text = BuildButtonText(value);
            RaiseConfigChanged(Label, value ? "On" : "Off");
        });
    }

    private string BuildButtonText(bool value) => $"{Label}: {(value ? "On" : "Off")}";
}