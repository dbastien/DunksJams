using System;

public enum FieldType
{
    Text,
    Number,
    Boolean,
    Object,
    Localization
}

[Serializable]
public class Field
{
    public string name;
    public FieldType type;
    public string value;
    public UnityEngine.Object objValue;

    public Field() { }

    public Field(string name, string value, FieldType type = FieldType.Text)
    {
        this.name = name;
        this.value = value;
        this.type = type;
    }

    public int AsInt() => int.TryParse(value, out var i) ? i : 0;
    public float AsFloat() => float.TryParse(value, out var f) ? f : 0f;
    public bool AsBool() => bool.TryParse(value, out var b) && b;
}