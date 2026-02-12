using System;

/// <summary>Toolset provider attribute. Use on classes implementing IToolset to add them to the toolbox.</summary>
public sealed class ToolsetProvider : Attribute
{
    public string displayName { get; set; }
    public string description { get; set; }
}
