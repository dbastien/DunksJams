using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class OptionSpinner : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI valueLabel;
    [SerializeField] Button buttonNext;
    [SerializeField] Button buttonPrevious;

    public class ValueChangedEvent : UnityEvent<int, string> { }
    public ValueChangedEvent onValueChanged = new();

    readonly List<string> options = new();
    int valueIndex;

    public void AddOption(string option) => options.Add(option);
    public void AddOptions(List<string> newOptions) => options.AddRange(newOptions);

    public void ClearOptions()
    {
        options.Clear();
        valueIndex = 0;
    }

    public void SelectOption(int index) => ValueIndex = index;

    public void SelectOption(string option)
    {
        if (options.Contains(option))
            ValueIndex = options.FindIndex(item => item == option);
    }

    void RefreshView() => valueLabel.text = options.Count > 0 ? options[valueIndex] : "N/A";

    public string Value
    {
        get => options.Count > 0 ? options[valueIndex] : "";
        set => SelectOption(value);
    }

    public int ValueIndex
    {
        get => valueIndex;
        set
        {
            if (options.Count == 0) return;

            int newValue = (value + options.Count) % options.Count; // wrap around
            if (valueIndex != newValue)
            {
                valueIndex = newValue;
                onValueChanged.Invoke(newValue, Value);
            }
            RefreshView();
        }
    }

    void Awake()
    {
        if (buttonNext != null) buttonNext.onClick.AddListener(OnNextValue);
        if (buttonPrevious != null) buttonPrevious.onClick.AddListener(OnPreviousValue);
    }

    void Start() => RefreshView();

    void OnDestroy()
    {
        if (buttonNext != null) buttonNext.onClick.RemoveListener(OnNextValue);
        if (buttonPrevious != null) buttonPrevious.onClick.RemoveListener(OnPreviousValue);
    }

    void OnNextValue() => ++ValueIndex;
    void OnPreviousValue() => --ValueIndex;
}
