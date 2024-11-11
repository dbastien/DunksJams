using UnityEngine;

public class ToggleHeaderAttribute : PropertyAttribute
{
    public string Header { get; }
    public string ToggleField { get; }

    public ToggleHeaderAttribute(string toggleField, string header) =>
        (ToggleField, Header) = (toggleField, header);
}