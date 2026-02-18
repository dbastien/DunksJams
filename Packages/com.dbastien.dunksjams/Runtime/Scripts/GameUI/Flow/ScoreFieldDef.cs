public readonly struct ScoreFieldDef
{
    public string Id { get; }
    public string Label { get; }
    public int InitialValue { get; }

    public ScoreFieldDef(string id, string label, int initialValue = 0)
    {
        Id = id;
        Label = label;
        InitialValue = initialValue;
    }

    public override string ToString() => $"{Label} ({Id})";
}