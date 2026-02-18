using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class OptionSpinner : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI valueLabel;
    [SerializeField] private Button buttonNext;
    [SerializeField] private Button buttonPrevious;

    public class ValueChangedEvent : UnityEvent<int, string> { }

    public ValueChangedEvent onValueChanged = new();

    private readonly List<string> options = new();
    private int valueIndex;

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

    private void RefreshView() => valueLabel.text = options.Count > 0 ? options[valueIndex] : "N/A";

    public string Value { get => options.Count > 0 ? options[valueIndex] : ""; set => SelectOption(value); }

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

    private void Awake()
    {
        if (buttonNext != null) buttonNext.onClick.AddListener(OnNextValue);
        if (buttonPrevious != null) buttonPrevious.onClick.AddListener(OnPreviousValue);
    }

    private void Start() => RefreshView();

    private void OnDestroy()
    {
        if (buttonNext != null) buttonNext.onClick.RemoveListener(OnNextValue);
        if (buttonPrevious != null) buttonPrevious.onClick.RemoveListener(OnPreviousValue);
    }

    private void OnNextValue() => ++ValueIndex;
    private void OnPreviousValue() => --ValueIndex;
}