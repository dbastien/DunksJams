using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class OpenLocalFileAttribute : PropertyAttribute { }

[AttributeUsage(AttributeTargets.Field)]
public class OpenLocalFolderAttribute : PropertyAttribute { }

[AttributeUsage(AttributeTargets.Field)]
public class SaveLocalFileAttribute : PropertyAttribute { }

[AttributeUsage(AttributeTargets.Method)]
public class ButtonAttribute : PropertyAttribute { }

[AttributeUsage(AttributeTargets.Method)]
public class ExposeMethodInEditorAttribute : PropertyAttribute { }

[AttributeUsage(AttributeTargets.Field)]
public class RequiredAttribute : PropertyAttribute { }

[AttributeUsage(AttributeTargets.Field)]
public class ProgressBarAttribute : PropertyAttribute
{
    public float Max { get; }
    public string Label { get; }

    public ProgressBarAttribute(float max, string label = null) =>
        (Max, Label) = (max, label);
}

[AttributeUsage(AttributeTargets.Field)]
public class HeaderColorAttribute : PropertyAttribute
{
    public Color Color { get; }
    public string Header { get; }

    public HeaderColorAttribute(float r, float g, float b, string header = "") =>
        (Color, Header) = (new Color(r, g, b), header);
}

[AttributeUsage(AttributeTargets.Field)]
public class MinMaxSliderAttribute : PropertyAttribute
{
    public float Min { get; }
    public float Max { get; }

    public MinMaxSliderAttribute(float min, float max) => (Min, Max) = (min, max);
}