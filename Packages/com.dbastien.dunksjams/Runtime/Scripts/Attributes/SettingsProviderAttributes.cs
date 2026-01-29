using System;

[AttributeUsage(AttributeTargets.Class)]
public class SettingsProviderSectionAttribute : Attribute
{
    public string Path { get; } // e.g. - "Project/‽/Console"
    public string Label { get; } // e.g - "Console"

    public SettingsProviderSectionAttribute(string path, string label)
    {
        Path = path;
        Label = label;
    }
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SettingsProviderFieldAttribute : Attribute
{
    public string Label { get; }

    public SettingsProviderFieldAttribute(string label) => Label = label;
}